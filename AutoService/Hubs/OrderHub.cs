using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AutoService.Hubs
{
    [Authorize(Roles = "Customer")]
    public class OrderHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var customerId = Context.User?.FindFirst("customerId")?.Value;

            if (!int.TryParse(customerId, out var id) || id <= 0)
            {
                Context.Abort();
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"customer-{id}");
            await base.OnConnectedAsync();
        }
    }
}
