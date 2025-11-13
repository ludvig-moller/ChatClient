namespace ChatClient.Commands
{
    public abstract class Command
    {
        public abstract string? ShortTrigger { get; }
        public abstract string[] Arguments { get; }
        public abstract string Description { get; }

        public string[] GetTriggers()
        {
            var fullTrigger = "/" + GetType().Name
                .ToLower()
                .Replace("command", "");

            return ShortTrigger != null ? [fullTrigger, ShortTrigger] : [fullTrigger];
        }

        public abstract Task Execute(string[] args);
    }

    public class HelpCommand : Command
    {
        public override string? ShortTrigger { get; } = null;
        public override string[] Arguments { get; } = [];
        public override string Description { get; } = "Shows a list of all commands and how to use them.";

        public override Task Execute(string[] args)
        {
            string helpMessage = "\nHelp Menu\n";

            for (var i = 0; i < CommandManager.Commands.Count; i++)
            {
                foreach (var trigger in CommandManager.Commands[i].GetTriggers())
                {
                    helpMessage += $"{trigger} ";
                }
                foreach (var argument in CommandManager.Commands[i].Arguments)
                {
                    helpMessage += $"<{argument}> ";
                }
                helpMessage += "- ";

                helpMessage += CommandManager.Commands[i].Description;

                if (i < CommandManager.Commands.Count - 1)
                    helpMessage += "\n";
            }

            Chat.AddMessage(new SystemMessage(helpMessage));

            return Task.CompletedTask;
        }
    }

    public class QuitCommand : Command
    {
        public override string? ShortTrigger { get; } = "/q";
        public override string[] Arguments { get; } = [];
        public override string Description { get; } = "Quits the application.";

        public override async Task Execute(string[] args)
            => await Chat.Quit();
    }

    public class JoinCommand : Command
    {
        public override string? ShortTrigger { get; } = "/j";
        public override string[] Arguments { get; } = ["room"];
        public override string Description { get; } = "Joins the room specified in the room argument.";

        public override async Task Execute(string[] args)
        {
            if (args.Length < 1 || args[0] == string.Empty)
            {
                Chat.AddMessage(new ErrorMessage("Command \"/join\" requires argument room."));
                return;
            }

            await Chat.JoinRoom(args[0]);
        }
    }

    public class LeaveCommand : Command
    {
        public override string? ShortTrigger { get; } = "/l";
        public override string[] Arguments { get; } = [];
        public override string Description { get; } = "Leaves the current room.";

        public override async Task Execute(string[] args)
            => await Chat.LeaveRoom();
    }

    public class RoomsCommand : Command
    {
        public override string? ShortTrigger { get; } = null;
        public override string[] Arguments { get; } = [];
        public override string Description { get; } = "Shows a list of all rooms.";

        public override Task Execute(string[] args)
        {
            string roomsMessage = "Rooms: ";
            for (var i = 0; i < Chat.Rooms.Length; i++)
            {
                roomsMessage += Chat.Rooms[i];

                if (i < Chat.Rooms.Length - 1)
                    roomsMessage += " - ";
            }

            Chat.AddMessage(new SystemMessage(roomsMessage));
            return Task.CompletedTask;
        }
    }

    public class DirectMessageCommand : Command
    {
        public override string? ShortTrigger { get; } = "/dm";
        public override string[] Arguments { get; } = ["receiver", "message"];
        public override string Description { get; } = "Sends a private message to a user specified by username in the receiver argument.";

        public override async Task Execute(string[] args)
        {
            if (args.Length < 2 || args[0] == string.Empty)
            {
                Chat.AddMessage(new ErrorMessage("Command \"/dm\" requires argument recevier and message."));
                return;
            }

            var receiver = args[0];

            string[] messageArray = new string[args.Length - 1];
            Array.Copy(args, 1, messageArray, 0, args.Length - 1);
            string messageText = string.Join(" ", messageArray);

            if (messageText == string.Empty)
            {
                Chat.AddMessage(new ErrorMessage("Can't send an empty message."));
                return;
            }

            var message = new DirectMessage(args[0], Chat.GetUsername(), messageText);

            await SocketManager.SendMessage(message, receiver);
            Chat.AddMessage(message);
        }
    }

    public class ClearCommand : Command
    {
        public override string? ShortTrigger { get; } = "/c";
        public override string[] Arguments { get; } = [];
        public override string Description { get; } = "Clear the terminal of all messages.";

        public override Task Execute(string[] args)
        {
            Chat.ClearMessages();
            return Task.CompletedTask;
        }
    }

    public class IgnoreCommand : Command
    {
        public override string? ShortTrigger { get; } = "/i";
        public override string[] Arguments { get; } = ["username"];
        public override string Description { get; } = "Ignore a user to stop seeing thier messages.";

        public override Task Execute(string[] args)
        {
            if (args.Length < 1 || args[0] == string.Empty)
            {
                Chat.AddMessage(new ErrorMessage("Command \"/ignore\" requires argument username."));
                return Task.CompletedTask;
            }

            Chat.IgnoreUser(args[0]);
            return Task.CompletedTask;
        }
    }

    public class UnignoreCommand : Command
    {
        public override string? ShortTrigger { get; } = null;
        public override string[] Arguments { get; } = ["username"];
        public override string Description { get; } = "Unignores a previously ignored user.";

        public override Task Execute(string[] args)
        {
            if (args.Length < 1 || args[0] == string.Empty)
            {
                Chat.AddMessage(new ErrorMessage("Command \"/unignore\" requires argument username."));
                return Task.CompletedTask;
            }

            Chat.UnignoreUser(args[0]);
            return Task.CompletedTask;
        }
    }

    public class IgnoredCommand : Command
    {
        public override string? ShortTrigger { get; } = null;
        public override string[] Arguments { get; } = [];
        public override string Description { get; } = "Show a list of all your ignored users.";

        public override Task Execute(string[] args)
        {
            string ignoredMessage = "Ignored: ";
            for (var i = 0; i < Chat.IgnoredUsers.Count; i++)
            {
                ignoredMessage += Chat.IgnoredUsers[i];

                if (i < Chat.IgnoredUsers.Count - 1)
                    ignoredMessage += " - ";
            }

            Chat.AddMessage(new SystemMessage(ignoredMessage));
            return Task.CompletedTask;
        }
    }
}
