// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

public class RelationshipsLeaf
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public RelationshipsBranch OptionalReferenceInverseBranch { get; set; } = null!;

    public RelationshipsBranch RequiredReferenceInverseBranch { get; set; } = null!;

    public int? CollectionBranchId { get; set; }
    public RelationshipsBranch CollectionInverseBranch { get; set; } = null!;
}
