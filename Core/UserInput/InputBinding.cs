using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpringProject.Core.UserInput;

public class InputBinding
{
    public bool Pressed { get; protected set; } = false;
    public bool Holding { get; protected set; } = false;
    public bool Released { get; protected set; } = false;

    public Point Point { get; protected set; } = Point.Zero;
    public float Float { get; protected set; } = 0.0f;
    public int Int { get; protected set; } = 0;
    public Vector2 Vector { get; protected set; } = Vector2.Zero;

    public virtual void Update()
    {
        
    }
}