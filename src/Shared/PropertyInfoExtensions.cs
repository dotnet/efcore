// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

// ReSharper disable once CheckNamespace

namespace System.Reflection;

[DebuggerStepThrough]
internal static class PropertyInfoExtensions
{
    public static bool IsStatic(this PropertyInfo property)
        => (property.GetMethod ?? property.SetMethod)!.IsStatic;

    public static bool IsCandidateProperty(this MemberInfo memberInfo, bool needsWrite = true, bool publicOnly = true)
        => memberInfo is PropertyInfo propertyInfo
            ? !propertyInfo.IsStatic()
            && propertyInfo.CanRead
            && (!needsWrite || propertyInfo.FindSetterProperty() != null)
            && propertyInfo.GetMethod != null
            && (!publicOnly || propertyInfo.GetMethod.IsPublic)
            && propertyInfo.GetIndexParameters().Length == 0
            : memberInfo is FieldInfo { IsStatic: false } fieldInfo
            && (!publicOnly || fieldInfo.IsPublic);

    public static bool IsIndexerProperty(this PropertyInfo propertyInfo)
    {
        var indexParams = propertyInfo.GetIndexParameters();
        return indexParams.Length == 1
            && indexParams[0].ParameterType == typeof(string);
    }

    public static PropertyInfo? FindGetterProperty(this PropertyInfo propertyInfo)
        => propertyInfo.DeclaringType!
            .GetPropertiesInHierarchy(propertyInfo.GetSimpleMemberName())
            .FirstOrDefault(p => p.GetMethod != null);

    public static PropertyInfo? FindSetterProperty(this PropertyInfo propertyInfo)
        => propertyInfo.DeclaringType!
            .GetPropertiesInHierarchy(propertyInfo.GetSimpleMemberName())
            .FirstOrDefault(p => p.SetMethod != null);
}
