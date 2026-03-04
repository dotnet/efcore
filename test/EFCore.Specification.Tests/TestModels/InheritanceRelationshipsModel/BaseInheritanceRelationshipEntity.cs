// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationshipsModel;

#nullable disable

public class BaseInheritanceRelationshipEntity
{
    public int Id { get; set; }

    public string Name { get; set; }

    public DerivedInheritanceRelationshipEntity DerivedSefReferenceOnBase { get; set; }
    public BaseReferenceOnBase BaseReferenceOnBase { get; set; }
    public ReferenceOnBase ReferenceOnBase { get; set; }
    public OwnedEntity OwnedReferenceOnBase { get; set; }

    public List<BaseCollectionOnBase> BaseCollectionOnBase { get; set; }
    public List<CollectionOnBase> CollectionOnBase { get; set; }
    public List<OwnedEntity> OwnedCollectionOnBase { get; set; }
}
