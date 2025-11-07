namespace ChatClient
{
    public static class SocketManager
    {
        private static readonly string _url = "wss://api.leetcode.se";
        private static readonly string _path = "/sys25d";
        private static readonly string _eventBase = "ludvigmoller";
        private static readonly SocketIOClient.SocketIO _client = new(_url, new() 
        { 
            Path = _path 
        });

        private static string? _current_room;

        public static async Task Connect()
        {
            _client.OnConnected += (sender, args) =>
            {
                Console.WriteLine("Connected");
            };

            _client.OnDisconnected += (sender, args) =>
            {
                Console.WriteLine("Disconnected");
            };

            await _client.ConnectAsync();
        }

        public static async Task SendMessage(Message message, string? username = null)
        {
            var target = username ?? _current_room;
            if (target == null)
                return;

            await _client.EmitAsync($"{_eventBase}_{target}_message", message);
        }

        public static async Task JoinRoom(string room, string username)
        {
            if (_current_room == room) 
                return;
            _current_room = room;

            await _client.EmitAsync($"{_eventBase}_{_current_room}_join", username);
        }

        public static async Task LeaveRoom(string username)
        {
            if (_current_room == null) 
                return;

            await _client.EmitAsync($"{_eventBase}_{_current_room}_leave", username);
            _current_room = null;
        }
    }
}
