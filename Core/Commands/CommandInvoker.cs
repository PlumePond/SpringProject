using System.Collections.Generic;
using SpringProject.Core.Debugging;

namespace SpringProject.Core.Commands;

public static class CommandInvoker
{
    static readonly Stack<ICommand> _undoStack = new Stack<ICommand>();
    static readonly Stack<ICommand> _redoStack = new Stack<ICommand>();

    public static void Execute(ICommand command)
    {
        command.Execute();
        _undoStack.Push(command);
    }

    public static void Undo()
    {
        if (_undoStack.Count > 0)
        {
            var activeCommand = _undoStack.Pop();
            activeCommand.Undo();
            _redoStack.Push(activeCommand);
        }
    }

    public static void Redo()
    {
        if (_redoStack.Count > 0)
        {
            var activeCommand = _redoStack.Pop();
            activeCommand.Redo();
            _undoStack.Push(activeCommand);
        }
    }
}