// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;

#nullable disable

public class Level1
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Date { get; set; }

    public Level2 OneToOne_Required_PK1 { get; set; }
    public Level2 OneToOne_Optional_PK1 { get; set; }

    public Level2 OneToOne_Required_FK1 { get; set; }
    public Level2 OneToOne_Optional_FK1 { get; set; }

    public ICollection<Level2> OneToMany_Required1 { get; set; }
    public ICollection<Level2> OneToMany_Optional1 { get; set; }

    public Level1 OneToOne_Optional_Self1 { get; set; }

    public ICollection<Level1> OneToMany_Required_Self1 { get; set; }
    public ICollection<Level1> OneToMany_Optional_Self1 { get; set; }
    public Level1 OneToMany_Required_Self_Inverse1 { get; set; }
    public Level1 OneToMany_Optional_Self_Inverse1 { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        return ReferenceEquals(this, obj) ? true : obj.GetType() == GetType() && Equals((Level1)obj);
    }

    private bool Equals(Level1 other)
        => Id == other.Id && string.Equals(Name, other.Name) && Date.Equals(other.Date);

    public override int GetHashCode()
        => HashCode.Combine(Id, Name, Date);
}
