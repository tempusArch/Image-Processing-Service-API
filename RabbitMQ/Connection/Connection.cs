using RabbitMQ.Client;

namespace ImageProcessingServiceAPI.Application;

public interface IRabbitMqConnection : IAsyncDisposable {
    Task<IConnection> GetConnectionAsync();
}

public class RabbitMqConnection : IRabbitMqConnection {
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public RabbitMqConnection(IConfiguration config) {
        _factory = new ConnectionFactory {
            HostName = config["RabbitMQ:Host"],
            Port = int.Parse(config["RabbitMQ:Port"]),
            UserName = config["RabbitMQ:Username"],
            Password = config["RabbitMQ:Password"],
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };
    }

    public async Task<IConnection> GetConnectionAsync() {
        if (_connection is { IsOpen: true })
            return _connection;

        await _lock.WaitAsync();

        try {
            if (_connection is not { IsOpen: true })
                _connection = await _factory.CreateConnectionAsync();

            return _connection;
        } finally {
            _lock.Release();
        }
    }

    public async ValueTask DisposeAsync() {
        if (_connection != null)
            await _connection.DisposeAsync();
    }
}