// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

#nullable disable

public class Weapon
{
    // auto generated key (sequence) TODO: make nullable when issue #478 is fixed
    public int Id { get; set; }

    public string Name { get; set; }
    public AmmunitionType? AmmunitionType { get; set; }
    public bool IsAutomatic { get; set; }

    // 1 - 1 self reference
    public int? SynergyWithId { get; set; }

    public virtual Weapon SynergyWith { get; set; }

    public string OwnerFullName { get; set; }
    public Gear Owner { get; set; }
}
