// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

public class RelationshipsLeafEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public RelationshipsBranchEntity OptionalReferenceInverseBranch { get; set; } = null!;

    public RelationshipsBranchEntity RequiredReferenceInverseBranch { get; set; } = null!;

    public int? CollectionBranchId { get; set; }
    public RelationshipsBranchEntity CollectionInverseBranch { get; set; } = null!;
}
