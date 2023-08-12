namespace ApplicationTypes.Console.Commands
{
    public interface ICommand
    {
        string Name { get; }
        int Execute(IShellService shellService, params string[] args);
    }
}
