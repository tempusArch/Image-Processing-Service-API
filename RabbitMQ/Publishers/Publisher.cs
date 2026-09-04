using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using RabbitMQ.Client;

namespace ImageProcessingServiceAPI.Application;

public class RabbitMqPublisher {
    private readonly IConfiguration _config;
    private readonly IRabbitMqConnection _rabbitConnection;
    public RabbitMqPublisher(IConfiguration configuration, IRabbitMqConnection rabbitMqConnection) {
        _config = configuration;
        _rabbitConnection = rabbitMqConnection;
    }

    public async Task ResizePublishAsync(ResizeMessage message) {
        var connection = await _rabbitConnection.GetConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: _config["RabbitMQ:Exchange"],
            type: ExchangeType.Topic,
            durable: true
        );

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var props = new BasicProperties {
            Persistent = true
        };

        await channel.BasicPublishAsync(
            exchange: _config["RabbitMQ:Exchange"],
            routingKey: "image.resize",
            mandatory: false,
            basicProperties: props,
            body: body
        );
    }

    public async Task CropPublishAsync(CropMessage message) {
        var connection = await _rabbitConnection.GetConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: _config["RabbitMQ:Exchange"],
            type: ExchangeType.Topic,
            durable: true
        );

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var props = new BasicProperties {
            Persistent = true
        };

        await channel.BasicPublishAsync(
            exchange: _config["RabbitMQ:Exchange"],
            routingKey: "image.crop",
            mandatory: false,
            basicProperties: props,
            body: body
        );
    }

    public async Task RotatePublishAsync(RotateMessage message) {
        var connection = await _rabbitConnection.GetConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: _config["RabbitMQ:Exchange"],
            type: ExchangeType.Topic,
            durable: true
        );

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var props = new BasicProperties {
            Persistent = true
        };

        await channel.BasicPublishAsync(
            exchange: _config["RabbitMQ:Exchange"],
            routingKey: "image.rotate",
            mandatory: false,
            basicProperties: props,
            body: body
        );
    }

    public async Task WatermarkPublishAsync(WatermarkMessage message) {
        var connection = await _rabbitConnection.GetConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: _config["RabbitMQ:Exchange"],
            type: ExchangeType.Topic,
            durable: true
        );

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var props = new BasicProperties {
            Persistent = true
        };

        await channel.BasicPublishAsync(
            exchange: _config["RabbitMQ:Exchange"],
            routingKey: "image.watermark",
            mandatory: false,
            basicProperties: props,
            body: body
        );
    }

    public async Task FlipPublishAsync(FlipMessage message) {
        var connection = await _rabbitConnection.GetConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: _config["RabbitMQ:Exchange"],
            type: ExchangeType.Topic,
            durable: true
        );

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var props = new BasicProperties {
            Persistent = true
        };

        await channel.BasicPublishAsync(
            exchange: _config["RabbitMQ:Exchange"],
            routingKey: "image.flip",
            mandatory: false,
            basicProperties: props,
            body: body
        );
    }

    public async Task MirrorPublishAsync(MirrorMessage message) {
        var connection = await _rabbitConnection.GetConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: _config["RabbitMQ:Exchange"],
            type: ExchangeType.Topic,
            durable: true
        );

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var props = new BasicProperties {
            Persistent = true
        };

        await channel.BasicPublishAsync(
            exchange: _config["RabbitMQ:Exchange"],
            routingKey: "image.mirror",
            mandatory: false,
            basicProperties: props,
            body: body
        );
    }

    public async Task CompressPublishAsync(CompressMessage message) {
        var connection = await _rabbitConnection.GetConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: _config["RabbitMQ:Exchange"],
            type: ExchangeType.Topic,
            durable: true
        );

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var props = new BasicProperties {
            Persistent = true
        };

        await channel.BasicPublishAsync(
            exchange: _config["RabbitMQ:Exchange"],
            routingKey: "image.compress",
            mandatory: false,
            basicProperties: props,
            body: body
        );
    }

    public async Task ChangeFormatPublishAsync(ChangeFormatMessage message) {
        var connection = await _rabbitConnection.GetConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: _config["RabbitMQ:Exchange"],
            type: ExchangeType.Topic,
            durable: true
        );

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var props = new BasicProperties {
            Persistent = true
        };

        await channel.BasicPublishAsync(
            exchange: _config["RabbitMQ:Exchange"],
            routingKey: "image.changeformat",
            mandatory: false,
            basicProperties: props,
            body: body
        );
    }

    public async Task FilterPublishAsync(FilterMessage message) {
        var connection = await _rabbitConnection.GetConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: _config["RabbitMQ:Exchange"],
            type: ExchangeType.Topic,
            durable: true
        );

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var props = new BasicProperties {
            Persistent = true
        };

        await channel.BasicPublishAsync(
            exchange: _config["RabbitMQ:Exchange"],
            routingKey: "image.filter",
            mandatory: false,
            basicProperties: props,
            body: body
        );
    }
}