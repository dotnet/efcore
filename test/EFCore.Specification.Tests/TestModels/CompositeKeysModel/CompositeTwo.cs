// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.CompositeKeysModel;

#nullable disable

public class CompositeTwo
{
    public string Id1 { get; set; }
    public int Id2 { get; set; }

    public string Name { get; set; }
    public DateTime Date { get; set; }

    public string Level1_Required_Id1 { get; set; }
    public int Level1_Required_Id2 { get; set; }

    public string Level1_Optional_Id1 { get; set; }
    public int? Level1_Optional_Id2 { get; set; }

    public CompositeThree OneToOne_Required_PK2 { get; set; }
    public CompositeThree OneToOne_Optional_PK2 { get; set; }

    public CompositeThree OneToOne_Required_FK2 { get; set; }
    public CompositeThree OneToOne_Optional_FK2 { get; set; }

    public ICollection<CompositeThree> OneToMany_Required2 { get; set; }
    public ICollection<CompositeThree> OneToMany_Optional2 { get; set; }

    public CompositeOne OneToOne_Required_PK_Inverse2 { get; set; }
    public CompositeOne OneToOne_Optional_PK_Inverse2 { get; set; }
    public CompositeOne OneToOne_Required_FK_Inverse2 { get; set; }
    public CompositeOne OneToOne_Optional_FK_Inverse2 { get; set; }

    public CompositeOne OneToMany_Required_Inverse2 { get; set; }
    public CompositeOne OneToMany_Optional_Inverse2 { get; set; }

    public CompositeTwo OneToOne_Optional_Self2 { get; set; }

    public ICollection<CompositeTwo> OneToMany_Required_Self2 { get; set; }
    public ICollection<CompositeTwo> OneToMany_Optional_Self2 { get; set; }
    public CompositeTwo OneToMany_Required_Self_Inverse2 { get; set; }
    public CompositeTwo OneToMany_Optional_Self_Inverse2 { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        return ReferenceEquals(this, obj) ? true : obj.GetType() == GetType() && Equals((CompositeTwo)obj);
    }

    private bool Equals(CompositeTwo other)
        => Id1 == other.Id1
            && Id2 == other.Id2
            && string.Equals(Name, other.Name)
            && Date.Equals(other.Date)
            && Level1_Required_Id1 == other.Level1_Required_Id1
            && Level1_Required_Id2 == other.Level1_Required_Id2
            && Level1_Optional_Id1 == other.Level1_Optional_Id1
            && Level1_Optional_Id2 == other.Level1_Optional_Id2;

    public override int GetHashCode()
        => HashCode.Combine(Id1, Id2, Name, Date, Level1_Required_Id1, Level1_Required_Id2, Level1_Optional_Id1, Level1_Optional_Id2);
}
