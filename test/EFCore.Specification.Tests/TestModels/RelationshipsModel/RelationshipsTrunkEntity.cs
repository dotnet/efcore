// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

public class RelationshipsTrunk
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? OptionalReferenceBranchId { get; set; }
    public RelationshipsBranch? OptionalReferenceBranch { get; set; } = null!;

    public int RequiredReferenceBranchId { get; set; }
    public RelationshipsBranch RequiredReferenceBranch { get; set; } = null!;

    public List<RelationshipsBranch> CollectionBranch { get; set; } = null!;

    public RelationshipsRoot? OptionalReferenceInverseRoot { get; set; } = null!;

    public RelationshipsRoot RequiredReferenceInverseRoot { get; set; } = null!;

    public int? CollectionRootId { get; set; }
    public RelationshipsRoot CollectionInverseRoot { get; set; } = null!;
}
