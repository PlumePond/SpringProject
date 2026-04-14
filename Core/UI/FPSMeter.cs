using System;
using System.Collections.Generic;
using System.Linq;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Content;
using SpringProject.Core.Debugging;

namespace SpringProject.Core.UI;

public class FPSMeter : TextElement
{
    public long TotalFrames { get; private set; }
    public float TotalSeconds { get; private set; }
    public float AverageFramesPerSecond { get; private set; }
    public float CurrentFramesPerSecond { get; private set; }

    const float UPDATE_INTERVAL = 1.0f;
    const int MAXIMUM_SAMPLES = 100;

    float _timer = 0.0f;

    private Queue<float> _sampleBuffer = new();

    public FPSMeter(Point localPosition, Font font, string text, Color color, Anchor anchor = Anchor.MiddleCenter) : base(localPosition, font, text, color, anchor)
    {
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        CurrentFramesPerSecond = 1.0f / deltaTime;

        _sampleBuffer.Enqueue(CurrentFramesPerSecond);

        if (_sampleBuffer.Count > MAXIMUM_SAMPLES)
        {
            _sampleBuffer.Dequeue();
            AverageFramesPerSecond = _sampleBuffer.Average(i => i);
        }
        else
        {
            AverageFramesPerSecond = CurrentFramesPerSecond;
        }

        TotalFrames++;
        TotalSeconds += deltaTime;

        _timer += deltaTime;
        if (_timer >= UPDATE_INTERVAL)
        {
            SetText($"FPS: ({(int)AverageFramesPerSecond})");
            _timer = 0.0f;
        }
    }
}