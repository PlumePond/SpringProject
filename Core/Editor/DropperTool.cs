using Microsoft.Xna.Framework;
using SpringProject.Core.UI;

namespace SpringProject.Core.Editor;

public class DropperTool(GridPlacement placement) : Tool(placement)
{
    public override CursorType CursorType => CursorType.Dropper;

    public override void PressPrimary(LevelObject hovered, Point worldMousePos, Point uiMousePos, bool swipe)
    {
        if (_placement.SelectedObjectData != hovered.data)
        {
            _placement.SetSelectedObjectData(hovered.data);
            NotificationManager.Notify($"Picked level object '{hovered.data.name}'.");
        }
    }

    public override void PressSecondary(LevelObject hovered, Point worldMousePos, Point uiMousePos, bool swipe)
    {
        if (ColorManager.SelectedColorIndex != hovered.colorIndex)
        {
            ColorManager.SetColorIndex(hovered.colorIndex);
            NotificationManager.Notify($"Picked color index {hovered.colorIndex}.");
        }
    }
}