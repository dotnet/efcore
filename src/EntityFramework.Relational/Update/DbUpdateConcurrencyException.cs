// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Update
{
    public class DbUpdateConcurrencyException : DbUpdateException
    {
        public DbUpdateConcurrencyException()
        {
        }

        public DbUpdateConcurrencyException([NotNull] string message)
            : base(message)
        {
            Check.NotEmpty(message, "message");
        }

        public DbUpdateConcurrencyException([NotNull] string message, [CanBeNull] Exception innerException)
            : base(message, innerException)
        {
            Check.NotEmpty(message, "message");
        }

        public DbUpdateConcurrencyException([NotNull] string message, [NotNull] IReadOnlyList<StateEntry> stateEntries)
            : base(message, stateEntries)
        {
            Check.NotEmpty(message, "message");
        }

        public DbUpdateConcurrencyException([NotNull] string message, [CanBeNull] Exception innerException, [NotNull] IReadOnlyList<StateEntry> stateEntries)
            : base(message, innerException, stateEntries)
        {
            Check.NotEmpty(message, "message");
        }
    }
}
