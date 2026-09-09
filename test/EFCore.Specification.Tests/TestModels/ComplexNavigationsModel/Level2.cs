// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;

#nullable disable

public class Level2
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Date { get; set; }

    public int Level1_Required_Id { get; set; }
    public int? Level1_Optional_Id { get; set; }

    public Level3 OneToOne_Required_PK2 { get; set; }
    public Level3 OneToOne_Optional_PK2 { get; set; }

    public Level3 OneToOne_Required_FK2 { get; set; }
    public Level3 OneToOne_Optional_FK2 { get; set; }

    public ICollection<Level3> OneToMany_Required2 { get; set; }
    public ICollection<Level3> OneToMany_Optional2 { get; set; }

    public Level1 OneToOne_Required_PK_Inverse2 { get; set; }
    public Level1 OneToOne_Optional_PK_Inverse2 { get; set; }
    public Level1 OneToOne_Required_FK_Inverse2 { get; set; }
    public Level1 OneToOne_Optional_FK_Inverse2 { get; set; }

    public Level1 OneToMany_Required_Inverse2 { get; set; }
    public Level1 OneToMany_Optional_Inverse2 { get; set; }

    public Level2 OneToOne_Optional_Self2 { get; set; }

    public ICollection<Level2> OneToMany_Required_Self2 { get; set; }
    public ICollection<Level2> OneToMany_Optional_Self2 { get; set; }
    public Level2 OneToMany_Required_Self_Inverse2 { get; set; }
    public Level2 OneToMany_Optional_Self_Inverse2 { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        return ReferenceEquals(this, obj) ? true : obj.GetType() == GetType() && Equals((Level2)obj);
    }

    private bool Equals(Level2 other)
        => Id == other.Id
            && string.Equals(Name, other.Name)
            && Date.Equals(other.Date)
            && Level1_Required_Id == other.Level1_Required_Id
            && Level1_Optional_Id == other.Level1_Optional_Id;

    public override int GetHashCode()
        => HashCode.Combine(Id, Name, Date, Level1_Required_Id, Level1_Optional_Id);
}
