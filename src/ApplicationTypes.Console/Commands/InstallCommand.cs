using System.Diagnostics;
using Timer = System.Timers.Timer;

namespace ApplicationTypes.Console.Commands
{
    public class InstallCommand : ICommand
    {
        public string Name => "install";

        private readonly string[] _steps = new string[]
        {
            "Prepare work station",
            "Install drivers",
            "Update configurations",
            "Dispose of resources"
        };

        public int Execute(IShellService shellService, params string[] args)
        {
            string output = null;

            int elapsedCount = 0;

            using var timer = new Timer(50);

            timer.Elapsed += (sender, e) =>
            {
                elapsedCount++;

                if (string.IsNullOrEmpty(output))
                    return;

                char loader = '-';

                switch (elapsedCount % 3)
                {
                    case 0:
                        loader = '\\';
                        break;
                    case 1:
                        loader = '/';
                        break;
                    default:
                        loader = '-';
                        break;
                }

                shellService.WriteTemp($"{loader} {output}");
            };

            timer.Start();

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            foreach (var step in _steps)
            {
                var value = Random.Shared.Next(0, 10);

                if (value == 0)
                    throw new Exception("Something went wrong.");

                output = step;

                Thread.Sleep(1000 * value);

                shellService.Write($"DONE - {step}.");
            }

            timer.Stop();

            stopwatch.Stop();

            shellService.Write($"Completed in {stopwatch.Elapsed.TotalMilliseconds}ms");

            return 0;
        }
    }
}
