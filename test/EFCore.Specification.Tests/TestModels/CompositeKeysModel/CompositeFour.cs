// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.CompositeKeysModel;

#nullable disable

public class CompositeFour
{
    public string Id1 { get; set; }
    public int Id2 { get; set; }

    public string Name { get; set; }

    public string Level3_Required_Id1 { get; set; }
    public int Level3_Required_Id2 { get; set; }

    public string Level3_Optional_Id1 { get; set; }
    public int? Level3_Optional_Id2 { get; set; }

    public CompositeThree OneToOne_Required_PK_Inverse4 { get; set; }
    public CompositeThree OneToOne_Optional_PK_Inverse4 { get; set; }
    public CompositeThree OneToOne_Required_FK_Inverse4 { get; set; }
    public CompositeThree OneToOne_Optional_FK_Inverse4 { get; set; }

    public CompositeThree OneToMany_Required_Inverse4 { get; set; }
    public CompositeThree OneToMany_Optional_Inverse4 { get; set; }

    public CompositeFour OneToOne_Optional_Self4 { get; set; }

    public ICollection<CompositeFour> OneToMany_Required_Self4 { get; set; }
    public ICollection<CompositeFour> OneToMany_Optional_Self4 { get; set; }
    public CompositeFour OneToMany_Required_Self_Inverse4 { get; set; }
    public CompositeFour OneToMany_Optional_Self_Inverse4 { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        return ReferenceEquals(this, obj) ? true : obj.GetType() == GetType() && Equals((CompositeFour)obj);
    }

    protected bool Equals(CompositeFour other)
        => Id1 == other.Id1
            && Id2 == other.Id2
            && string.Equals(Name, other.Name)
            && Level3_Required_Id1 == other.Level3_Required_Id1
            && Level3_Required_Id2 == other.Level3_Required_Id2
            && Level3_Optional_Id1 == other.Level3_Optional_Id1
            && Level3_Optional_Id2 == other.Level3_Optional_Id2;

    public override int GetHashCode()
        => HashCode.Combine(Id1, Id2, Name, Level3_Required_Id1, Level3_Required_Id2, Level3_Optional_Id1, Level3_Optional_Id2);
}
