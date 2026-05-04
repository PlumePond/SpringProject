using Microsoft.Xna.Framework;
using SpringProject.Core.Audio;
using SpringProject.Core.Debugging;
using SpringProject.Core.UI;
using SpringProject.Core.UserInput;

namespace SpringProject.Core.Editor;

public class PointerTool(GridPlacement placement) : Tool(placement)
{
    public override CursorType CursorType => CursorType.Pointer;

    AudioComposite _invalidSound => AudioManager.Get("invalid");

    public override void Update(GameTime gameTime)
    {
        
    }

    public override void PressPrimary(LevelObject hovered, Point mousePos, bool swipe)
    {
        if (swipe) return;

        if (_placement.selectedObject != hovered)
        {
            _placement.Select(hovered);
        }
        else
        {
            _placement.Deselect();
        }
    }

    public override void PressEmpty(Point mousePos, bool swipe)
    {
        if (_placement.selectedObject != null)
        {
            _placement.Deselect();
            return;
        }

        if (_placement.SelectedObjectData == null) return;
        if (!_placement.CanPlaceObject) return;

        var objectPos = _placement.CalculateSmartPlacement(_placement.SelectedObjectData, mousePos, _placement.Grid.GridSize, _placement.Rotation, out bool invalidPlacement, null);
        if (!invalidPlacement)
        {
            _placement.PlaceObject(_placement.SelectedObjectData, objectPos, _placement.Grid.activeLayer);
        }
    }

    public override void PressSecondary(LevelObject hovered, Point mousePos, bool swipe)
    {
        Debug.Log($"deleted object: '{hovered.data.name}'");
        _placement.RemoveObject(hovered);
    }

    public override void Hold(GameTime gameTime, LevelObject hovered)
    {
        
    }

    public override void Release()
    {
        
    }
}