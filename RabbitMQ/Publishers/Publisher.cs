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

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var props = new BasicProperties {
            Persistent = true
        };

        await PublisherBoilerplate.Initialize(
            channel,
            _config["RabbitMQ:Exchange"],
            _config["RabbitMQ:DeadLetterExchange"],
            "image.resize",
            body,
            props
        );
    }

    public async Task CropPublishAsync(CropMessage message) {
        var connection = await _rabbitConnection.GetConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var props = new BasicProperties {
            Persistent = true
        };

        await PublisherBoilerplate.Initialize(
            channel,
            _config["RabbitMQ:Exchange"],
            _config["RabbitMQ:DeadLetterExchange"],
            "image.crop",
            body,
            props
        );
    }

    public async Task RotatePublishAsync(RotateMessage message) {
        var connection = await _rabbitConnection.GetConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var props = new BasicProperties {
            Persistent = true
        };

        await PublisherBoilerplate.Initialize(
            channel,
            _config["RabbitMQ:Exchange"],
            _config["RabbitMQ:DeadLetterExchange"],
            "image.rotate",
            body,
            props
        );
    }

    public async Task WatermarkPublishAsync(WatermarkMessage message) {
        var connection = await _rabbitConnection.GetConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var props = new BasicProperties {
            Persistent = true
        };

        await PublisherBoilerplate.Initialize(
            channel,
            _config["RabbitMQ:Exchange"],
            _config["RabbitMQ:DeadLetterExchange"],
            "image.watermark",
            body,
            props
        );
    }

    public async Task FlipPublishAsync(FlipMessage message) {
        var connection = await _rabbitConnection.GetConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var props = new BasicProperties {
            Persistent = true
        };

        await PublisherBoilerplate.Initialize(
            channel,
            _config["RabbitMQ:Exchange"],
            _config["RabbitMQ:DeadLetterExchange"],
            "image.flip",
            body,
            props
        );
    }

    public async Task MirrorPublishAsync(MirrorMessage message) {
        var connection = await _rabbitConnection.GetConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var props = new BasicProperties {
            Persistent = true
        };
        await PublisherBoilerplate.Initialize(
            channel,
            _config["RabbitMQ:Exchange"],
            _config["RabbitMQ:DeadLetterExchange"],
            "image.mirror",
            body,
            props
        );
    }

    public async Task CompressPublishAsync(CompressMessage message) {
        var connection = await _rabbitConnection.GetConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var props = new BasicProperties {
            Persistent = true
        };
        await PublisherBoilerplate.Initialize(
            channel,
            _config["RabbitMQ:Exchange"],
            _config["RabbitMQ:DeadLetterExchange"],
            "image.compress",
            body,
            props
        );
    }

    public async Task ChangeFormatPublishAsync(ChangeFormatMessage message) {
        var connection = await _rabbitConnection.GetConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var props = new BasicProperties {
            Persistent = true
        };
        await PublisherBoilerplate.Initialize(
            channel,
            _config["RabbitMQ:Exchange"],
            _config["RabbitMQ:DeadLetterExchange"],
            "image.changeformat",
            body,
            props
        );
    }

    public async Task FilterPublishAsync(FilterMessage message) {
        var connection = await _rabbitConnection.GetConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var props = new BasicProperties {
            Persistent = true
        };
        await PublisherBoilerplate.Initialize(
            channel,
            _config["RabbitMQ:Exchange"],
            _config["RabbitMQ:DeadLetterExchange"],
            "image.filter",
            body,
            props
        );
    }
}