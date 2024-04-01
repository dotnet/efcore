// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.AspNetIdentity;

#nullable disable

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
        => Name;
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
