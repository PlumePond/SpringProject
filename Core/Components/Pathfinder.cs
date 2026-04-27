using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.AI;
using SpringProject.Core.Editor;

namespace SpringProject.Core.Components;

public class Pathfinder : Component
{
    public Transform Target = null;

    protected virtual float PathfinderUpdateInterval => 0.5f;
    protected virtual float NodeThresholdX => 16f;
    protected virtual float NodeThresholdY => 32f;

    protected float _pathfinderUpdateTimer = 0.0f;
    protected List<Node> _path;
    protected int _currentNode = 0;

    public Action<Node> FollowPathEvent;

    public void HandlePathfinding(GameTime gameTime)
    {
        if (Target == null) return;

        _pathfinderUpdateTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_pathfinderUpdateTimer > PathfinderUpdateInterval)
        {
            var path = Pathfinding.FindPath(LevelObject.hitbox.Center, Target.position);

            if (path != null && path.Count > 0)
            {
                _path = path;
                _currentNode = 0;

                // find the closest node to current position to avoid snapping back
                float closest = float.MaxValue;
                for (int i = 0; i < _path.Count; i++)
                {
                    float dist = LevelObject.hitbox.Center.Distance(_path[i].Point);
                    if (dist < closest)
                    {
                        closest = dist;
                        _currentNode = i;
                    }
                }

                _pathfinderUpdateTimer = 0.0f;
            }
        }

        if (_path != null && _path.Count > 1)
        {
            FollowPathEvent?.Invoke(_path[_currentNode]);
            
            bool withinThresholdX = MathF.Abs(LevelObject.hitbox.Center.X - _path[_currentNode].Point.X) < NodeThresholdX;
            bool withinThresholdY = MathF.Abs(LevelObject.hitbox.Center.Y - _path[_currentNode].Point.Y) < NodeThresholdY;

            if (withinThresholdX || withinThresholdY)
            {
                if (_currentNode + 1 < _path.Count)
                {
                    _currentNode++;
                }
            }
        }
    }

    public override void DrawDebug(SpriteBatch spriteBatch)
    {
        base.DrawDebug(spriteBatch);

        if (_path == null) return;

        foreach (var node in _path)
        {
            node.Draw(spriteBatch);
        }
    }
}