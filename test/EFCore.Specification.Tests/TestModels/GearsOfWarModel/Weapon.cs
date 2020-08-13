// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel
{
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
}
