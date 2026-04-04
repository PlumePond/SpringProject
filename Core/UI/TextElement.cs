using System;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpringProject.Core.UI;

public class TextElement : Element
{
    string _text = "";
    SpriteFontBase _font = null;

    public TextElement(Point localPosition, Vector2 localScale, SpriteFontBase font, string text, Color color, Origin origin = Origin.MiddleCenter, Anchor anchor = Anchor.MiddleCenter) : base(localPosition, Point.Zero, localScale, origin, anchor)
    {
        this.color = color;
        _font = font;
        _text = text;
        size = _font.MeasureString(_text, AbsoluteScale).ToPoint();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawString(_font, _text, AbsolutePosition.ToVector2(), color, 0, Vector2.Zero, AbsoluteScale);

        base.Draw(spriteBatch);
    }

    public void SetText(string text)
    {
        _text = text;
        size = _font.MeasureString(_text, AbsoluteScale).ToPoint();
    }
}