using System;
using Microsoft.Xna.Framework;
using SpringProject.Core.Commands;
using SpringProject.Core.Debugging;
using SpringProject.Core.Editor;
using SpringProject.Core.UI;

namespace SpringProject.Core.Commands;

public class ColorObjectCommand : Command
{
    LevelObject _levelObject;
    int _colorIndex;
    int _originalColorIndex;

    public ColorObjectCommand(LevelObject levelObject, int colorIndex)
    {
        _colorIndex = colorIndex;
        _levelObject = levelObject;
    }

    public override void Execute()
    {
        _originalColorIndex = _levelObject.colorIndex;
        _levelObject.SetColorIndex(_colorIndex);       
    }

    public override void Undo()
    {
        _levelObject.SetColorIndex(_originalColorIndex);
        NotificationManager.Notify($"Undo: Color Object '{_levelObject.data.name}'");
    }

    public override void Redo()
    {
        _levelObject.SetColorIndex(_colorIndex);
        NotificationManager.Notify($"Redo: Color Object '{_levelObject.data.name}'");
    }
}