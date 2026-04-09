using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Content;
using SpringProject.Core.Debugging;
using SpringProject.Core.UI;
using SpringProject.Core.UserInput;

namespace SpringProject.Core;

public class ScrollRect : Element
{
    protected int _scrollOffset;
    protected int _scrollSpeed = 1;

    int _contentHeight;
    protected bool _canScroll = true;

    public Action<float> ScrollEvent;
    public Action<bool> UpdateCanScrollEvent;
    
    public ScrollRect(Point localPosition, Point size, Anchor anchor = Anchor.MiddleCenter) : base(localPosition, size, anchor)
    {
        
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (_children.Count < 1)
        {
            return;
        }

        int scrollDelta = Input.Get("scroll").DeltaInt / 120;

        if (_hovering && scrollDelta != 0)
        {
            _scrollOffset -= scrollDelta * _scrollSpeed;
            _scrollOffset = Math.Clamp(_scrollOffset, 0, _contentHeight - size.Y);
            
            float ratio = (_contentHeight - size.Y) > 0 ? (float)_scrollOffset / (_contentHeight - size.Y) : 0f;
            ScrollEvent?.Invoke(ratio);
        }
        else
        {
            // clamp scroll offset
            _scrollOffset = Math.Clamp(_scrollOffset, 0, _contentHeight - size.Y);
        }

        Point childPos = new Point(0, -_scrollOffset);

        foreach (var child in _children)
        {
            child.SetLocalPosition(childPos);
        }

        //CalculateContentHeight();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.End();

        RasterizerState rasterizerState = new RasterizerState
        {
            ScissorTestEnable = true
        };

        Rectangle originalScissor = Main.Graphics.ScissorRectangle;
        Rectangle scissorRect = new Rectangle(AbsolutePosition * new Point(Main.Settings.UISize), size * AbsoluteScale.ToPoint() * new Point(Main.Settings.UISize));
        Main.Graphics.ScissorRectangle = scissorRect;

        // draw children
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, rasterizerState, null, Main.UIMatrtix);
        
        DrawChildren(spriteBatch);

        spriteBatch.End();

        Main.Graphics.ScissorRectangle = originalScissor;

        // restart the previous spritebatch
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Main.UIMatrtix);

        //Debug.DrawRectangleOutline(spriteBatch, Bounds, Color.Lime, 1);
    }

    public override void AddChild(Element child)
    {
        base.AddChild(child);

        CalculateContentHeight();
        child.AddChildEvent += OnChildAddChild;
    }

    public void SetScroll(float ratio)
    {
        ratio = Math.Clamp(ratio, 0f, 1f);
        _scrollOffset = (int)(ratio * (_contentHeight - size.Y));

        Point childPos = new Point(0, -_scrollOffset);
        foreach (var child in _children)
        {
            child.SetLocalPosition(childPos);
        }
    }

    void CalculateContentHeight()
    {
        int maxY = size.Y;

        foreach (var child in _children)
        {
            int bottom = child.localPosition.Y + child.size.Y;
            if (bottom > maxY) maxY = bottom;
        }

        _contentHeight = maxY;

        CalculateCanScroll();
    }

    void OnChildAddChild(Element child)
    {
        CalculateContentHeight();
    }
    
    void CalculateCanScroll()
    {
        if (_contentHeight - size.Y > 0 && !_canScroll)
        {
            _canScroll = true;
            UpdateCanScrollEvent?.Invoke(_canScroll);
        }
        else if (_contentHeight - size.Y <= 0 && _canScroll)
        {
            _canScroll = false;
            UpdateCanScrollEvent?.Invoke(_canScroll);
        }
    }
}