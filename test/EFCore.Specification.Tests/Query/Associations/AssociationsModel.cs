// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.Query.Associations;

/// <summary>
///     Main entity type used as the root for most test queries.
///     References <see cref="AssociateType" />, which represents the main type/association to be tested.
/// </summary>
public class RootEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public required AssociateType RequiredAssociate { get; set; }
    public AssociateType? OptionalAssociate { get; set; }
    public List<AssociateType> AssociateCollection { get; set; } = null!;

    public RootReferencingEntity? RootReferencingEntity { get; set; }

    // Foreign keys and inverse navigations are unmapped by default, and are explicitly mapped via
    // the Fluent API for navigation tests only.
    [NotMapped] // Explicitly mapped via Fluent API for navigation tests only
    public int RequiredAssociateId { get; set; }

    [NotMapped] // Explicitly mapped via Fluent API for navigation tests only
    public int? OptionalAssociateId { get; set; }

    public override string ToString()
        => Name;
}

/// <summary>
///     The main type to be tested; mapped differently (entity type, complex type...) across
///     different test variations.
/// </summary>
public class AssociateType : IEquatable<AssociateType>
{
    public required int Id { get; set; }
    public required string Name { get; set; }

    public required int Int { get; set; }
    public required string String { get; set; }
    public required List<int> Ints { get; set; }

    public int Unmapped => Int + 1;

    public required NestedAssociateType RequiredNestedAssociate { get; set; }
    public NestedAssociateType? OptionalNestedAssociate { get; set; }
    public List<NestedAssociateType> NestedCollection { get; set; } = null!;

    // Foreign keys and inverse navigations are unmapped by default, and are explicitly mapped via
    // the Fluent API for navigation tests only.
    [NotMapped]
    public int RequiredNestedAssociateId { get; set; }

    [NotMapped]
    public int? OptionalNestedAssociateId { get; set; }

    [NotMapped]
    public RootEntity RequiredAssociateInverse { get; set; } = null!;

    [NotMapped]
    public RootEntity OptionalAssociateInverse { get; set; } = null!;

    [NotMapped]
    public RootEntity AssociateCollectionInverse { get; set; } = null!;

    [NotMapped]
    public int? CollectionRootId { get; set; }

    public bool Equals(AssociateType? other)
        => other is not null
           && Id == other.Id
           && Name == other.Name
           && Int == other.Int
           && String == other.String
           && (Ints is null ? other.Ints is null : other.Ints is not null && Ints.SequenceEqual(other.Ints))
           && RequiredNestedAssociate.Equals(other.RequiredNestedAssociate)
           && (OptionalNestedAssociate is null ? other.OptionalNestedAssociate is null : OptionalNestedAssociate.Equals(other.OptionalNestedAssociate))
           // NestedCollection is annotated non-nullable, but ComplexTableSplitting doesn't support collections so we check it
           && (NestedCollection is null ? other.NestedCollection is null : NestedCollection.SequenceEqual(other.NestedCollection));

    public AssociateType DeepClone()
        => new()
        {
            Id = Id,
            Name = Name,
            Int = Int,
            String = String,
            Ints = Ints is null ? null! : Ints.ToList(),
            RequiredNestedAssociate = RequiredNestedAssociate.DeepClone(),
            OptionalNestedAssociate = OptionalNestedAssociate?.DeepClone(),

            // NestedCollection is annotated non-nullable, but ComplexTableSplitting doesn't support collections so we null-bang it
            NestedCollection = NestedCollection is null ? null! : NestedCollection.Select(n => n.DeepClone()).ToList()
        };

    public override string ToString()
        => Name;
}

/// <summary>
///     An additional nested type contained within <see cref="AssociateType" />, for tests which exercise
///     nested associations.
/// </summary>
public class NestedAssociateType : IEquatable<NestedAssociateType>
{
    public required int Id { get; set; }
    public required string Name { get; set; }

    public required int Int { get; set; }
    public required string String { get; set; }

    public required List<int> Ints { get; set; }

    // Foreign keys and inverse navigations are unmapped by default, and are explicitly mapped via
    // the Fluent API for navigation tests only.
    [NotMapped]
    public AssociateType RequiredNestedAssociateInverse { get; set; } = null!;

    [NotMapped]
    public AssociateType OptionalNestedAssociateInverse { get; set; } = null!;

    [NotMapped]
    public AssociateType NestedCollectionInverse { get; set; } = null!;

    [NotMapped]
    public int? CollectionAssociateId { get; set; }

    public bool Equals(NestedAssociateType? other)
        => other is not null
            && Id == other.Id
            && Name == other.Name
            && Int == other.Int
            && String == other.String
            && (Ints is null ? other.Ints is null : other.Ints is not null && Ints.SequenceEqual(other.Ints));

    public NestedAssociateType DeepClone()
        => new()
        {
            Id = Id,
            Name = Name,
            Int = Int,
            String = String,
            Ints = Ints?.ToList()!
        };

    public override string ToString()
        => Name;
}

/// <summary>
///     Regular entity type referencing RootEntity, which is also a regular entity type.
///     Used to test e.g. projecting complex types on via optional (null) navigations.
/// </summary>
public class RootReferencingEntity
{
    public int Id { get; set; }

    public RootEntity? Root { get; set; } = null!;
}
