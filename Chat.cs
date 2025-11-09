namespace ChatClient
{
    public static class Chat
    {
        private static bool _isRuinning = true;
        private static string _username = string.Empty;
        private static string _currentRoom = "general";
        private static readonly List<Message> _messages = new();

        public static async Task Start()
        {
            _username = GetUsername();

            await SocketManager.Connect();
            await SocketManager.JoinRoom(_currentRoom, _username);
            await Run();
        }

        private static async Task Run()
        {
            while (_isRuinning)
            {
                var input = Console.ReadLine();

                if (input == null)
                    continue;

                if (input == "/quit")
                {
                    _isRuinning = false;
                    await SocketManager.LeaveRoom(_currentRoom, _username);
                    await SocketManager.Disconnect();
                    break;
                }

                if (_currentRoom == null)
                {
                    Console.WriteLine("Can't send message without joining a room.");
                    continue;
                }

                var message = new Message { Text = input, Username = _username };

                AddMessage(message);
                await SocketManager.SendMessage(message, _currentRoom);
            }
        }

        public static void AddMessage(Message message)
        {
            _messages.Add(message);
            Console.WriteLine(message);
        }

        private static string GetUsername()
        {
            string username = string.Empty;

            while (username == string.Empty)
            {
                var input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("Username can't be empty.");
                    continue;
                }

                if (input.Any(char.IsWhiteSpace))
                {
                    Console.WriteLine("Username can't have any whitespace.");
                    continue;
                }

                username = input;
            }
            return username;
        }
    }
}
