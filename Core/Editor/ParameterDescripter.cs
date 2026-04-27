using System;
using System.Reflection;

public class ParameterDescriptor
{
    public string Label { get; }
    public Type ValueType { get; }
    public float Min { get; }
    public float Max { get; }

    private readonly object _target;
    private readonly MemberInfo _member;

    public ParameterDescriptor(object target, MemberInfo member, ParameterAttribute attribute)
    {
        _target = target;
        _member = member;
        Label = attribute.Label ?? member.Name;
        Min = attribute.Min;
        Max = attribute.Max;
        ValueType = member is FieldInfo f ? f.FieldType : ((PropertyInfo)member).PropertyType;
    }

    public object GetValue()
    {
        return _member is FieldInfo f ? f.GetValue(_target) : ((PropertyInfo)_member).GetValue(_target);
    }

    public void SetValue(object value)
    {
        if (_member is FieldInfo f)
        {
            f.SetValue(_target, value);
        }
        else
        {
            ((PropertyInfo)_member).SetValue(_target, value);
        }
    }
}