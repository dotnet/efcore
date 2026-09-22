// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.EntitySplitting;

#nullable disable

public class EntityOne
{
    public int Id { get; set; }
    public string StringValue1 { get; set; }
    public string StringValue2 { get; set; }
    public string StringValue3 { get; set; }
    public string StringValue4 { get; set; }
    public int IntValue1 { get; set; }
    public int IntValue2 { get; set; }
    public int IntValue3 { get; set; }
    public int IntValue4 { get; set; }
    public List<EntityTwo> EntityTwos { get; set; } = [];
    public EntityThree EntityThree { get; set; }

    [NotMapped]
    public OwnedReference OwnedReference { get; set; }

    [NotMapped]
    public List<OwnedCollection> OwnedCollection { get; set; } = [];
}

public class EntityTwo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public EntityOne EntityOne { get; set; }
}

public class EntityThree
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<EntityOne> EntityOnes { get; set; } = [];
}

public class OwnedReference
{
    public int Id { get; set; }
    public string OwnedStringValue1 { get; set; }
    public string OwnedStringValue2 { get; set; }
    public string OwnedStringValue3 { get; set; }
    public string OwnedStringValue4 { get; set; }
    public int OwnedIntValue1 { get; set; }
    public int OwnedIntValue2 { get; set; }
    public int OwnedIntValue3 { get; set; }
    public int OwnedIntValue4 { get; set; }

    [NotMapped]
    public OwnedNestedReference OwnedNestedReference { get; set; }
}

public class OwnedCollection
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    public string OwnedStringValue1 { get; set; }
    public string OwnedStringValue2 { get; set; }
    public string OwnedStringValue3 { get; set; }
    public string OwnedStringValue4 { get; set; }
    public int OwnedIntValue1 { get; set; }
    public int OwnedIntValue2 { get; set; }
    public int OwnedIntValue3 { get; set; }
    public int OwnedIntValue4 { get; set; }
}

public class OwnedNestedReference
{
    public int Id { get; set; }
    public string OwnedNestedStringValue1 { get; set; }
    public string OwnedNestedStringValue2 { get; set; }
    public string OwnedNestedStringValue3 { get; set; }
    public string OwnedNestedStringValue4 { get; set; }
    public int OwnedNestedIntValue1 { get; set; }
    public int OwnedNestedIntValue2 { get; set; }
    public int OwnedNestedIntValue3 { get; set; }
    public int OwnedNestedIntValue4 { get; set; }
}

public class BaseEntity
{
    public int Id { get; set; }
    public int BaseValue { get; set; }

    [NotMapped]
    public OwnedReference OwnedReference { get; set; }

    [NotMapped]
    public List<OwnedCollection> OwnedCollection { get; set; } = [];
}

public class MiddleEntity : BaseEntity
{
    public int MiddleValue { get; set; }
}

public class SiblingEntity : BaseEntity
{
    public int SiblingValue { get; set; }
}

public class LeafEntity : MiddleEntity
{
    public int LeafValue { get; set; }
}
