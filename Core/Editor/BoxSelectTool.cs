using Microsoft.Xna.Framework;
using SpringProject.Core.UI;

namespace SpringProject.Core.Editor;

public class BoxSelectTool(GridPlacement placement) : Tool(placement)
{
    public override CursorType CursorType => CursorType.BoxSelect;

    public override void PressPrimary(LevelObject hovered, Point mousePos, bool swipe)
    {
        
    }

    public override void Release()
    {
        
    }
}