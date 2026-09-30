using RabbitMQ.Client;

namespace ImageProcessingServiceAPI.Application;

public static class PublisherBoilerplate {
    public static async Task Initialize(IChannel channel, string mainExchange, string deadLetterExchange, string routingKey, byte[]? body, BasicProperties props) {
        await channel.ExchangeDeclareAsync(
            exchange: mainExchange,
            type: ExchangeType.Topic,
            durable: true
        );

        await channel.ExchangeDeclareAsync(
            exchange: deadLetterExchange,
            type: ExchangeType.Topic,
            durable: true
        );

        await channel.BasicPublishAsync(
            exchange: mainExchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: props,
            body: body
        );
    }
}