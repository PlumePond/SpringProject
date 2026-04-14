using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpringProject.Core.Editor;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace SpringProject.Core;

public class RayHit(Point hitPoint, LevelObject hitObject)
{
    public readonly Point HitPoint = hitPoint;
    public readonly LevelObject HitObject = hitObject;
}

public class RayData(List<RayHit> hits, Vector2 start, Vector2 end)
{
    public readonly List<RayHit> Hits = hits;
    public readonly Vector2 Start = start;
    public readonly Vector2 End = end;
}

public static class Raycasting
{
    public static bool Cast(Grid grid, int layer, Vector2 origin, Vector2 dir, float distance, out RayData rayData, LevelObject ignoreObject = null, bool penetrate = false)
    {
        var levelObjects = grid.layers[layer].LevelObjects.OrderBy(a => Vector2.Distance(a.transform.position.ToVector2(), origin)).ToList();
        
        dir.Normalize();
        var hits = new List<RayHit>();
        var rayOrigin = origin;
        var rayEnd = rayOrigin + dir * distance;

        bool done = false;
        foreach (var levelObject in levelObjects)
        {
            if (done) break;
            if (levelObject == ignoreObject) continue;
            
            var vertices = levelObject.hitbox.Vertices().ToList();
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector2 a = vertices[i];
                Vector2 b = vertices[(i + 1) % vertices.Count];

                if (RaySegmentIntersect(rayOrigin, dir, a, b, out var t))
                {
                    if (t > distance) break;
                    
                    Vector2 hitPos = rayOrigin + dir * t;
                    hits.Add(new RayHit(hitPos.ToPoint(), levelObject));
                    break;
                }
            }

            // stop at the first object if penetrating
            if (hits.Count > 0 && !penetrate) done = true;
        }

        var hitEnd = (hits.Count > 0 && !penetrate) ? hits[0].HitPoint.ToVector2() : rayEnd;
        rayData = new RayData(hits, origin, hitEnd);
        
        return hits.Count > 0;
    }
    
    public static bool Cast(Grid grid, int layer, Vector2 origin, Vector2 end, out RayData rayData, LevelObject ignoreObject = null, bool penetrate = false)
    {
        Vector2 dir = Vector2.Normalize(end - origin);
        float distance = Vector2.Distance(origin, end);
        
        return Cast(grid, layer, origin, dir, distance, out rayData, ignoreObject, penetrate);
    }

    static bool RaySegmentIntersect(Vector2 origin, Vector2 dir, Vector2 a, Vector2 b, out float t)
    {
        t = 0f;
        Vector2 edge = b - a;
        float denominator = dir.X * edge.Y - dir.Y * edge.X;
        
        // check if it is parallel
        if (MathF.Abs(denominator) < 1e-6f) return false;

        Vector2 toA = a - origin;
        t = (toA.X * edge.Y - toA.Y * edge.X) / denominator;
        float u = (toA.X * dir.Y - toA.Y * dir.X) / denominator;

        return t >= -1e-4f && u >= 0f && u <= 1f;
    }
}