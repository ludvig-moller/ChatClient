using SocketIOClient;
using System.Text.Json;

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
                => Chat.AddMessage(new SystemMessage("Connected to the server."));

            _client.OnDisconnected += (sender, args) 
                => Chat.AddMessage(new SystemMessage("Disconnected from the server."));

            _client.On($"{_eventBase}_{username.ToLower()}_message", response 
                => OnDirectMessage(username, response));

            await _client.ConnectAsync();

            await Task.Delay(1000);
        }

        public static async Task Disconnect()
            => await _client.DisconnectAsync();

        public static async Task SendMessage(Message message, string target)
            => await _client.EmitAsync($"{_eventBase}_{target}_message", message);

        public static async Task JoinRoom(string room, string username)
        {
            _client.On($"{_eventBase}_{room}_join", response 
                => OnJoin(room, response));
            _client.On($"{_eventBase}_{room}_leave", response 
                => OnLeave(room, response));
            _client.On($"{_eventBase}_{room}_message", response 
                => OnRoomMessage(room, response));

            await _client.EmitAsync($"{_eventBase}_{room}_join", username);
        }

        public static async Task LeaveRoom(string room, string username)
        {
            _client.Off($"{_eventBase}_{room}_join");
            _client.Off($"{_eventBase}_{room}_leave");
            _client.Off($"{_eventBase}_{room}_message");

            await _client.EmitAsync($"{_eventBase}_{room}_leave", username); 
        }

        private static void OnRoomMessage(string room, SocketIOResponse response)
        {
            try
            {
                var json = response.GetValue<JsonElement>();
                string? sender = json.TryGetProperty("Sender", out JsonElement jsonSender) 
                    ? jsonSender.ToString() 
                    : null;
                string? text = json.TryGetProperty("Text", out JsonElement jsonText)
                    ? jsonText.ToString()
                    : null;
                DateTime? date = json.TryGetProperty("Date", out JsonElement jsonDate)
                    ? date = jsonDate.GetDateTime()
                    : null;

                if (sender == null || text == null)
                    throw new Exception();
                
                Chat.AddMessage(new RoomMessage(room, sender, text, date));
            }
            catch
            {
                Chat.AddMessage(new ErrorMessage("Recived invaild message."));
            }
        }

        private static void OnDirectMessage(string receiver, SocketIOResponse response)
        {
            try
            {
                var json = response.GetValue<JsonElement>();
                string? sender = json.TryGetProperty("Sender", out JsonElement jsonSender)
                    ? jsonSender.ToString()
                    : null;
                string? text = json.TryGetProperty("Text", out JsonElement jsonText)
                    ? jsonText.ToString()
                    : null;
                DateTime? date = json.TryGetProperty("Date", out JsonElement jsonDate)
                    ? date = jsonDate.GetDateTime()
                    : null;

                if (sender == null || text == null)
                    throw new Exception();

                Chat.AddMessage(new DirectMessage(receiver, sender, text, date));
            }
            catch
            {
                Chat.AddMessage(new ErrorMessage("Recived invaild message."));
            }
        }

        private static void OnJoin(string room, SocketIOResponse response)
        {
            var username = response.GetValue<string>();
            Chat.AddMessage(new JoinRoomMessage(room, username));
        }

        private static void OnLeave(string room, SocketIOResponse response)
        {
            var username = response.GetValue<string>();
            Chat.AddMessage(new LeaveRoomMessage(room, username));
        }
    }
}
