using SocketIOClient;

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

        public static async Task Connect(string username)
        {
            _client.OnConnected += (sender, args) 
                => Console.WriteLine("Connected to the server!");

            _client.OnDisconnected += (sender, args) 
                => Console.WriteLine("Disconnected from the server!");

            _client.On($"{_eventBase}_{username.ToLower()}_message", OnMessage);

            await _client.ConnectAsync();

            await Task.Delay(1000);
        }

        public static async Task Disconnect()
            => await _client.DisconnectAsync();

        public static async Task SendMessage(Message message, string target)
            => await _client.EmitAsync($"{_eventBase}_{target}_message", message);

        public static async Task JoinRoom(string room, string username)
        {
            _client.On($"{_eventBase}_{room}_join", OnJoin);
            _client.On($"{_eventBase}_{room}_leave", OnLeave);
            _client.On($"{_eventBase}_{room}_message", OnMessage);

            await _client.EmitAsync($"{_eventBase}_{room}_join", username);
        }

        public static async Task LeaveRoom(string room, string username)
        {
            _client.Off($"{_eventBase}_{room}_join");
            _client.Off($"{_eventBase}_{room}_leave");
            _client.Off($"{_eventBase}_{room}_message");

            await _client.EmitAsync($"{_eventBase}_{room}_leave", username); 
        }

        private static void OnMessage(SocketIOResponse response)
        {
            try
            {
                var message = response.GetValue<Message>();
                Chat.AddMessage(message);
            }
            catch 
            {
                var message = new Message { Text = "Recived invaild message.", Username = "System" };
                Chat.AddMessage(message);
            }
        }

        private static void OnJoin(SocketIOResponse response)
        {
            var username = response.GetValue<string>();
            var message = new Message { Text = $"{username} joined the room.", Username = "System" };
            Chat.AddMessage(message);
        }

        private static void OnLeave(SocketIOResponse response)
        {
            var username = response.GetValue<string>();
            var message = new Message { Text = $"{username} left the room.", Username = "System" };
            Chat.AddMessage(message);
        }
    }
}
