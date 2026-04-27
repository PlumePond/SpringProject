using System.Collections.Generic;
using SpringProject.Core.Debugging;
using SpringProject.Core.UI;

namespace SpringProject.Core.Commands;

public static class CommandInvoker
{
    static readonly Stack<Command> _undoStack = new Stack<Command>();
    static readonly Stack<Command> _redoStack = new Stack<Command>();

    public static void Execute(Command command)
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