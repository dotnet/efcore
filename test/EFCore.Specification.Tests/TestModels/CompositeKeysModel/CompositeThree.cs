// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.CompositeKeysModel;

#nullable disable

public class CompositeThree
{
    public string Id1 { get; set; }
    public int Id2 { get; set; }

    public string Name { get; set; }

    public string Level2_Required_Id1 { get; set; }
    public int Level2_Required_Id2 { get; set; }
    public string Level2_Optional_Id1 { get; set; }
    public int? Level2_Optional_Id2 { get; set; }

    public CompositeFour OneToOne_Required_PK3 { get; set; }
    public CompositeFour OneToOne_Optional_PK3 { get; set; }

    public CompositeFour OneToOne_Required_FK3 { get; set; }
    public CompositeFour OneToOne_Optional_FK3 { get; set; }

    public ICollection<CompositeFour> OneToMany_Required3 { get; set; }
    public ICollection<CompositeFour> OneToMany_Optional3 { get; set; }

    public CompositeTwo OneToOne_Required_PK_Inverse3 { get; set; }
    public CompositeTwo OneToOne_Optional_PK_Inverse3 { get; set; }
    public CompositeTwo OneToOne_Required_FK_Inverse3 { get; set; }
    public CompositeTwo OneToOne_Optional_FK_Inverse3 { get; set; }

    public CompositeTwo OneToMany_Required_Inverse3 { get; set; }
    public CompositeTwo OneToMany_Optional_Inverse3 { get; set; }

    public CompositeThree OneToOne_Optional_Self3 { get; set; }

    public ICollection<CompositeThree> OneToMany_Required_Self3 { get; set; }
    public ICollection<CompositeThree> OneToMany_Optional_Self3 { get; set; }
    public CompositeThree OneToMany_Required_Self_Inverse3 { get; set; }
    public CompositeThree OneToMany_Optional_Self_Inverse3 { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        return ReferenceEquals(this, obj) ? true : obj.GetType() == GetType() && Equals((CompositeThree)obj);
    }

    protected bool Equals(CompositeThree other)
        => Id1 == other.Id1
            && Id2 == other.Id2
            && string.Equals(Name, other.Name)
            && Level2_Required_Id1 == other.Level2_Required_Id1
            && Level2_Required_Id2 == other.Level2_Required_Id2
            && Level2_Optional_Id1 == other.Level2_Optional_Id1
            && Level2_Optional_Id2 == other.Level2_Optional_Id2;

    public override int GetHashCode()
        => HashCode.Combine(Id1, Id2, Name, Level2_Required_Id1, Level2_Required_Id2, Level2_Optional_Id1, Level2_Optional_Id2);
}
