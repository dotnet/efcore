// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.AspNetIdentity;

#nullable disable

public class IdentityUserToken<TKey>
    where TKey : IEquatable<TKey>
{
    public virtual TKey UserId { get; set; }
    public virtual string LoginProvider { get; set; }
    public virtual string Name { get; set; }

    [ProtectedPersonalData]
    public virtual string Value { get; set; }
}
