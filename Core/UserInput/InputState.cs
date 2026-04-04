using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SpringProject.Core.UserInput;

public class InputState
{
    public bool Pressed { get; protected set; } = false;
    public bool Holding { get; protected set; } = false;
    public bool Released { get; protected set; } = false;

    public Point Point { get; protected set; } = Point.Zero;
    public float Float { get; protected set; } = 0.0f;
    public int Int { get; protected set; } = 0;
    public Vector2 Vector { get; protected set; } = Vector2.Zero;
    
    public Point DeltaPoint { get; protected set; } = Point.Zero;
    public float DeltaFloat { get; protected set; } = 0.0f;
    public int DeltaInt { get; protected set; } = 0;
    public Vector2 DeltaVector { get; protected set; } = Vector2.Zero;

    protected Point _prevPoint = Point.Zero;
    protected float _prevFloat = 0.0f;
    protected int _prevInt = 0;
    protected Vector2 _prevVector = Vector2.Zero;

    protected List<InputBinding> _bindings = new List<InputBinding>();

    public Action PressedEvent { get; set; }
    public Action ReleasedEvent { get; set; }

    public virtual void AddBinding(InputBinding binding)
    {
        _bindings.Add(binding);
    }

    public virtual void Update()
    {
        bool anyPressed = false;
        bool anyHolding = false;
        bool anyReleased = false;

        Point tempPoint = Point.Zero;
        float tempFloat = 0.0f;
        int tempInt = 0;
        Vector2 tempVector = Vector2.Zero;

        foreach (var binding in _bindings)
        {
            binding.Update();

            if (binding.Pressed)
            {
                anyPressed = true;
            }
            if (binding.Holding)
            {
                anyHolding = true;
            }
            if (binding.Released)
            {
                anyReleased = true;
            }

            tempPoint += binding.Point;
            tempFloat += binding.Float;
            tempInt += binding.Int;
            tempVector += binding.Vector;
        }

        Pressed = anyPressed;
        Holding = anyHolding;
        Released = anyReleased;

        Point = tempPoint;
        Float = tempFloat;
        Int = tempInt;
        Vector = tempVector;

        if (Pressed)
        {
            PressedEvent?.Invoke();
        }
        else if (Released)
        {
            ReleasedEvent?.Invoke();
        }
        
        DeltaPoint = Point - _prevPoint;
        DeltaFloat = Float - _prevFloat;
        DeltaInt = Int - _prevInt;
        DeltaVector = Vector - _prevVector;

        _prevPoint = Point;
        _prevFloat = Float;
        _prevInt = Int;
        _prevVector = Vector;
    }
}