using System;
using Microsoft.Xna.Framework;
using SpringProject.Core.Commands;
using SpringProject.Core.Debugging;
using SpringProject.Core.Editor;

namespace SpringProject.Core.Commands;

public class PlaceObjectCommand : ICommand
{
    GridPlacement _gridPlacement;
    LevelObjectData _levelObjectData;
    Grid _grid;
    bool _flipX;
    bool _flipY;
    Point _point;
    int _rotation;
    int _layer;
    int _colorIndex;

    LevelObject _levelObject;

    public PlaceObjectCommand(GridPlacement gridPlacement, LevelObjectData levelObjectData, Point point, Grid grid, bool flipX, bool flipY, int rotation, int layer, int colorIndex)
    {
        _gridPlacement = gridPlacement;
        _levelObjectData = levelObjectData;
        _point = point;
        _grid = grid;
        _flipX = flipX;
        _flipY = flipY;
        _rotation = rotation;
        _layer = layer;
        _colorIndex = colorIndex;
    }

    public void Execute()
    {
        _levelObject = (LevelObject)Activator.CreateInstance(_levelObjectData.type);
        _levelObject.Initialize(_levelObjectData, _grid, _point);
        _levelObject.SetRotation(!_levelObjectData.scalable ? _rotation : 0);
        _levelObject.SetFlipX(_flipX);
        _levelObject.SetFlipY(_flipY);
        _levelObject.SetLayer(_layer);
        _levelObject.SetColorIndex(_colorIndex);
                    
        _grid.layers[_layer].LevelObjects.Add(_levelObject);

        // trigger on placed
        _levelObject.OnPlaced();
    }

    public void Undo()
    {
        if (_levelObject == _gridPlacement.selectedObject)
        {
            _gridPlacement.Deselect();
        }

        _grid.layers[_levelObject.layer].LevelObjects.Remove(_levelObject);

        Debug.Log($"Undo: Place Object '{_levelObjectData.name}'");
    }

    public void Redo()
    {
        _grid.layers[_levelObject.layer].LevelObjects.Add(_levelObject);

        Debug.Log($"Redo: Place Object '{_levelObjectData.name}'");
    }
}