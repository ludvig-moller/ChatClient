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

        public static async Task Connect()
        {
            _client.OnConnected += (sender, args) 
                => Console.WriteLine("Connected to the server!");

            _client.OnDisconnected += (sender, args) 
                => Console.WriteLine("Disconnected from the server!");

            await _client.ConnectAsync();
        }

        public static async Task Disconnect()
            => await _client.DisconnectAsync();

        public static async Task SendMessage(Message message, string target)
            => await _client.EmitAsync($"{_eventBase}_{target}_message", message);

        public static async Task JoinRoom(string room, string username)
            => await _client.EmitAsync($"{_eventBase}_{room}_join", username);

        public static async Task LeaveRoom(string room, string username)
            => await _client.EmitAsync($"{_eventBase}_{room}_leave", username);
    }
}
