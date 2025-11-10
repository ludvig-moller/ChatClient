namespace ChatClient
{
    public static class Chat
    {
        private static bool _isRuinning = true;
        private static string _username = string.Empty;
        private static string _currentRoom = "general";
        private static readonly List<Message> _messages = new();

        private static string _currentPrompt = string.Empty;
        private static string _currentInput = string.Empty;

        public static async Task Start()
        {
            _username = GetUsername();

            await SocketManager.Connect(_username);
            await SocketManager.JoinRoom(_currentRoom, _username);
            await Run();
        }

        private static async Task Run()
        {
            while (_isRuinning)
            {
                var input = GetInput($"Message in {_currentRoom}: ");

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
            OutputMessages();
        }

        private static void OutputMessages()
        {
            Console.Clear();

            foreach (var message in _messages)
                Console.WriteLine($"{message.Date.ToLocalTime().ToShortTimeString()} {message.Username}: {message.Text}");

            Console.Write(_currentPrompt);
            Console.Write(_currentInput);
        }

        private static string GetInput(string prompt)
        {
            Console.Write(prompt);
            _currentPrompt = prompt;

            ConsoleKeyInfo inputKey = new();
            while (inputKey.Key != ConsoleKey.Enter || _currentInput == string.Empty)
            {
                OutputMessages();

                inputKey = Console.ReadKey();

                if (inputKey.Key == ConsoleKey.Enter)
                    continue;

                if (inputKey.Key == ConsoleKey.Backspace)
                    _currentInput = _currentInput.Length > 0 
                        ? _currentInput.Remove(_currentInput.Length - 1) 
                        : _currentInput;
                else
                    _currentInput += inputKey.KeyChar;
            }

            var newInput = _currentInput;
            _currentInput = string.Empty;
            _currentPrompt = string.Empty;

            return newInput;
        }

        private static string GetUsername()
        {
            string username = string.Empty;

            while (username == string.Empty)
            {
                var input = GetInput("Enter your username: ");

                if (input.Any(char.IsWhiteSpace))
                {
                    AddMessage(
                        new Message 
                        { 
                            Text = "Username can't have any whitespace. Try again.", 
                            Username = "System" 
                        });
                    continue;
                }

                username = input;
            }
            return username;
        }
    }
}
