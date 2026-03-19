using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpringProject.Core.UserInput;

namespace SpringProject.Core.UI;

public class Element
{
    public Point localPosition { get; protected set; }
    public Point size { get; protected set; }
    public Vector2 localScale { get; protected set; }
    public Origin origin { get; protected set; }
    public Anchor anchor { get; protected set; }

    public Point originOffset { get; protected set; }
    public Point anchorOffset { get; protected set; }

    public Color color { get; protected set; } = Color.White;

    protected bool _prevHovering = false;
    protected bool _prevPressed = false;

    public Point AbsolutePosition
    {
        get
        {
            if (_parent != null)
            {
                return _parent.AbsolutePosition + localPosition + anchorOffset + originOffset;
            }
            else
            {
                return localPosition;
            }
        }
    }

    public Vector2 AbsoluteScale
    {
        get
        {
            if (_parent != null)
            {
                return _parent.AbsoluteScale * localScale;
            }
            else
            {
                return localScale;
            }
        }
    }

    protected List<Element> _children;
    protected Element _parent;

    public Element(Point localPosition, Point size, Vector2 localScale, Origin origin = Origin.MiddleCenter, Anchor anchor = Anchor.MiddleCenter)
    {
        this.localPosition = localPosition;
        this.size = size;
        this.localScale = localScale;
        this.origin = origin;
        this.anchor = anchor;

        _children = new List<Element>();
        originOffset = GetOriginOffset();
    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        
    }

    public bool WithinBounds(Point point)
    {
        return 
        point.X >= AbsolutePosition.X && 
        point.X <= AbsolutePosition.X + size.X * AbsoluteScale.X && 
        point.Y >= AbsolutePosition.Y && 
        point.Y <= AbsolutePosition.Y + size.Y * AbsoluteScale.Y;
    }

    public virtual void Update(GameTime gameTime)
    {
        Point mousePoint = Input.Get("cursor").Point;

        // convert mouse position to a point in the window. it is already in window coordinates, but we need to convert it to a point and account for the pixel scale of the UI
        Point mousePosition = new Point(mousePoint.X / Main.Settings.UISize, mousePoint.Y / Main.Settings.UISize);

        // check if the mouse is hovering over the element
        bool isHovering = WithinBounds(mousePosition);
    
        bool isPressed = Input.Get("ui_click").Holding;

        if (isHovering && !_prevHovering)
        {
            OnMouseEnter();
        }
        else if (isHovering)
        {
            OnMouseHover();
        }
        else if (!isHovering && _prevHovering)
        {
            OnMouseExit();
        }

        if (isHovering && isPressed && !_prevPressed)
        {
            OnPressed();
        }
        else if (isHovering && !isPressed && _prevPressed)
        {
            OnReleased();
        }

        _prevHovering = isHovering;
        _prevPressed = isPressed;
    }

    public void UpdateChildren(GameTime gameTime)
    {
        foreach (var child in _children)
        {
            child.Update(gameTime);
            child.UpdateChildren(gameTime);
        }
    }

    public void DrawChildren(SpriteBatch spriteBatch)
    {
        foreach (var child in _children)
        {
            child.Draw(spriteBatch);
            child.DrawChildren(spriteBatch);
        }
    }

    public virtual void AddChild(Element child)
    {
        _children.Add(child);
        child._parent = this;
        child.anchorOffset = child.GetAnchorOffset();
    }

    public virtual void RemoveChild(Element child)
    {
        _children.Remove(child);
        child._parent = null;
    }

    public virtual void ClearChildren()
    {
        foreach (var child in _children)
        {
            child._parent = null;
        }
        
        _children.Clear();
    }

    public void SetLocalPosition(Point localPosition)
    {
        this.localPosition = localPosition;
        ReCalculateOffsets();
    }

    public void SetLocalScale(Vector2 localScale)
    {
        this.localScale = localScale;
        ReCalculateOffsets();
    }

    public void SetSize(Point size)
    {
        this.size = size;
        ReCalculateOffsets();
    }

    public void SetAnchor(Anchor anchor)
    {
        this.anchor = anchor;
        anchorOffset = GetAnchorOffset();
    }

    public void SetOrigin(Origin origin)
    {
        this.origin = origin;
        originOffset = GetOriginOffset();
    }

    public void SetColor(Color color)
    {
        this.color = color;
    }

    public void ReCalculateOffsets()
    {
        originOffset = GetOriginOffset();
        anchorOffset = GetAnchorOffset();

        foreach (var child in _children)
        {
            child.ReCalculateOffsets();
        }
    }

    public Point GetOriginOffset()
    {
        switch (origin)
        {
            case Origin.TopLeft:
                return Point.Zero;
            case Origin.TopCenter:
                return new Point(-size.X / 2, 0);
            case Origin.TopRight:
                return new Point(-size.X, 0);
            case Origin.MiddleLeft:
                return new Point(0, -size.Y / 2);
            case Origin.MiddleCenter:
                return new Point(-size.X / 2, -size.Y / 2);
            case Origin.MiddleRight:
                return new Point(-size.X, -size.Y / 2);
            case Origin.BottomLeft:
                return new Point(0, -size.Y);
            case Origin.BottomCenter:
                return new Point(-size.X / 2, -size.Y);
            case Origin.BottomRight:
                return new Point(-size.X, -size.Y);
            default:
                return Point.Zero;
        }
    }

    // get the anchor offset. 
    // for example, if an element is a child of the canvas, if the localposition is (0, 0) and the anchor is middle center, 
    // the element will be drawn at the center of the canvas. 
    // this offset is added to the absolute position when drawing the element.
    public Point GetAnchorOffset()
    {
        if (_parent == null)
            return Point.Zero;

        float parentWidth  = _parent.size.X;
        float parentHeight = _parent.size.Y;

        float x = anchor switch
        {
            Anchor.TopLeft    or Anchor.MiddleLeft   or Anchor.BottomLeft   => 0f,
            Anchor.TopCenter  or Anchor.MiddleCenter or Anchor.BottomCenter => parentWidth * 0.5f,
            Anchor.TopRight   or Anchor.MiddleRight  or Anchor.BottomRight  => parentWidth,
            _ => 0f
        };

        float y = anchor switch
        {
            Anchor.TopLeft    or Anchor.TopCenter    or Anchor.TopRight    => 0f,
            Anchor.MiddleLeft or Anchor.MiddleCenter or Anchor.MiddleRight => parentHeight * 0.5f,
            Anchor.BottomLeft or Anchor.BottomCenter or Anchor.BottomRight => parentHeight,
            _ => 0f
        };

        return new Point((int)x, (int)y);
    }

    public virtual void OnMouseEnter()
    {
    }

    public virtual void OnMouseHover()
    {
    }

    public virtual void OnMouseExit()
    {
    }

    public virtual void OnPressed()
    {
    }

    public virtual void OnReleased()
    {
    }
}