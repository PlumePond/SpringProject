using System;
using Microsoft.Xna.Framework;
using SpringProject.Core.Commands;
using SpringProject.Core.Debugging;
using SpringProject.Core.Editor;

namespace SpringProject.Core.Commands;

public class RemoveObjectCommand : ICommand
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

    public void Execute()
    {
        if (_levelObject == _gridPlacement.selectedObject)
        {
            _gridPlacement.Deselect();
        }
            
        _grid.layers[_levelObject.layer].LevelObjects.Remove(_levelObject);

        _gridPlacement.Dehover();
    }

    public void Undo()
    {
        _grid.layers[_levelObject.layer].LevelObjects.Add(_levelObject);

        Debug.Log($"Undo: Remove Object '{_levelObject.data.name}'");
    }

    public void Redo()
    {
        if (_levelObject == _gridPlacement.selectedObject)
        {
            _gridPlacement.Deselect();
        }
            
        _grid.layers[_levelObject.layer].LevelObjects.Remove(_levelObject);

        _gridPlacement.Dehover();

        Debug.Log($"Redo: Remove Object '{_levelObject.data.name}'");
    }
}