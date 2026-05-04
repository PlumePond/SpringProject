using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpringProject.Core.Debugging;
using SpringProject.Core.UserInput;

namespace SpringProject.Core.UI;

public class Element
{
    public Point localPosition { get; protected set; }
    public Point size { get; protected set; }
    public Vector2 localScale { get; protected set; } = Vector2.One;
    public Anchor anchor { get; protected set; }

    public Point originOffset { get; protected set; }
    public Point anchorOffset { get; protected set; }

    public Color color { get; protected set; } = Color.White;

    public bool Active { get; protected set; } = true;

    protected bool _hovering = false;

    protected bool _prevHovering = false;
    protected bool _prevPressed = false;

    public Rectangle Bounds { get; protected set; }
    public Rectangle? ClippingBounds { get; protected set; } = null;

    public Action<Element> AddChildEvent;

    public List<Element> Children => _children;

    protected List<Element> _children;
    protected Element _parent;

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

    public Element(Point localPosition, Point size, Anchor anchor = Anchor.MiddleCenter)
    {
        this.localPosition = localPosition;
        this.size = size;
        this.anchor = anchor;

        _children = new List<Element>();
        ReCalculateOffsets();
    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        DrawChildren(spriteBatch);
    }

    public virtual bool WithinBounds(Point point)
    {
        if (ClippingBounds.HasValue)
        {
            return Bounds.Contains(point) && ClippingBounds.Value.Contains(point);
        }

        return Bounds.Contains(point);
    }

    public virtual void Update(GameTime gameTime)
    {
        Point mousePoint = Input.Get("cursor").Point;

        // convert mouse position to a point in the window. it is already in window coordinates, but we need to convert it to a point and account for the pixel scale of the UI
        Point mousePosition = new Point(mousePoint.X / Main.Settings.UISize, mousePoint.Y / Main.Settings.UISize);

        // check if the mouse is hovering over the element
        _hovering = WithinBounds(mousePosition);
    
        bool isPressed = Input.Get("ui_click").Holding;

        if (_hovering && !_prevHovering)
        {
            OnMouseEnter();
        }
        else if (_hovering)
        {
            OnMouseHover();
        }
        else if (!_hovering && _prevHovering)
        {
            OnMouseExit();
        }

        if (_hovering && isPressed && !_prevPressed)
        {
            OnPressed();
        }
        else if (_hovering && isPressed)
        {
            OnHeld();
        }
        else if (_hovering && !isPressed && _prevPressed)
        {
            OnReleased();
        }
        else if (!_hovering && !isPressed && _prevPressed)
        {
            OnReleasedOff();
        }

        if (!_hovering && isPressed && !_prevPressed)
        {
            OnPressedOff();
        }
        

        _prevHovering = _hovering;
        _prevPressed = isPressed;

        UpdateChildren(gameTime);
    }

    public virtual void OnEnable()
    {
        
    }

    public virtual void OnDisable()
    {
        
    }

    public void UpdateChildren(GameTime gameTime)
    {
        foreach (var child in _children.ToList())
        {
            if (child.Active)
            {
                child.Update(gameTime);
            }
        }
    }

    public void DrawChildren(SpriteBatch spriteBatch)
    {
        foreach (var child in _children)
        {
            if (child.Active)
            {
                child.Draw(spriteBatch);
            }
        }
    }

    public virtual void AddChild(Element child)
    {
        _children.Add(child);
        child._parent = this;
        child.anchorOffset = child.GetAnchorOffset();

        AddChildEvent?.Invoke(child);
        child.OnParented(this);

        if (Active)
        {
            child.SetActive(true);
        }
    }

    public virtual void OnParented(Element parent)
    {
        ReCalculateOffsets();
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

    public void SetColor(Color color)
    {
        this.color = color;
    }

    public void ReCalculateOffsets()
    {
        originOffset = GetOriginOffset();
        anchorOffset = GetAnchorOffset();

        ReCalculateBounds();

        foreach (var child in _children)
        {
            child.ReCalculateOffsets();
            ReCalculateBounds();
        }
    }

    public void ReCalculateBounds()
    {
        Bounds = new Rectangle(AbsolutePosition, size * AbsoluteScale.ToPoint());
    }

    public Point GetOriginOffset()
    {
        switch (anchor)
        {
            case Anchor.TopLeft:
                return Point.Zero;
            case Anchor.TopCenter:
                return new Point(-size.X / 2, 0);
            case Anchor.TopRight:
                return new Point(-size.X, 0);
            case Anchor.MiddleLeft:
                return new Point(0, -size.Y / 2);
            case Anchor.MiddleCenter:
                return new Point(-size.X / 2, -size.Y / 2);
            case Anchor.MiddleRight:
                return new Point(-size.X, -size.Y / 2);
            case Anchor.BottomLeft:
                return new Point(0, -size.Y);
            case Anchor.BottomCenter:
                return new Point(-size.X / 2, -size.Y);
            case Anchor.BottomRight:
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

    public virtual void OnHeld()
    {
        
    }

    public virtual void OnReleased()
    {
    }

    public virtual void OnReleasedOff()
    {
        
    }

    public virtual void OnPressedOff()
    {
        
    }

    public void SetActive(bool active)
    {
        Active = active;

        if (!active)
        {
            OnDisable();
        }
        else
        {
            OnEnable();
        }
    }

    public void SetClippingBounds(Rectangle bounds)
    {
        ClippingBounds = bounds;

        // recursively set all children's clipping bounds
        foreach (var child in _children)
        {
            child.SetClippingBounds(bounds);
        }
    }
}