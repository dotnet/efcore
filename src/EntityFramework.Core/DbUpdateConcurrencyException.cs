// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Update;

namespace Microsoft.Data.Entity
{
    public class DbUpdateConcurrencyException : DbUpdateException
    {
        public DbUpdateConcurrencyException(
            [NotNull] string message,
            [NotNull] IReadOnlyList<IUpdateEntry> entries)
            : base(message, entries)
        {
        }
    }
}
