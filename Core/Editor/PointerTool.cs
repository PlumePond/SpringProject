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

    public override void PressPrimary(LevelObject hovered, Point worldMousePos, Point uiMousePos, bool swipe)
    {
        if (swipe) return;

        if (!_placement.SelectedObjects.Contains(hovered))
        {
            _placement.Select(hovered);
        }
        else
        {
            _placement.Deselect();
        }
    }

    public override void PressEmpty(Point worldMousePos, Point uiMousePos, bool swipe)
    {
        if (_placement.SelectedObjects.Count > 0)
        {
            _placement.Deselect();
            return;
        }

        if (_placement.SelectedObjectData == null) return;
        if (!_placement.CanPlaceObject) return;

        var objectPos = _placement.CalculateSmartPlacement(_placement.SelectedObjectData, worldMousePos, (int)_placement.SnapSize, _placement.Rotation, out bool invalidPlacement, null);
        if (!invalidPlacement)
        {
            _placement.PlaceObject(_placement.SelectedObjectData, objectPos, _placement.Grid.activeLayer);
        }
    }

    public override void PressSecondary(LevelObject hovered, Point worldMousePos, Point uiMousePos, bool swipe)
    {
        _placement.RemoveObject(hovered);
    }

    public override void Hold(GameTime gameTime, LevelObject hovered)
    {
        
    }

    public override void Release()
    {
        
    }
}