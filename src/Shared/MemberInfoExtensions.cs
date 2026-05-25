// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace System.Reflection;

internal static class EntityFrameworkMemberInfoExtensions
{
    public static MemberInfo ResolveMemberForType(this MemberInfo memberInfo, Type clrType)
    {
        if (memberInfo.DeclaringType is { IsInterface: true } interfaceType
            && interfaceType != clrType
            && interfaceType.IsAssignableFrom(clrType)
            && memberInfo is PropertyInfo propertyInfo)
        {
            var propertyGetter = propertyInfo.GetMethod;
            if (propertyGetter != null)
            {
                var interfaceMapping = clrType.GetInterfaceMap(interfaceType);
                var index = Array.FindIndex(interfaceMapping.InterfaceMethods, p => p.Equals(propertyGetter));
                if (index >= 0)
                {
                    var targetMethod = interfaceMapping.TargetMethods[index];
                    foreach (var runtimeProperty in clrType.GetProperties(
                                 BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if (targetMethod.Equals(runtimeProperty.GetMethod))
                        {
                            return runtimeProperty;
                        }
                    }
                }
            }
        }

        return memberInfo;
    }

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

    public static Type GetMemberType(this MemberInfo memberInfo)
        => (memberInfo as PropertyInfo)?.PropertyType ?? ((FieldInfo)memberInfo).FieldType;

    public static bool IsSameAs(this MemberInfo? propertyInfo, MemberInfo? otherPropertyInfo)
        => propertyInfo == null
            ? otherPropertyInfo == null
            : (otherPropertyInfo != null
                && (Equals(propertyInfo, otherPropertyInfo)
                    || (propertyInfo.Name == otherPropertyInfo.Name
                        && propertyInfo.DeclaringType != null
                        && otherPropertyInfo.DeclaringType != null
                        && (propertyInfo.DeclaringType == otherPropertyInfo.DeclaringType
                            || propertyInfo.DeclaringType.GetTypeInfo().IsSubclassOf(otherPropertyInfo.DeclaringType)
                            || otherPropertyInfo.DeclaringType.GetTypeInfo().IsSubclassOf(propertyInfo.DeclaringType)
                            || propertyInfo.DeclaringType.GetTypeInfo().ImplementedInterfaces.Contains(otherPropertyInfo.DeclaringType)
                            || otherPropertyInfo.DeclaringType.GetTypeInfo().ImplementedInterfaces
                                .Contains(propertyInfo.DeclaringType)))));

    public static bool IsOverriddenBy(this MemberInfo? propertyInfo, MemberInfo? otherPropertyInfo)
        => propertyInfo == null
            ? otherPropertyInfo == null
            : (otherPropertyInfo != null
                && (Equals(propertyInfo, otherPropertyInfo)
                    || (propertyInfo.Name == otherPropertyInfo.Name
                        && propertyInfo.DeclaringType != null
                        && otherPropertyInfo.DeclaringType != null
                        && (propertyInfo.DeclaringType == otherPropertyInfo.DeclaringType
                            || otherPropertyInfo.DeclaringType.GetTypeInfo().IsSubclassOf(propertyInfo.DeclaringType)
                            || otherPropertyInfo.DeclaringType.GetTypeInfo().ImplementedInterfaces
                                .Contains(propertyInfo.DeclaringType)))));

    public static string GetSimpleMemberName(this MemberInfo member)
    {
        var name = member.Name;
        var index = name.LastIndexOf('.');
        return index >= 0 ? name[(index + 1)..] : name;
    }

    public static bool IsReallyVirtual(this MethodInfo method)
        => method is { IsVirtual: true, IsFinal: false };
}
