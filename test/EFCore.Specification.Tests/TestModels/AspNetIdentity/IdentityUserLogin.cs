// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.AspNetIdentity;

#nullable disable

public class IdentityUserLogin<TKey>
    where TKey : IEquatable<TKey>
{
    public virtual string LoginProvider { get; set; }
    public virtual string ProviderKey { get; set; }
    public virtual string ProviderDisplayName { get; set; }
    public virtual TKey UserId { get; set; }
}
