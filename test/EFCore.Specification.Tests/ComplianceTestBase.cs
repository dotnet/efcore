// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class ComplianceTestBase
{
    protected abstract Assembly TargetAssembly { get; }
    protected virtual ICollection<Type> IgnoredTestBases { get; } = new List<Type>();

    [ConditionalFact]
    public virtual void All_test_bases_must_be_implemented()
    {
        var concreteTests = TargetAssembly.GetTypes().Where(
                c => c.BaseType != typeof(object)
                    && !c.IsAbstract
                    && (c.IsPublic || c.IsNestedPublic))
            .ToList();
        var nonImplementedBases
            = (from baseType in GetBaseTestClasses()
               where !IgnoredTestBases.Contains(baseType)
                   && baseType != typeof(NonSharedModelTestBase)
                   && !concreteTests.Any(c => Implements(c, baseType))
               select baseType.FullName)
            .ToList();

        Assert.False(
            nonImplementedBases.Count > 0,
            "\r\n-- Missing derived classes for --\r\n" + string.Join(Environment.NewLine, nonImplementedBases));
    }

    protected virtual IEnumerable<Type> GetBaseTestClasses()
        => typeof(ComplianceTestBase).Assembly.ExportedTypes.Where(t => t.Name.Contains("TestBase"));

    private static bool Implements(Type type, Type interfaceOrBaseType)
        => (type.IsPublic || type.IsNestedPublic) && interfaceOrBaseType.IsGenericTypeDefinition
            ? GetGenericTypeImplementations(type, interfaceOrBaseType).Any()
            : interfaceOrBaseType.IsAssignableFrom(type);

    private static IEnumerable<Type> GetGenericTypeImplementations(Type type, Type interfaceOrBaseType)
    {
        var typeInfo = type.GetTypeInfo();
        if (!typeInfo.IsGenericTypeDefinition)
        {
            var baseTypes = interfaceOrBaseType.IsInterface
                ? typeInfo.ImplementedInterfaces
                : GetBaseTypes(type);
            foreach (var baseType in baseTypes)
            {
                if (baseType.IsGenericType
                    && baseType.GetGenericTypeDefinition() == interfaceOrBaseType)
                {
                    yield return baseType;
                }
            }

            if (type.IsGenericType
                && type.GetGenericTypeDefinition() == interfaceOrBaseType)
            {
                yield return type;
            }
        }
    }

    private static IEnumerable<Type> GetBaseTypes(Type type)
    {
        type = type.BaseType;

        while (type != null)
        {
            yield return type;

            type = type.BaseType;
        }
    }
}
