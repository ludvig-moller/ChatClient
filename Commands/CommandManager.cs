using System.Reflection;

namespace ChatClient.Commands
{
    public static class CommandManager
    {
        public static List<Command> Commands { get; } = Assembly.GetAssembly(typeof(Command)).GetTypes()
            .Where(c => c.IsSubclassOf(typeof(Command)))
            .Select(Activator.CreateInstance)
            .Cast<Command>()
            .ToList();

        public static async Task HandleCommand(string input)
        {
            string[] commandInputParts = input.Split(' ');

            string commandInput = commandInputParts[0].ToLower();

            Command? choosenCommand = null;
            foreach (var command in Commands)
            {
                foreach (var trigger in command.GetTriggers())
                {
                    if (commandInput != trigger)
                        continue;

                    choosenCommand = command;
                    break;
                }
                if (choosenCommand != null)
                    break;
            }

            if (choosenCommand == null)
            {
                Chat.AddMessage(new ErrorMessage($"Command \"{commandInput}\" dose not exist. Type /help to see the list of all commands."));
                return;
            }

            string[] args = new string[commandInputParts.Length - 1];
            Array.Copy(commandInputParts, 1, args, 0, commandInputParts.Length - 1);

            await choosenCommand.Execute(args);
        }
    }
}
