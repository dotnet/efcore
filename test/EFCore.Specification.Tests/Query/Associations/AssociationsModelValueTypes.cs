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

    public required ValueRelatedType RequiredRelated { get; set; }
    public ValueRelatedType? OptionalRelated { get; set; }

    // TODO: We don't yet support complex collections of value types, #31411;
    // For now map a reference type collection instead.
    public List<RelatedType> RelatedCollection { get; set; } = null!;

    public override string ToString() => Name;

    public static ValueRootEntity FromRootEntity(RootEntity rootEntity)
        => new()
        {
            Id = rootEntity.Id,
            Name = rootEntity.Name,

            RequiredRelated = ValueRelatedType.FromRelatedType(rootEntity.RequiredRelated).Value,
            OptionalRelated = ValueRelatedType.FromRelatedType(rootEntity.OptionalRelated),

            // TODO: Collection of nullable related types
            RelatedCollection = rootEntity.RelatedCollection.Select(r => r.DeepClone()).ToList()
        };
}

/// <summary>
/// Variant of <see cref="RelatedType" /> as a value type.
/// </summary>
public struct ValueRelatedType : IEquatable<ValueRelatedType>
{
    public ValueRelatedType()
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
    public List<NestedType> NestedCollection { get; set; } = null!;

    public readonly bool Equals(ValueRelatedType other)
        => Id == other.Id
           && Name == other.Name
           && Int == other.Int
           && String == other.String
           && RequiredNested.Equals(other.RequiredNested)
           && (OptionalNested is null && other.OptionalNested is null || OptionalNested?.Equals(other.RequiredNested) == true)
           && NestedCollection.SequenceEqual(other.NestedCollection);

    [return: NotNullIfNotNull(nameof(relatedType))]
    public static ValueRelatedType? FromRelatedType(RelatedType? relatedType)
        => relatedType is null ? null : new()
        {
            Id = relatedType.Id,
            Name = relatedType.Name,
            Int = relatedType.Int,
            String = relatedType.String,

            RequiredNested = ValueNestedType.FromNestedType(relatedType.RequiredNested).Value,
            OptionalNested = ValueNestedType.FromNestedType(relatedType.OptionalNested),
            NestedCollection = relatedType.NestedCollection.Select((n) => n.DeepClone()).ToList()
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
    public static ValueNestedType? FromNestedType(NestedType? nestedType)
        => nestedType is null ? null : new()
        {
            Id = nestedType.Id,
            Name = nestedType.Name,
            Int = nestedType.Int,
            String = nestedType.String
        };

    public override string ToString() => Name;
}
