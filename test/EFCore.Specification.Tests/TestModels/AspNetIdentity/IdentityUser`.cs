// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.AspNetIdentity;

#nullable disable

public class IdentityUser<TKey>
    where TKey : IEquatable<TKey>
{
    public IdentityUser()
    {
    }

    public IdentityUser(string userName)
        : this()
    {
        UserName = userName;
    }

    [PersonalData]
    public virtual TKey Id { get; set; }

    [ProtectedPersonalData]
    public virtual string UserName { get; set; }

    public virtual string NormalizedUserName { get; set; }

    [ProtectedPersonalData]
    public virtual string Email { get; set; }

    public virtual string NormalizedEmail { get; set; }

    [PersonalData]
    public virtual bool EmailConfirmed { get; set; }

    public virtual string PasswordHash { get; set; }

    public virtual string SecurityStamp { get; set; }

    public virtual string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

    [ProtectedPersonalData]
    public virtual string PhoneNumber { get; set; }

    [PersonalData]
    public virtual bool PhoneNumberConfirmed { get; set; }

    [PersonalData]
    public virtual bool TwoFactorEnabled { get; set; }

    public virtual DateTimeOffset? LockoutEnd { get; set; }

    public virtual bool LockoutEnabled { get; set; }

    public virtual int AccessFailedCount { get; set; }

    public override string ToString()
        => UserName;
}
