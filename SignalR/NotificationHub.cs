using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
namespace PeShop.SignalR
{
    public class NotificationHub : Hub
    {
        // Static dictionary để track connections: key = "user:123" hoặc "shop:456", value = List<ConnectionId>
        private static readonly Dictionary<string, HashSet<string>> _activeConnections = new();
        private static readonly object _lock = new();

        public override async Task OnConnectedAsync()
        {
            var type = Context.GetHttpContext()?.Request.Query["type"];
            
            if (type?.ToString() == "user")
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
                    
                    // Track connection
                    lock (_lock)
                    {
                        var key = $"user:{userId}";
                        if (!_activeConnections.ContainsKey(key))
                            _activeConnections[key] = new HashSet<string>();
                        _activeConnections[key].Add(Context.ConnectionId);
                    }
                    
                    Console.WriteLine($"User {userId} connected: {Context.ConnectionId}");
                }
                else
                {
                    throw new Exception("yêu cầu không hợp lệ");
                }
            }
            else if (type?.ToString() == "shop")
            {
                var shopId = Context.User?.FindFirst("shop_id")?.Value;
                if (!string.IsNullOrEmpty(shopId))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"shop:{shopId}");
                    
                    // Track connection
                    lock (_lock)
                    {
                        var key = $"shop:{shopId}";
                        if (!_activeConnections.ContainsKey(key))
                            _activeConnections[key] = new HashSet<string>();
                        _activeConnections[key].Add(Context.ConnectionId);
                    }
                    
                    Console.WriteLine($"Shop {shopId} connected: {Context.ConnectionId}");
                }
                else
                {
                    throw new Exception("yêu cầu không hợp lệ");
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var shopId = Context.User?.FindFirst("shop_id")?.Value;

            // Remove connection from tracking
            lock (_lock)
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    var key = $"user:{userId}";
                    if (_activeConnections.ContainsKey(key))
                    {
                        _activeConnections[key].Remove(Context.ConnectionId);
                        if (_activeConnections[key].Count == 0)
                            _activeConnections.Remove(key);
                    }
                }
                
                if (!string.IsNullOrEmpty(shopId))
                {
                    var key = $"shop:{shopId}";
                    if (_activeConnections.ContainsKey(key))
                    {
                        _activeConnections[key].Remove(Context.ConnectionId);
                        if (_activeConnections[key].Count == 0)
                            _activeConnections.Remove(key);
                    }
                }
            }

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user:{userId}");
                Console.WriteLine($"User {userId} disconnected: {Context.ConnectionId}");
            }
            
            if (!string.IsNullOrEmpty(shopId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"shop:{shopId}");
                Console.WriteLine($"Shop {shopId} disconnected: {Context.ConnectionId}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Static method để check connection (có thể gọi từ bất kỳ đâu)
        public static bool IsConnected(string id, string type)
        {
            lock (_lock)
            {
                var key = $"{type}:{id}";
                return _activeConnections.ContainsKey(key) && _activeConnections[key].Count > 0;
            }
        }

        // Static method để lấy số lượng user và shop online
        public static (int OnlineUsers, int OnlineShops) GetOnlineCount()
        {
            lock (_lock)
            {
                var onlineUsers = _activeConnections.Keys.Count(k => k.StartsWith("user:"));
                var onlineShops = _activeConnections.Keys.Count(k => k.StartsWith("shop:"));
                return (onlineUsers, onlineShops);
            }
        }
    }
}
