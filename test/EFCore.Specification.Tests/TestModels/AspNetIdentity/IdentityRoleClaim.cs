// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.AspNetIdentity;

#nullable disable

public class IdentityRoleClaim<TKey>
    where TKey : IEquatable<TKey>
{
    public virtual int Id { get; set; }
    public virtual TKey RoleId { get; set; }
    public virtual string ClaimType { get; set; }
    public virtual string ClaimValue { get; set; }
}
