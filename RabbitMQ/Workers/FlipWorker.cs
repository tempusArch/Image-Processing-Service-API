using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ImageProcessingServiceApi.Application;

public class FlipWorker : BackgroundService {
    private readonly IConfiguration _config;
    private readonly IRabbitMqConnection _rabbitConnection;
    private IChannel? _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    //private readonly ImageService _imageService;
    //private readonly LocalStorageService _localStorage;

    public FlipWorker(IConfiguration configuration, IRabbitMqConnection rabbitMqConnection, IServiceScopeFactory scopeFactory/*ImageService imageService, LocalStorageService localStorageService*/) {
        _config = configuration;
        _rabbitConnection = rabbitMqConnection;
        _scopeFactory = scopeFactory;
        //_imageService = imageService;
        //_localStorage = localStorageService;
    }

    public async Task<IChannel> GetChannelAsync() {
        if (_channel?.IsOpen == true)
            return _channel;

        var connection = await _rabbitConnection.GetConnectionAsync();
        _channel = await connection.CreateChannelAsync();

        return _channel;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken) {
        while (!cancellationToken.IsCancellationRequested) {
            using var scope = _scopeFactory.CreateScope();
            var _imageService = scope.ServiceProvider.GetRequiredService<ImageService>();
            var _localStorage = scope.ServiceProvider.GetRequiredService<LocalStorageService>();
            
            await using var channel = await GetChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: _config["RabbitMQ:Exchange"],
                type: ExchangeType.Topic,
                durable: true
            );

            await channel.QueueDeclareAsync(
                queue: "image.flip.queue",
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            await channel.QueueBindAsync(
                queue: "image.flip.queue",
                exchange: _config["RabbitMQ:Exchange"],
                routingKey: "image.flip"
            );

            await channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false
            );

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async(_, ea) => {
                try {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var message = JsonSerializer.Deserialize<FlipMessage>(json);

                    if (message == null) {
                        await channel.BasicAckAsync(ea.DeliveryTag, false);
                        return;
                    }

                    var result = await _imageService.FlipImage(message.UserId, message.ImageId, message.flipMode, message.ResultName, cancellationToken);
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
                queue: "image.flip.queue",
                autoAck: false,
                consumer: consumer
            );

            await Task.Delay(Timeout.Infinite, cancellationToken);

            await Task.Delay(1000, cancellationToken);

        }
    }
}