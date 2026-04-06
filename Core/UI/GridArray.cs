using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Content;
using SpringProject.Core.Debugging;
using SpringProject.Core.UI;
using SpringProject.Core.UserInput;

namespace SpringProject.Core;

public class GridArray : Element
{
    protected Point _gridSize;
    protected int _space;

    List<bool[]> _packingNodes = new();
    public int gridColumns { get; private set; }

    public int GridRows => _packingNodes.Count; 
    
    public GridArray(Point localPosition, Point size, Point gridSize, int space, Anchor anchor = Anchor.MiddleCenter) : base(localPosition, size, anchor)
    {
        _gridSize = gridSize;
        _space = space;

        InitGrid();
    }

    void InitGrid()
    {
        gridColumns = (size.X + _space) / (_gridSize.X + _space);
        _packingNodes.Clear();
    }

    void AddRow()
    {
        _packingNodes.Add(new bool[gridColumns]);
    }

    public override void AddChild(Element child)
    {
        base.AddChild(child);

        // i used desmos to figure this out. i heart desmos.
        int rowCapacity = ((size.X - _gridSize.X) / (_gridSize.X + _space)) + 1;
        int x = ((_children.Count - 1) % rowCapacity) * (_gridSize.X + _space);
        int y = ((_children.Count - 1) / rowCapacity) * (_gridSize.Y + _space);

        Point pos = new Point(x, y);

        child.SetLocalPosition(pos);

        // recalculate height
        int rows = _children.Count / rowCapacity;
        int height = ((rows + 1)* _gridSize.Y) + (_space * (rows - 1));
        size = new Point(size.X, height);
        ReCalculateBounds();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
    }

    public void Clear()
    {
        _children.Clear();
    }

    public void PackChildren()
    {
        // reset occupancy
        _packingNodes.Clear();

        // sort children by area (largest to smallest)
        var sortedChildren = _children.OrderByDescending(child =>
        {
            var nodeSize = ChildNodeSize(child);
            return nodeSize.X * nodeSize.Y;
        });

        foreach (var child in sortedChildren)
        {
            Point nodeSize = ChildNodeSize(child);
            Point gridPos = FindFreePosition(nodeSize);

            Occupy(gridPos.X, gridPos.Y, nodeSize);

            // convert grid coords to pixel coords
            int px = gridPos.X * (_gridSize.X + _space);
            int py = gridPos.Y * (_gridSize.Y + _space);
            child.SetLocalPosition(new Point(px, py));
        }

        RecalculateHeight();
    }

    void RecalculateHeight()
    {
        int rows = _packingNodes.Count;
        int height = (rows * _gridSize.Y) + (_space * (rows - 1));
        size = new Point(size.X, height);
        ReCalculateBounds();
    }

    Point ChildNodeSize(Element element)
    {
        var size = element.size;
        size.X = Math.Max(_gridSize.X, size.X);
        size.Y = Math.Max(_gridSize.Y, size.Y);

        return new Point(size.X / _gridSize.X, size.Y / _gridSize.Y);
    }

    Point FindFreePosition(Point nodeSize)
    {
        // ensure that there are at least enough rows for the child to fit
        while (_packingNodes.Count < nodeSize.Y)
        {
            AddRow();
        }

        int row = 0;

        while(true)
        {
            for (int column = 0; column <= gridColumns - nodeSize.X; column++)
            {
                if (CanFit(column, row, nodeSize))
                {
                    return new Point(column, row);
                }
            }

            row++;
            
            // if it has reached the bottom, then grow
            if (row + nodeSize.Y > _packingNodes.Count)
            {
                AddRow();
            }
        }   
    }

    bool CanFit(int column, int row, Point nodeSize)
    {
        if (row + nodeSize.Y > _packingNodes.Count)
        {
            return false;
        }

        // check through each column and row
        for (int dy = 0; dy < nodeSize.Y; dy++)
        {
            for (int dx = 0; dx < nodeSize.X; dx++)
            {
                if (_packingNodes[row + dy][column + dx])
                {
                    // if the packing nodes are already occupied, it cannot fit
                    return false;
                }
            }
        }
        // can fit
        return true;
    }

    void Occupy(int column, int row, Point nodeSize)
    {
        for (int dy = 0; dy < nodeSize.Y; dy++)
        {
            for (int dx = 0; dx < nodeSize.X; dx++)
            {
                _packingNodes[row + dy][column + dx] = true;
            }
        }
    }
}