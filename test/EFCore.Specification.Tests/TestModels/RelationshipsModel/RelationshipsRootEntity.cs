// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

public class RelationshipsRoot
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? OptionalReferenceTrunkId { get; set; }
    public RelationshipsTrunk? OptionalReferenceTrunk { get; set; } = null!;

    public int RequiredReferenceTrunkId { get; set; }
    public RelationshipsTrunk RequiredReferenceTrunk { get; set; } = null!;
    public List<RelationshipsTrunk> CollectionTrunk { get; set; } = null!;
}
