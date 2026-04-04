using Microsoft.Xna.Framework;

namespace SpringProject.Core;

public class Transform
{
    public Point position { get; set; }
    public int rotation { get; set; } = 0;
    public bool flipX { get; set; } = false;
    public bool flipY { get; set; } = false;

    public Transform()
    {
        
    }
}