// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

public class RelationshipsBranchEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? OptionalReferenceLeafId { get; set; }
    public RelationshipsLeafEntity? OptionalReferenceLeaf { get; set; } = null!;

    public int RequiredReferenceLeafId { get; set; }
    public RelationshipsLeafEntity RequiredReferenceLeaf { get; set; } = null!;
    public List<RelationshipsLeafEntity> CollectionLeaf { get; set; } = null!;

    public RelationshipsTrunkEntity? OptionalReferenceInverseTrunk { get; set; } = null!;

    public RelationshipsTrunkEntity RequiredReferenceInverseTrunk { get; set; } = null!;

    public int? CollectionTrunkId { get; set; }
    public RelationshipsTrunkEntity CollectionInverseTrunk { get; set; } = null!;
}
