namespace InspectionWorker.Infrastructure.Configuration;

public class RabbitMqSettings
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string InspectionRequestQueue { get; set; } = "inspection-requests";
    public string InspectionResultQueue { get; set; } = "inspection-results";
    public string DeadLetterExchange { get; set; } = "inspection-dlx";
    public string DeadLetterQueue { get; set; } = "inspection-dlq";
    public int PrefetchCount { get; set; } = 10;
    public int MaxRetryAttempts { get; set; } = 3;
}
