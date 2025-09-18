// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Query.Associations;

/// <summary>
/// Variant of <see cref="RootEntity" /> containing value type complex types.
/// </summary>
public class ValueRootEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public required ValueAssociateType RequiredAssociate { get; set; }
    public ValueAssociateType? OptionalAssociate { get; set; }

    // TODO: We don't yet support complex collections of value types, #31411;
    // For now map a reference type collection instead.
    public List<AssociateType> AssociateCollection { get; set; } = null!;

    public override string ToString() => Name;

    public static ValueRootEntity FromRootEntity(RootEntity rootEntity)
        => new()
        {
            Id = rootEntity.Id,
            Name = rootEntity.Name,

            RequiredAssociate = ValueAssociateType.FromAssociateType(rootEntity.RequiredAssociate).Value,
            OptionalAssociate = ValueAssociateType.FromAssociateType(rootEntity.OptionalAssociate),

            // TODO: Collection of nullable associate types
            AssociateCollection = rootEntity.AssociateCollection.Select(r => r.DeepClone()).ToList()
        };
}

/// <summary>
/// Variant of <see cref="AssociateType" /> as a value type.
/// </summary>
public struct ValueAssociateType : IEquatable<ValueAssociateType>
{
    public ValueAssociateType()
    {
    }

    public int Id { get; set; }
    public required string Name { get; set; }

    public int Int { get; set; }
    public string String { get; set; } = null!;

    public required ValueNestedType RequiredNested { get; set; }
    public ValueNestedType? OptionalNested { get; set; }

    // TODO: Collection of nullable nested types
    // TODO: We don't yet support complex collections of value types, #31411;
    // For now map a reference type collection instead.
    public List<NestedAssociateType> NestedCollection { get; set; } = null!;

    public readonly bool Equals(ValueAssociateType other)
        => Id == other.Id
           && Name == other.Name
           && Int == other.Int
           && String == other.String
           && RequiredNested.Equals(other.RequiredNested)
           && (OptionalNested is null && other.OptionalNested is null || OptionalNested?.Equals(other.RequiredNested) == true)
           && NestedCollection.SequenceEqual(other.NestedCollection);

    [return: NotNullIfNotNull(nameof(associate))]
    public static ValueAssociateType? FromAssociateType(AssociateType? associate)
        => associate is null ? null : new()
        {
            Id = associate.Id,
            Name = associate.Name,
            Int = associate.Int,
            String = associate.String,

            RequiredNested = ValueNestedType.FromNestedType(associate.RequiredNestedAssociate).Value,
            OptionalNested = ValueNestedType.FromNestedType(associate.OptionalNestedAssociate),
            NestedCollection = associate.NestedCollection.Select((n) => n.DeepClone()).ToList()
        };

    public override string ToString() => Name;
}

/// <summary>
/// Variant of <see cref="ValueNestedType" /> as a value type.
/// </summary>
public struct ValueNestedType : IEquatable<ValueNestedType>
{
    public ValueNestedType()
    {
    }

    public int Id { get; set; }
    public required string Name { get; set; }

    public int Int { get; set; }
    public string String { get; set; } = null!;

    public readonly bool Equals(ValueNestedType other)
        => Id == other.Id
           && Name == other.Name
           && Int == other.Int
           && String == other.String;

    [return: NotNullIfNotNull(nameof(nestedType))]
    public static ValueNestedType? FromNestedType(NestedAssociateType? nestedType)
        => nestedType is null ? null : new()
        {
            Id = nestedType.Id,
            Name = nestedType.Name,
            Int = nestedType.Int,
            String = nestedType.String
        };

    public override string ToString() => Name;
}
