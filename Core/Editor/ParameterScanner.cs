using System.Collections.Generic;
using System.Reflection;

public static class ParameterScanner
{
    public static List<ParameterDescriptor> Scan(object target)
    {
        var result = new List<ParameterDescriptor>();
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        foreach (var member in target.GetType().GetMembers(flags))
        {
            var attribute = member.GetCustomAttribute<ParameterAttribute>();

            if (attribute == null) continue;
            if (member is not FieldInfo and not PropertyInfo) continue;

            result.Add(new ParameterDescriptor(target, member, attribute));
        }

        return result;
    }
}