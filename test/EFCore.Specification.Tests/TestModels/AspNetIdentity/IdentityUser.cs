// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.AspNetIdentity;

public class IdentityUser : IdentityUser<string>
{
    public IdentityUser()
    {
        Id = Guid.NewGuid().ToString();
        SecurityStamp = Guid.NewGuid().ToString();
    }

    public IdentityUser(string userName)
        : this()
    {
        UserName = userName;
    }
}
