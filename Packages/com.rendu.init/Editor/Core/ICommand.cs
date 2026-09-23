namespace RD.Init.Editor
{
    public interface ICommand
    {
        void Execute();
    }

    public interface ICommand<T> : ICommand
    {
        void Execute(T parameter);
    }
}
