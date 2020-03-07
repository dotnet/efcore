// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.AspNetIdentity
{
    public class IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        public IdentityRole()
        {
        }

        public IdentityRole(string roleName)
            : this()
        {
            Name = roleName;
        }

        public virtual TKey Id { get; set; }

        public virtual string Name { get; set; }

        public virtual string NormalizedName { get; set; }

        public virtual string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

        public override string ToString()
        {
            return Name;
        }
    }

    public class IdentityRole : IdentityRole<string>
    {
        public IdentityRole()
        {
            Id = Guid.NewGuid().ToString();
        }

        public IdentityRole(string roleName)
            : this()
        {
            Name = roleName;
        }
    }
}
