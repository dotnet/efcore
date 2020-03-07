// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.AspNetIdentity
{
    public class IdentityUserToken<TKey>
        where TKey : IEquatable<TKey>
    {
        public virtual TKey UserId { get; set; }
        public virtual string LoginProvider { get; set; }
        public virtual string Name { get; set; }

        [ProtectedPersonalData]
        public virtual string Value { get; set; }
    }
}
