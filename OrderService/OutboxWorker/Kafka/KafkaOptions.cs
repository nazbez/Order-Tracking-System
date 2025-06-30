namespace OutboxWorker.Kafka;

public sealed class KafkaOptions
{
    public static string KafkaSectionName => "Kafka";

    public string BootstrapServers { get; init; } = string.Empty;
    public string Topic { get; init; } = string.Empty;
    public string SaslUsername { get; init; } = string.Empty;
    public string SaslPassword { get; init; } = string.Empty;
}
