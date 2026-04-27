using Microsoft.Xna.Framework;
using SpringProject.Core.UI;

namespace SpringProject.Core.Editor;

public abstract class Tool(GridPlacement placement)
{
    protected GridPlacement _placement = placement;
    public virtual CursorType CursorType => default; 

    public virtual void Update(GameTime gameTime)
    {
        
    }

    public virtual void PressPrimary(LevelObject hovered, Point mousePos, bool swipe)
    {
        
    }

    public virtual void PressSecondary(LevelObject hovered, Point mousePos, bool swipe)
    {
        
    }

    public virtual void PressEmpty(Point mousePos, bool swipe)
    {
        
    }

    public virtual void Hold(GameTime gameTime, LevelObject hovered)
    {
        
    }

    public virtual void Release()
    {
        
    }
}