// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.Query.Relationships;

/// <summary>
/// Main entity type used as the root for most test queries.
/// References <see cref="RelatedType" />, which represents the main type/relationship to be tested.
/// </summary>
public class RootEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public required RelatedType RequiredRelated { get; set; }
    public RelatedType? OptionalRelated { get; set; }
    public List<RelatedType> RelatedCollection { get; set; } = null!;

    public RootReferencingEntity? RootReferencingEntity { get; set; }

    // Foreign keys and inverse navigations are unmapped by default, and are explicitly mapped via
    // the Fluent API for navigation tests only.
    [NotMapped] // Explicitly mapped via Fluent API for navigation tests only
    public int RequiredRelatedId { get; set; }
    [NotMapped] // Explicitly mapped via Fluent API for navigation tests only
    public int OptionalRelatedId { get; set; }
}

/// <summary>
/// The main type to be tested; mapped differently (entity type, complex type...) across
/// different test variations.
/// </summary>
public class RelatedType
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public int Int { get; set; }
    public string String { get; set; } = null!;

    public required NestedType RequiredNested { get; set; }
    public NestedType? OptionalNested { get; set; }
    public List<NestedType> NestedCollection { get; set; } = null!;

    // Foreign keys and inverse navigations are unmapped by default, and are explicitly mapped via
    // the Fluent API for navigation tests only.
    [NotMapped]
    public int RequiredNestedId { get; set; }
    [NotMapped]
    public int? OptionalNestedId { get; set; }

    [NotMapped]
    public RootEntity RequiredRelatedInverse { get; set; } = null!;
    [NotMapped]
    public RootEntity OptionalRelatedInverse { get; set; } = null!;
    [NotMapped]
    public RootEntity RelatedCollectionInverse { get; set; } = null!;
    [NotMapped]
    public int? CollectionRootId { get; set; }
}

/// <summary>
/// An additional nested type contained within <see cref="RelatedType" />, for tests which exercise
/// nested relationships.
/// </summary>
public class NestedType
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public int Int { get; set; }
    public string String { get; set; } = null!;

    // Foreign keys and inverse navigations are unmapped by default, and are explicitly mapped via
    // the Fluent API for navigation tests only.
    [NotMapped]
    public RelatedType RequiredNestedInverse { get; set; } = null!;
    [NotMapped]
    public RelatedType OptionalNestedInverse { get; set; } = null!;
    [NotMapped]
    public RelatedType NestedCollectionInverse { get; set; } = null!;
    [NotMapped]
    public int? CollectionRelatedId { get; set; }
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
