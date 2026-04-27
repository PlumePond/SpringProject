using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ParameterAttribute(string label = null, float min = 0f, float max = 1f) : Attribute
{
    public string Label { get; } = label;
    public float Min { get; } = min;
    public float Max { get; } = max;
}