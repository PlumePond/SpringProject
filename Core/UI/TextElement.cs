using System;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Content;
using SpringProject.Core.Debugging;

namespace SpringProject.Core.UI;

public class TextElement : Element
{
    string _text = "";
    Font _font = null;

    public TextElement(Point localPosition, Font font, string text, Color color, Anchor anchor = Anchor.MiddleCenter) : base(localPosition, Point.Zero, anchor)
    {
        this.color = color;
        _font = font;
        _text = text;
        size = _font.FontBase.MeasureString(_text, AbsoluteScale).ToPoint() + _font.Offset.ToPoint();
        
        ReCalculateOffsets();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        //Debug.DrawRectangle(spriteBatch, Bounds, Color.Lime);
        spriteBatch.DrawString(_font.FontBase, _text, AbsolutePosition.ToVector2() + _font.Offset, color, 0, Vector2.Zero, AbsoluteScale);

        base.Draw(spriteBatch);
    }

    public void SetText(string text)
    {
        _text = text;
        size = _font.FontBase.MeasureString(_text, AbsoluteScale).ToPoint() + _font.Offset.ToPoint();
        ReCalculateOffsets();
    }
}