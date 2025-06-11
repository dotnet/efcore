// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

public class RelationshipsTrunkEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? OptionalReferenceBranchId { get; set; }
    public RelationshipsBranchEntity? OptionalReferenceBranch { get; set; } = null!;

    public int RequiredReferenceBranchId { get; set; }
    public RelationshipsBranchEntity RequiredReferenceBranch { get; set; } = null!;

    public List<RelationshipsBranchEntity> CollectionBranch { get; set; } = null!;

    public RelationshipsRootEntity? OptionalReferenceInverseRoot { get; set; } = null!;

    public RelationshipsRootEntity RequiredReferenceInverseRoot { get; set; } = null!;

    public int? CollectionRootId { get; set; }
    public RelationshipsRootEntity CollectionInverseRoot { get; set; } = null!;
}
