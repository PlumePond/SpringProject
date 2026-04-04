namespace SpringProject.Core.Commands;

public interface ICommand
{
    public void Execute();
    public void Undo();
    public void Redo();
}