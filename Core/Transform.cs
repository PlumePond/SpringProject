using Microsoft.Xna.Framework;
using SpringProject.Core.Components;

namespace SpringProject.Core;

public class Transform : Component
{
    public Point position { get; set; }
    public int rotation { get; set; } = 0;
    public bool flipX { get; set; } = false;
    public bool flipY { get; set; } = false;
}