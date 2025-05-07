// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;

#nullable disable

public class Level4
{
    public int Id { get; set; }
    public string Name { get; set; }

    public int Level3_Required_Id { get; set; }
    public int? Level3_Optional_Id { get; set; }

    public Level3 OneToOne_Required_PK_Inverse4 { get; set; }
    public Level3 OneToOne_Optional_PK_Inverse4 { get; set; }
    public Level3 OneToOne_Required_FK_Inverse4 { get; set; }
    public Level3 OneToOne_Optional_FK_Inverse4 { get; set; }

    public Level3 OneToMany_Required_Inverse4 { get; set; }
    public Level3 OneToMany_Optional_Inverse4 { get; set; }

    public Level4 OneToOne_Optional_Self4 { get; set; }

    public ICollection<Level4> OneToMany_Required_Self4 { get; set; }
    public ICollection<Level4> OneToMany_Optional_Self4 { get; set; }
    public Level4 OneToMany_Required_Self_Inverse4 { get; set; }
    public Level4 OneToMany_Optional_Self_Inverse4 { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        return ReferenceEquals(this, obj) ? true : obj.GetType() == GetType() && Equals((Level4)obj);
    }

    protected bool Equals(Level4 other)
        => Id == other.Id
            && string.Equals(Name, other.Name)
            && Level3_Required_Id == other.Level3_Required_Id
            && Level3_Optional_Id == other.Level3_Optional_Id;

    public override int GetHashCode()
        => HashCode.Combine(Id, Name, Level3_Required_Id, Level3_Optional_Id);
}
