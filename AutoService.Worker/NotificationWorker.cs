using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace AutoService.Worker
{
    public class NotificationWorker : BackgroundService
    {
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationWorker> _logger;
        private DateTime _lastMaintenanceCheck = DateTime.MinValue;

        public NotificationWorker(
            IHttpClientFactory factory,
            IConfiguration configuration,
            ILogger<NotificationWorker> logger)
        {
            _factory = factory;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(15));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckNotifications(stoppingToken);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Notification check failed");
                }

                try
                {
                    if (!await timer.WaitForNextTickAsync(stoppingToken))
                        break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task CheckNotifications(CancellationToken ct)
        {
            var email = _configuration["Worker:Email"];
            var password = _configuration["Worker:Password"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("Set Worker:Email and Worker:Password in local secrets or environment variables");
                return;
            }

            var url = _configuration["Api:BaseUrl"] ?? "http://localhost:5260";
            var baseAddress = new Uri(url.TrimEnd('/') + "/");

            if (baseAddress.Scheme != Uri.UriSchemeHttps && !baseAddress.IsLoopback)
                throw new InvalidOperationException("The Worker requires HTTPS for non-local APIs");

            using var client = _factory.CreateClient();
            client.BaseAddress = baseAddress;

            using var login = await client.PostAsJsonAsync(
                "api/auth/login", new { Email = email, Password = password }, ct);
            login.EnsureSuccessStatusCode();

            using var document = await JsonDocument.ParseAsync(
                await login.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            string token = document.RootElement.GetProperty("token").GetString()
                ?? throw new InvalidOperationException("API did not return a JWT");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var ready = await client.GetFromJsonAsync<List<Item>>(
                "api/notifications/pending-ready", ct) ?? new();

            foreach (var order in ready)
            {
                using var response = await client.PostAsync(
                    $"api/notifications/order-ready/{order.Id}", null, ct);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Ready order notification processed: {OrderId}", order.Id);
            }

            if (_lastMaintenanceCheck.Date == DateTime.UtcNow.Date)
                return;

            var due = await client.GetFromJsonAsync<List<Item>>(
                "api/cars/service-due?days=14", ct) ?? new();

            foreach (var car in due)
            {
                using var response = await client.PostAsync(
                    $"api/notifications/maintenance/{car.Id}", null, ct);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Maintenance notification processed: {CarId}", car.Id);
            }

            _lastMaintenanceCheck = DateTime.UtcNow.Date;
        }

        private sealed class Item
        {
            public int Id { get; set; }
        }
    }
}
