// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestModels.AspNetIdentity
{
    public class IdentityDbContext<TUser> : IdentityDbContext<TUser, IdentityRole, string>
        where TUser : IdentityUser
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
