using System;
using Microsoft.Xna.Framework;
using SpringProject.Core.Commands;
using SpringProject.Core.Debugging;
using SpringProject.Core.Editor;
using SpringProject.Core.UI;

namespace SpringProject.Core.Commands;

public class RemoveObjectCommand : Command
{
    GridPlacement _gridPlacement;
    LevelObject _levelObject;
    Grid _grid;

    public RemoveObjectCommand(GridPlacement gridPlacement, Grid grid, LevelObject levelObject)
    {
        _gridPlacement = gridPlacement;
        _grid = grid;
        _levelObject = levelObject;
    }

    public override void Execute()
    {
        if (_gridPlacement.SelectedObjects.Contains(_levelObject))
        {
            _gridPlacement.Deselect();
        }
            
        _grid.layers[_levelObject.layer].LevelObjects.Remove(_levelObject);

        _gridPlacement.Dehover();

        // trigger on removed
        _levelObject.OnRemoved();
        _gridPlacement.CanPlaceObject = true;
    }

    public override void Undo()
    {
        _grid.layers[_levelObject.layer].LevelObjects.Add(_levelObject);

        NotificationManager.Notify($"Undo: Remove Object '{_levelObject.data.name}'");
    }

    public override void Redo()
    {
        if (_gridPlacement.SelectedObjects.Contains(_levelObject))
        {
            _gridPlacement.Deselect();
        }
            
        _grid.layers[_levelObject.layer].LevelObjects.Remove(_levelObject);

        _gridPlacement.Dehover();

        NotificationManager.Notify($"Redo: Remove Object '{_levelObject.data.name}'");
    }
}