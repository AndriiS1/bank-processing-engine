using Confluent.Kafka;
using Domain.Abstractions;
using Microsoft.Extensions.Configuration;
namespace Infrastructure.Services;

public class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;

    public KafkaProducer(IConfiguration configuration)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            Acks = Acks.All,
            MessageSendMaxRetries = 5,
            EnableIdempotence = true
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task ProduceAsync(string topic, string key, string value, CancellationToken ct)
    {
        var message = new Message<string, string> { Key = key, Value = value };
        await _producer.ProduceAsync(topic, message, ct);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
        GC.SuppressFinalize(this);
    }
}
