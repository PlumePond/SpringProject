using System;
using Microsoft.Xna.Framework;
using SpringProject.Core.Commands;
using SpringProject.Core.Debugging;
using SpringProject.Core.Editor;
using SpringProject.Core.UI;

namespace SpringProject.Core.Commands;

public class PlaceObjectCommand : Command
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

    public override void Execute()
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

    public override void Undo()
    {
        if (_gridPlacement.SelectedObjects.Contains(_levelObject))
        {
            _gridPlacement.Deselect();
        }

        _grid.layers[_levelObject.layer].LevelObjects.Remove(_levelObject);

        NotificationManager.Notify($"Undo: Place Object '{_levelObject.data.name}'");
    }

    public override void Redo()
    {
        _grid.layers[_levelObject.layer].LevelObjects.Add(_levelObject);

        NotificationManager.Notify($"Redo: Place Object '{_levelObject.data.name}'");
    }
}