// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.AspNetIdentity
{
    public class IdentityDbContext<TUser, TRole, TKey> : IdentityDbContext<TUser, TRole, TKey, IdentityUserClaim<TKey>,
        IdentityUserRole<TKey>, IdentityUserLogin<TKey>, IdentityRoleClaim<TKey>, IdentityUserToken<TKey>>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        public IdentityDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected IdentityDbContext()
        {
        }
    }
}
