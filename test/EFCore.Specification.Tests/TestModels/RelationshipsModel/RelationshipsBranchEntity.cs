// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

public class RelationshipsBranch
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? OptionalReferenceLeafId { get; set; }
    public RelationshipsLeaf? OptionalReferenceLeaf { get; set; } = null!;

    public int RequiredReferenceLeafId { get; set; }
    public RelationshipsLeaf RequiredReferenceLeaf { get; set; } = null!;
    public List<RelationshipsLeaf> CollectionLeaf { get; set; } = null!;

    public RelationshipsTrunk? OptionalReferenceInverseTrunk { get; set; } = null!;

    public RelationshipsTrunk RequiredReferenceInverseTrunk { get; set; } = null!;

    public int? CollectionTrunkId { get; set; }
    public RelationshipsTrunk CollectionInverseTrunk { get; set; } = null!;
}
