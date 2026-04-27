using Microsoft.Xna.Framework;
using SpringProject.Core.UI;
using SpringProject.Core.Commands;

namespace SpringProject.Core.Editor;

public class PaintTool(GridPlacement placement) : Tool(placement)
{
    public override CursorType CursorType => CursorType.Paint;

    public override void Hold(GameTime gameTime, LevelObject hovered)
    {
        if (hovered == null) return;
        if (hovered.colorIndex == ColorManager.SelectedColorIndex) return;

        CommandInvoker.Execute(new ColorObjectCommand(hovered, ColorManager.SelectedColorIndex));
    }
}