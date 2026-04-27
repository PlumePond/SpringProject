using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpringProject.Core.Content;
using SpringProject.Core.Debugging;
using SpringProject.Core.Editor;
using SpringProject.Core.UserInput;

namespace SpringProject.Core.AI;

public class Node(Point point)
{
    public Point Point { get; private set; } = point;
    public Node Parent { get; set; }

    // scores for a-star algorithm
    public float G { get; set; } = 0;
    public float H { get; set; } = 0;
    public float F => G + H;

    public void Reset()
    {
        G = 0;
        H = 0;
        Parent = null;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (Parent != null)
        {
            Debug.DrawLine(spriteBatch, Point, Parent.Point, Color.Black, 2);
        }
        
        var texture = TextureManager.Get("pathfinding_node_path");
        var destRect = new Rectangle(Point.X, Point.Y, texture.Width, texture.Height);
        var origin = new Vector2(texture.Width / 2, texture.Height / 2);
        spriteBatch.Draw(texture, destRect, null, Color.White, 0f, origin, SpriteEffects.None, 0);
    }
}

public static class Pathfinding
{
    static Grid _grid;
    static Rectangle _bounds;
    static int _resolution;
    static int _layer;

    static Dictionary<Point, Node> _nodeMap = new Dictionary<Point, Node>();

    static Node _start;
    static List<Node> _path = new List<Node>();
    static Node _goal;

    public static void Initialize(Grid grid, int layer, int resolution)
    {
        _grid = grid;
        _bounds = new Rectangle(0, 0, grid.size.X, grid.size.Y);
        _layer = layer;
        _resolution = resolution;

        PopulateNodes();
    }

    public static void PopulateNodes()
    {
        _nodeMap.Clear();

        if (_grid.layers.Length <= _layer)
        {
            throw new System.Exception($"Pathfinding: Grid does not contain layer index {_layer}.");
        }

        for (int x = 0; x < _bounds.Width; x++)
        {
            for (int y = 0; y < _bounds.Height; y++)
            {
                var point = new Point(x * _resolution + _resolution / 2, y * _resolution + _resolution / 2);
                
                if (_grid.InsideObject(point, _layer, out var obj))
                {
                    if (obj.data.solid) continue;
                }
                
                var node = new Node(point);
                _nodeMap[point] = node;
            }
        }
    }

    public static Node GetNodeAtPoint(Point point)
    {
        _nodeMap.TryGetValue(point, out var node);
        return node;
    }

    static List<Node> GetNeighbors(Node node)
    {
        var neighbors = new List<Node>();

        Point[] offsets =
        {
            new Point(0, -_resolution),
            new Point(_resolution, 0),
            new Point(0, _resolution),
            new Point(-_resolution, 0),
        };

        foreach (var offset in offsets)
        {
            var neighbor = GetNodeAtPoint(node.Point + offset);

            if (neighbor != null)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    static float Heuristic(Node a, Node b)
    {
        return Math.Abs(a.Point.X - b.Point.X) + Math.Abs(a.Point.Y - b.Point.Y);
    }
    
    public static List<Node> FindPath(Node start, Node goal)
    {
        if (start == null) return null;
        if (goal == null) return null;

        // reset all nodes 
        foreach (var node in _nodeMap.Values)
        {
            node.Reset();
        }

        var closedSet = new HashSet<Node>();
        var open = new PriorityQueue<Node, float>();
        var visited = new HashSet<Node>(); // nodes that have already been given a best G score

        start.G = 0; // cost from start to start is 0
        start.H = Heuristic(start, goal);
        open.Enqueue(start, start.F);

        while (open.Count > 0)
        {
            var current = open.Dequeue();
            if (closedSet.Contains(current)) continue;
            
            if (current == goal) return ReconstructPath(current);

            // move current node from open to closed
            closedSet.Add(current);

            // check all neighbors
            foreach (var neighbor in GetNeighbors(current))
            {
                // already checked this node
                if (closedSet.Contains(neighbor)) continue;

                // calculate g score
                var tentativeG = current.G + Heuristic(current, neighbor);

                if (!visited.Contains(neighbor) || tentativeG < neighbor.G)
                {
                    // the best path so far was found
                    visited.Add(neighbor);
                    neighbor.Parent = current;
                    neighbor.G = tentativeG;
                    neighbor.H = Heuristic(neighbor, goal);
                    open.Enqueue(neighbor, neighbor.F);
                }
            }
        }

        return null;
    }

    public static List<Node> FindPath(Point start, Point goal)
    {
        var startNode = GetClosestNode(start);
        var goalNode = GetClosestNode(goal);

        if (startNode == null) return null;
        if (goalNode == null) return null;

        return FindPath(startNode, goalNode);
    }

    public static List<Node> ReconstructPath(Node node)
    {
        List<Node> path = new List<Node>();

        while (node != null)
        {
            path.Add(node);
            node = node.Parent;
        }

        path.Reverse();
        return path;
    }

    public static Node GetClosestNode(Point point)
    {
        var snapped = new Point(
        (point.X / _resolution) * _resolution + _resolution / 2, 
        (point.Y / _resolution) * _resolution + _resolution / 2);

        return GetNodeAtPoint(snapped);
    }

    public static void Update(GameTime gameTime)
    {
        Point cursorPoint = Camera.Instance.ScreenToWorld(Input.Get("cursor").Vector, _grid.layers[_layer].ParallaxFactor).ToPoint();

        if (Input.Get("place").Pressed)
        {
            _start = GetClosestNode(cursorPoint);
            _path = FindPath(_start, _goal);
        }
        if (Input.Get("remove").Pressed)
        {
            _goal = GetClosestNode(cursorPoint);
            _path = FindPath(_start, _goal);
        }
    }

    public static void DrawDebug(SpriteBatch spriteBatch)
    {
        float parallaxFactor = _grid.layers[_layer].ParallaxFactor;
        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Camera.Instance.GetParallaxTransform(parallaxFactor));
        
        foreach (var node in _nodeMap.Values)
        {
            var texture = TextureManager.Get("pathfinding_node");
            var destRect = new Rectangle(node.Point.X, node.Point.Y, texture.Width, texture.Height);
            var origin = new Vector2(texture.Width / 2, texture.Height / 2);
            spriteBatch.Draw(texture, destRect, null, Color.White * 0.5f, 0f, origin, SpriteEffects.None, 0);
        }

        if (_path != null)
        {
            for (int i = 0; i < _path.Count; i++)
            {
                if (i + 1 < _path.Count)
                {
                    Debug.DrawLine(spriteBatch, _path[i].Point, _path[i + 1].Point, Color.Black, 2);
                }

                var texture = TextureManager.Get("pathfinding_node_path");
                var destRect = new Rectangle(_path[i].Point.X, _path[i].Point.Y, texture.Width, texture.Height);
                var origin = new Vector2(texture.Width / 2, texture.Height / 2);
                spriteBatch.Draw(texture, destRect, null, Color.White, 0f, origin, SpriteEffects.None, 0);
            }
        }

        if (_start != null)
        {
            var texture = TextureManager.Get("pathfinding_node_path");
            var destRect = new Rectangle(_start.Point.X, _start.Point.Y, texture.Width, texture.Height);
            var origin = new Vector2(texture.Width / 2, texture.Height / 2);
            spriteBatch.Draw(texture, destRect, null, Color.Lime, 0f, origin, SpriteEffects.None, 0);
        }

        if (_goal != null)
        {
            var texture = TextureManager.Get("pathfinding_node_path");
            var destRect = new Rectangle(_goal.Point.X, _goal.Point.Y, texture.Width, texture.Height);
            var origin = new Vector2(texture.Width / 2, texture.Height / 2);
            spriteBatch.Draw(texture, destRect, null, Color.Red, 0f, origin, SpriteEffects.None, 0);
        }

        spriteBatch.End();
    }
}