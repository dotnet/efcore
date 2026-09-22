// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationshipsModel;

#nullable disable

public class DerivedInheritanceRelationshipEntity : BaseInheritanceRelationshipEntity
{
    public int? BaseId { get; set; }

    public BaseReferenceOnDerived BaseReferenceOnDerived { get; set; }
    public DerivedReferenceOnDerived DerivedReferenceOnDerived { get; set; }
    public ReferenceOnDerived ReferenceOnDerived { get; set; }
    public BaseInheritanceRelationshipEntity BaseSelfReferenceOnDerived { get; set; }
    public OwnedEntity OwnedReferenceOnDerived { get; set; }

    public List<BaseCollectionOnDerived> BaseCollectionOnDerived { get; set; }
    public List<DerivedCollectionOnDerived> DerivedCollectionOnDerived { get; set; }
    public List<CollectionOnDerived> CollectionOnDerived { get; set; }
    public List<OwnedEntity> OwnedCollectionOnDerived { get; set; }
}
