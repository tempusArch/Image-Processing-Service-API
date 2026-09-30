using RabbitMQ.Client;

namespace ImageProcessingServiceAPI.Application;

public static class WorkerBoilerplate {
    public static async Task Initialize(IChannel channel, string mainExchange, string deadLetterExchange, string queueName, string routingKey) {
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

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?> {
                ["x-dead-letter-exchange"] = deadLetterExchange,
                ["x-dead-letter-routing-key"] = "image.dlq"
            }
        );

        await channel.QueueDeclareAsync(
            queue: "image.deadletter.queue",
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        await channel.QueueBindAsync(
            queue: queueName,
            exchange: mainExchange,
            routingKey: routingKey
        );

        await channel.QueueBindAsync(
            queue: "image.deadletter.queue",
            exchange: deadLetterExchange,
            routingKey: "image.dlq"
        );

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false
        );
    }
}