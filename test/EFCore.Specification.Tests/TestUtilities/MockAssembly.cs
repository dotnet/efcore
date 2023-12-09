// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class MockAssembly : Assembly
{
    private readonly IEnumerable<TypeInfo> _definedTypes;
    private readonly ReflectionTypeLoadException _exception;

    public static Assembly Create(params Type[] definedTypes)
        => Create(
            definedTypes,
            definedTypes.Length == 0
                ? null
                : new MockMethodInfo(definedTypes.First()));

    public static Assembly Create(Type[] definedTypes, MethodInfo entryPoint, ReflectionTypeLoadException exception = null)
    {
        var definedTypeInfos = definedTypes.Select(t => t.GetTypeInfo()).ToArray();

        return new MockAssembly(definedTypeInfos, entryPoint, exception);
    }

    public MockAssembly(IEnumerable<TypeInfo> definedTypes, MethodInfo entryPoint, ReflectionTypeLoadException exception)
    {
        _definedTypes = definedTypes;
        _exception = exception;
        EntryPoint = entryPoint;
    }

    public override MethodInfo EntryPoint { get; }

    public override IEnumerable<TypeInfo> DefinedTypes
        => _exception != null ? throw _exception : _definedTypes;

    public override AssemblyName GetName()
        => new(nameof(MockAssembly));

    public override string FullName
        => nameof(MockAssembly);

    public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        => (Attribute[])Array.CreateInstance(attributeType, 0);
}
