// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InMemoryTypeMappingSource : TypeMappingSource
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InMemoryTypeMappingSource(TypeMappingSourceDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override CoreTypeMapping? FindMapping(in TypeMappingInfo mappingInfo)
    {
        var clrType = mappingInfo.ClrType;
        Check.DebugAssert(clrType != null, "ClrType is null");

        var jsonValueReaderWriter = Dependencies.JsonValueReaderWriterSource.FindReaderWriter(clrType);

        if (clrType.IsValueType
            || clrType == typeof(string)
            || (clrType == typeof(byte[]) && mappingInfo.ElementTypeMapping == null))
        {
            return CreateMapping(clrType, jsonValueReaderWriter: jsonValueReaderWriter);
        }

        if (clrType.FullName == "NetTopologySuite.Geometries.Geometry"
            || clrType.GetBaseTypes().Any(t => t.FullName == "NetTopologySuite.Geometries.Geometry"))
        {
            var comparer = (ValueComparer)Activator.CreateInstance(typeof(GeometryValueComparer<>).MakeGenericType(clrType))!;

            return CreateMapping(clrType, comparer, comparer, jsonValueReaderWriter);
        }

        return base.FindMapping(mappingInfo);
    }

    private static InMemoryTypeMapping CreateMapping(
        Type clrType,
        ValueComparer? comparer = null,
        ValueComparer? keyComparer = null,
        JsonValueReaderWriter? jsonValueReaderWriter = null)
        => clrType switch
        {
            _ when clrType == typeof(bool) => Create<bool>(comparer, keyComparer, jsonValueReaderWriter),
            _ when clrType == typeof(byte) => Create<byte>(comparer, keyComparer, jsonValueReaderWriter),
            _ when clrType == typeof(sbyte) => Create<sbyte>(comparer, keyComparer, jsonValueReaderWriter),
            _ when clrType == typeof(char) => Create<char>(comparer, keyComparer, jsonValueReaderWriter),
            _ when clrType == typeof(short) => Create<short>(comparer, keyComparer, jsonValueReaderWriter),
            _ when clrType == typeof(ushort) => Create<ushort>(comparer, keyComparer, jsonValueReaderWriter),
            _ when clrType == typeof(int) => Create<int>(comparer, keyComparer, jsonValueReaderWriter),
            _ when clrType == typeof(uint) => Create<uint>(comparer, keyComparer, jsonValueReaderWriter),
            _ when clrType == typeof(long) => Create<long>(comparer, keyComparer, jsonValueReaderWriter),
            _ when clrType == typeof(ulong) => Create<ulong>(comparer, keyComparer, jsonValueReaderWriter),
            _ when clrType == typeof(float) => Create<float>(comparer, keyComparer, jsonValueReaderWriter),
            _ when clrType == typeof(double) => Create<double>(comparer, keyComparer, jsonValueReaderWriter),
            _ when clrType == typeof(decimal) => Create<decimal>(comparer, keyComparer, jsonValueReaderWriter),
            _ when clrType == typeof(string) => Create<string>(comparer, keyComparer, jsonValueReaderWriter),
            _ when clrType == typeof(Guid) => Create<Guid>(comparer, keyComparer, jsonValueReaderWriter),
            _ when clrType == typeof(DateTime) => Create<DateTime>(comparer, keyComparer, jsonValueReaderWriter),
            _ when clrType == typeof(DateTimeOffset) => Create<DateTimeOffset>(comparer, keyComparer, jsonValueReaderWriter),
            _ when clrType == typeof(DateOnly) => Create<DateOnly>(comparer, keyComparer, jsonValueReaderWriter),
            _ when clrType == typeof(TimeOnly) => Create<TimeOnly>(comparer, keyComparer, jsonValueReaderWriter),
            _ when clrType == typeof(TimeSpan) => Create<TimeSpan>(comparer, keyComparer, jsonValueReaderWriter),
            _ when clrType == typeof(byte[]) => Create<byte[]>(comparer, keyComparer, jsonValueReaderWriter),
            _ => CreateMappingWithReflection(clrType, comparer, keyComparer, jsonValueReaderWriter)
        };

    private static InMemoryTypeMapping Create<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        ValueComparer? comparer,
        ValueComparer? keyComparer,
        JsonValueReaderWriter? jsonValueReaderWriter)
        => comparer is null && keyComparer is null && jsonValueReaderWriter is null
            ? InMemoryTypeMapping<T>.Default
            : new InMemoryTypeMapping<T>(comparer, keyComparer, jsonValueReaderWriter);

    [UnconditionalSuppressMessage(
        "AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.",
        Justification = "The type mapping source is not used at runtime by NativeAOT applications, which use a compiled model instead.")]
    private static InMemoryTypeMapping CreateMappingWithReflection(
        Type clrType,
        ValueComparer? comparer,
        ValueComparer? keyComparer,
        JsonValueReaderWriter? jsonValueReaderWriter)
    {
        var genericType = typeof(InMemoryTypeMapping<>).MakeGenericType(clrType);
        return comparer is null && keyComparer is null && jsonValueReaderWriter is null
            ? (InMemoryTypeMapping)genericType.GetAnyProperty(nameof(InMemoryTypeMapping<object>.Default))!.GetValue(null)!
            : (InMemoryTypeMapping)Activator.CreateInstance(genericType, comparer, keyComparer, jsonValueReaderWriter)!;
    }
}
