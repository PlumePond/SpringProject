using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpringProject.Core.Commands;
using SpringProject.Core.Debugging;
using SpringProject.Core.Editor;
using SpringProject.Core.UI;

namespace SpringProject.Core.Commands;

public class BatchRemoveObjectsCommand : Command
{
    GridPlacement _gridPlacement;
    LevelObject[] _levelObjects;
    Grid _grid;

    public BatchRemoveObjectsCommand(GridPlacement gridPlacement, Grid grid, LevelObject[] levelObjects)
    {
        _gridPlacement = gridPlacement;
        _grid = grid;
        _levelObjects = levelObjects;
    }

    public override void Execute()
    {
        foreach (var levelObject in _levelObjects)
        {
            _grid.layers[levelObject.layer].LevelObjects.Remove(levelObject);
            levelObject.OnRemoved();
        }
        
        _gridPlacement.Deselect();
        _gridPlacement.Dehover();
        _gridPlacement.CanPlaceObject = true;
    }

    public override void Undo()
    {
        foreach (var levelObject in _levelObjects)
        {
            _grid.layers[levelObject.layer].LevelObjects.Add(levelObject);
            levelObject.OnPlaced();
        }

        NotificationManager.Notify($"Undo: Batch Remove Objects. Count: {_levelObjects.Length}.");
    }

    public override void Redo()
    {
        Execute();
        NotificationManager.Notify($"Redo: Batch Remove Objects. Count: {_levelObjects.Length}.");
    }
}