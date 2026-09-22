// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;

#nullable disable

public class InheritanceDerived1 : InheritanceBase1
{
    public InheritanceLeaf1 ReferenceSameType { get; set; }
    public InheritanceLeaf1 ReferenceDifferentType { get; set; }
    public List<InheritanceLeaf1> CollectionSameType { get; set; }
    public List<InheritanceLeaf1> CollectionDifferentType { get; set; }
}
