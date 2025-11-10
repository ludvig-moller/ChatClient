namespace ChatClient
{
    public abstract class Message(string sender, string text, DateTime? date = null)
    {
        public DateTime Date { get; } = date ?? DateTime.UtcNow;
        public string Sender { get; } = sender;
        public string Text { get; } = text;

        public abstract Tuple<ConsoleColor[], string[]> GetMessageParts();
    }

    public class SystemMessage(string text) : Message("System", text)
    {
        public override Tuple<ConsoleColor[], string[]> GetMessageParts()
        {
            var colors = new ConsoleColor[2];
            var textParts = new string[2];

            colors[0] = ConsoleColor.Gray;
            textParts[0] = Date.ToLocalTime().ToShortTimeString() + " ";

            colors[1] = ConsoleColor.Blue;
            textParts[1] = $"[SYSTEM] {Text}";

            return new Tuple<ConsoleColor[], string[]>(colors, textParts);
        }
    }

    public class ErrorMessage(string text) : Message("Error", text)
    {
        public override Tuple<ConsoleColor[], string[]> GetMessageParts()
        {
            var colors = new ConsoleColor[2];
            var messageParts = new string[2];

            colors[0] = ConsoleColor.Gray;
            messageParts[0] = Date.ToLocalTime().ToShortTimeString() + " ";

            colors[1] = ConsoleColor.Red;
            messageParts[1] = $"[Error] {Text}";

            return new Tuple<ConsoleColor[], string[]>(colors, messageParts);
        }
    }

    public class RoomMessage(string room, string sender, string text, DateTime? date = null) : Message(sender, text, date)
    {
        private readonly string _room = room;

        public override Tuple<ConsoleColor[], string[]> GetMessageParts()
        {
            var colors = new ConsoleColor[3];
            var messageParts = new string[3];

            colors[0] = ConsoleColor.Gray;
            messageParts[0] = $"{Date.ToLocalTime().ToShortTimeString()} [{_room}] ";

            colors[1] = ConsoleColor.Cyan;
            messageParts[1] = Sender + ": ";

            colors[2] = ConsoleColor.White;
            messageParts[2] = Text;

            return new Tuple<ConsoleColor[], string[]>(colors, messageParts);
        }
    }

    public class JoinRoomMessage(string room, string sender, DateTime? date = null) : Message(sender, "joined the room!", date)
    {
        private readonly string _room = room;

        public override Tuple<ConsoleColor[], string[]> GetMessageParts()
        {
            var colors = new ConsoleColor[2];
            var messageParts = new string[2];

            colors[0] = ConsoleColor.Gray;
            messageParts[0] = $"{Date.ToLocalTime().ToShortTimeString()} [{_room}] ";

            colors[1] = ConsoleColor.Green;
            messageParts[1] = $"{Sender} {Text}";

            return new Tuple<ConsoleColor[], string[]>(colors, messageParts);
        }
    }

    public class LeaveRoomMessage(string room, string sender, DateTime? date = null) : Message(sender, "left the room!", date)
    {
        private readonly string _room = room;

        public override Tuple<ConsoleColor[], string[]> GetMessageParts()
        {
            var colors = new ConsoleColor[2];
            var messageParts = new string[2];

            colors[0] = ConsoleColor.Gray;
            messageParts[0] = $"{Date.ToLocalTime().ToShortTimeString()} [{_room}] ";

            colors[1] = ConsoleColor.Red;
            messageParts[1] = $"{Sender} {Text}";

            return new Tuple<ConsoleColor[], string[]>(colors, messageParts);
        }
    }

    public class DirectMessage(string receiver, string sender, string text, DateTime? date = null) : Message(sender, text, date)
    {
        private readonly string _receiver = receiver;

        public override Tuple<ConsoleColor[], string[]> GetMessageParts()
        {
            var colors = new ConsoleColor[3];
            var messageParts = new string[3];

            colors[0] = ConsoleColor.Gray;
            messageParts[0] = $"{Date.ToLocalTime().ToShortTimeString()} [DM] ";

            colors[1] = ConsoleColor.Cyan;
            messageParts[1] = $"{Sender} -> {_receiver}: ";

            colors[2] = ConsoleColor.White;
            messageParts[2] = Text;

            return new Tuple<ConsoleColor[], string[]>(colors, messageParts);
        }
    }
}
