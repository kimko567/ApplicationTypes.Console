using ApplicationTypes.Console.Commands;
using System.Reflection;

namespace ApplicationTypes.Console
{
    public interface IShellService
    {
        /// <summary>
        /// Write a message.
        /// </summary>
        /// <param name="message">Message to write.</param>
        void Write(string message);
        /// <summary>
        /// Write a temporary message.
        /// The next write operation will clear <paramref name="message"/>.
        /// </summary>
        /// <param name="message">Message to write.</param>
        void WriteTemp(string message);
    }

    public sealed class Shell : IShellService
    {
        private const string c_clearChar = "\b \b";
        private const string c_suffix = "> ";
        private string _context = null;
        private string _currentMessage = null;
        private bool _exit = false;
        private Stack<string> _history = new Stack<string>();

        /// <summary>
        /// Start and run shell.
        /// </summary>
        public void Run()
        {
            while (!_exit)
            {
                System.Console.Write($"{_context}{c_suffix}");

                var input = System.Console.ReadLine();

                if (!string.IsNullOrEmpty(input))
                    HandleInput(input);
            }
        }

        /// <inheritdoc/>
        public void Write(string message)
        {
            if (_currentMessage != null)
                message = $"{GetClearString()}{message}";

            System.Console.WriteLine(message);
        }

        /// <inheritdoc/>
        public void WriteTemp(string message)
        {
            if (_currentMessage != null)
                ClearCurrentMessage();

            _currentMessage = message;

            System.Console.Write(message);
        }

        private void ClearCurrentMessage()
        {
            System.Console.Write(GetClearString());

            _currentMessage = null;
        }

        private string GetClearString() => string.Join(string.Empty, Enumerable.Range(0, _currentMessage.Length).Select(t => c_clearChar));

        private void HandleInput(string input)
        {
            _history.Push(input);

            var args = input.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .ToArray();

            var command = args.First();
            var commandArgs = args.Skip(1).ToArray();

            if (!IsShellCommand(command))
            {
                var commandHandler = Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where(type => typeof(ICommand).IsAssignableFrom(type) && !type.IsInterface)
                    .Select(t => (ICommand)Activator.CreateInstance(t))
                    .FirstOrDefault(t => t.Name == command);

                if (commandHandler != null)
                    try
                    {
                        commandHandler.Execute(this, commandArgs);
                    }
                    catch (Exception ex)
                    {
                        HandleException(ex);
                    }

                return;
            }

            switch (command)
            {
                case Command.Clear:
                    System.Console.Clear();
                    break;
                case Command.Exit:
                    _exit = true;
                    break;
                case Command.History:
                    System.Console.WriteLine(_history.ToString());
                    break;
                default:
                    System.Console.WriteLine("Unknown command.");
                    break;
            }
        }

        private void HandleException(Exception ex)
        {
            var initialColor = System.Console.ForegroundColor;

            System.Console.ForegroundColor = ConsoleColor.Red;

            System.Console.WriteLine(ex.ToString());

            System.Console.ForegroundColor = initialColor;
        }

        private bool IsShellCommand(string command)
        {
            return Command.All.Contains(command);
        }

        public static class Command
        {
            public const string Clear = "clear";
            public const string Exit = "exit";
            public const string History = "history";

            public static readonly string[] All = new string[]
                {
                    Clear,
                    Exit,
                    History
                };
        }
    }
}
