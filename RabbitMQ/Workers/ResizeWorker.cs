using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ImageProcessingServiceAPI.Application;

public class ResizeWorker : BackgroundService {
    private readonly IConfiguration _config;
    private readonly IRabbitMqConnection _rabbitConnection;
    private readonly IServiceScopeFactory _scopeFactory;
    //private readonly ImageService _imageService;
    //private readonly LocalStorageService _localStorage;

    public ResizeWorker(IConfiguration configuration, IRabbitMqConnection rabbitMqConnection, IServiceScopeFactory scopeFactory/*ImageService imageService, LocalStorageService localStorageService*/) {
        _config = configuration;
        _rabbitConnection = rabbitMqConnection;
        _scopeFactory = scopeFactory;
        //_imageService = imageService;
        //_localStorage = localStorageService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken) {
        while (!cancellationToken.IsCancellationRequested) {
            try {
                var connection = await _rabbitConnection.GetConnectionAsync();
                await using var channel = await connection.CreateChannelAsync();

                await channel.ExchangeDeclareAsync(
                    exchange: _config["RabbitMQ:Exchange"],
                    type: ExchangeType.Topic,
                    durable: true
                );

                await channel.QueueDeclareAsync(
                    queue: "image.resize.queue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false
                );

                await channel.QueueBindAsync(
                    queue: "image.resize.queue",
                    exchange: _config["RabbitMQ:Exchange"],
                    routingKey: "image.resize"
                );

                await channel.BasicQosAsync(
                    prefetchSize: 0,
                    prefetchCount: 1,
                    global: false
                );

                var consumer = new AsyncEventingBasicConsumer(channel);

                consumer.ReceivedAsync += async(_, ea) => {
                    try {
                        using var scope = _scopeFactory.CreateScope();
                        var _imageService = scope.ServiceProvider.GetRequiredService<ImageService>();
                        var _localStorage = scope.ServiceProvider.GetRequiredService<LocalStorageService>();

                        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                        var message = JsonSerializer.Deserialize<ResizeMessage>(json);

                        if (message == null) {
                            await channel.BasicAckAsync(ea.DeliveryTag, false);
                            return;
                        }

                        var result = await _imageService.ResizeImage(message.UserId, message.ImageId, message.Width, message.Height, message.ResultName, cancellationToken);
                        await _localStorage.CompletedBytesSaveToLocal(result.Item1, result.Item2, cancellationToken);
                        await channel.BasicAckAsync(ea.DeliveryTag, false);

                    } catch (SerializationException ex) {
                        Console.WriteLine(ex);

                        await channel.BasicNackAsync(
                            ea.DeliveryTag,
                            false,
                            false
                        );
                
                    } catch (FileNotFoundException ex) {
                        Console.WriteLine(ex);

                        await channel.BasicNackAsync(
                            ea.DeliveryTag,
                            false,
                            false
                        );
                
                    } catch (Exception ex) {
                        Console.WriteLine(ex);

                        await channel.BasicNackAsync(
                            ea.DeliveryTag,
                            false,
                            false
                        );
                    } 
                };

                await channel.BasicConsumeAsync(
                    queue: "image.resize.queue",
                    autoAck: false,
                    consumer: consumer
                );

                await Task.Delay(Timeout.Infinite, cancellationToken);
            
            } catch (OperationCanceledException) {
                break;

            } catch (Exception ex) {
                Console.WriteLine($"{ex}, RabbitMQ connection error in ResizeWorker. Retrying in 5 seconds...");
                await Task.Delay(5000, cancellationToken);

            }
            
        }
    }
}