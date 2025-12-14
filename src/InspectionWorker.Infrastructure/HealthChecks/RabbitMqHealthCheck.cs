using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace InspectionWorker.Infrastructure.HealthChecks;

public class RabbitMqHealthCheck : IHealthCheck
{
    private readonly string _hostName;
    private readonly int _port;
    private readonly string _userName;
    private readonly string _password;
    private readonly string _virtualHost;

    public RabbitMqHealthCheck(string hostName, int port, string userName, string password, string virtualHost)
    {
        _hostName = hostName;
        _port = port;
        _userName = userName;
        _password = password;
        _virtualHost = virtualHost;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _hostName,
                Port = _port,
                UserName = _userName,
                Password = _password,
                VirtualHost = _virtualHost
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            await Task.CompletedTask;
            return HealthCheckResult.Healthy("RabbitMQ connection is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("RabbitMQ connection failed", ex);
        }
    }
}
