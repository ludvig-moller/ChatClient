using ChatClient.Commands;

namespace ChatClient
{
    public static class Chat
    {
        private static bool _isRuinning = true;
        private static string _username = string.Empty;
        private static string? _currentRoom = null;

        private static readonly List<Message> _messages = new();

        private static string _currentPrompt = string.Empty;
        private static string _currentInput = string.Empty;

        public static string[] Rooms { get; } = ["General", "Study", "Comedy", "Game", "Music", "Movie", "Art"];
        public static List<string> IgnoredUsers { get; } = new();

        public static async Task Start()
        {
            AddMessage(new SystemMessage("Welcome to this chat start by entering your username!"));

            _username = AskForUsername();

            await SocketManager.Connect(_username);
            await Run();
        }

        private static async Task Run()
        {
            AddMessage(new SystemMessage("Start by typing a command. If you need help /help shows all the commands."));

            while (_isRuinning)
            {
                string input = GetInput(_currentRoom != null
                        ? $"Message in {_currentRoom}: "
                        : "Command: "
                    );

                if (input[0] == '/')
                {
                    await CommandManager.HandleCommand(input);
                    continue;
                }

                if (_currentRoom == null)
                {
                    AddMessage(new ErrorMessage("Can't send messages without beeing in a room. Use the command /join <room>."));
                    continue;
                }

                var message = new RoomMessage(_currentRoom, _username, input);
                
                AddMessage(message);
                await SocketManager.SendMessage(message, _currentRoom);
            }
        }

        public static async Task Quit()
        {
            _isRuinning = false;

            if (_currentRoom != null)
                await SocketManager.LeaveRoom(_currentRoom, _username);

            await SocketManager.Disconnect();
        }

        public static async Task JoinRoom(string room)
        {
            var foundRoom = false;
            foreach (var availableRoom in Rooms)
            {
                if (availableRoom.Equals(room, StringComparison.CurrentCultureIgnoreCase))
                {
                    room = availableRoom;
                    foundRoom = true;
                    break;
                }
            }

            if (!foundRoom)
            {
                AddMessage(new ErrorMessage($"Room \"{room}\" dose not exist. Type /rooms to get the list of rooms."));
                return;
            }

            if (_currentRoom != null)
            {
                await SocketManager.LeaveRoom(_currentRoom, _username);
                AddMessage(new LeaveRoomMessage(_currentRoom, _username));
            }

            _currentRoom = room;
            await SocketManager.JoinRoom(room, _username);
            AddMessage(new JoinRoomMessage(_currentRoom, _username));
        }

        public static async Task LeaveRoom()
        {
            if (_currentRoom == null)
            {
                AddMessage(new ErrorMessage("Can't leave without beeing in a room."));
                return;
            }

            await SocketManager.LeaveRoom(_currentRoom, _username);
            AddMessage(new LeaveRoomMessage(_currentRoom, _username));
            _currentRoom = null;
        }

        public static void IgnoreUser(string username)
        {
            username = username.ToLower();

            if (IgnoredUsers.Contains(username))
            {
                AddMessage(new ErrorMessage($"User \"{username}\" is already ignored."));
                return;
            }

            IgnoredUsers.Add(username);
            AddMessage(new SystemMessage($"User \"{username}\" has been ignored."));
        }

        public static void UnignoreUser(string username)
        {
            username = username.ToLower();

            if (IgnoredUsers.Remove(username))
            {
                AddMessage(new SystemMessage($"User \"{username}\" has been unignored."));
            }
            else
            {
                AddMessage(new ErrorMessage($"User \"{username}\" was not found in the ignore list."));
            }
        }

        public static void AddMessage(Message message)
        {
            if (IgnoredUsers.Contains(message.Sender.ToLower()))
                return;

            _messages.Add(message);
            OutputMessages();
        }

        public static void ClearMessages()
        {
            _messages.Clear();
            OutputMessages();
        }

        private static void OutputMessages()
        {
            Console.Clear();

            foreach (var message in _messages)
            {
                var messageParts = message.GetMessageParts();
                var colors = messageParts.Item1;
                var textParts = messageParts.Item2; 

                for (var i = 0; i < colors.Length && i < textParts.Length; i++)
                {
                    Console.ForegroundColor = colors[i];
                    Console.Write(textParts[i]);
                }

                Console.Write('\n');
            }

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(_currentPrompt);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(_currentInput);
        }

        public static string GetUsername()
            => _username;

        private static string AskForUsername()
        {
            string username = string.Empty;

            while (username == string.Empty)
            {
                var input = GetInput("Enter your username: ");

                if (input.Any(char.IsWhiteSpace))
                {
                    AddMessage(new ErrorMessage("Username can't have any whitespace. Try again."));
                    continue;
                }

                username = input;
            }
            return username;
        }

        private static string GetInput(string prompt)
        {
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
    }
}
