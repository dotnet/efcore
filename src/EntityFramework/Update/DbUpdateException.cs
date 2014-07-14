// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Update
{
    public class DbUpdateException : InvalidOperationException
    {
        private readonly IReadOnlyList<StateEntry> _stateEntries;

        public DbUpdateException()
        {
            _stateEntries = new List<StateEntry>();
        }

        public DbUpdateException([NotNull] string message)
            : base(message)
        {
            Check.NotEmpty(message, "message");

            _stateEntries = new List<StateEntry>();
        }

        public DbUpdateException([NotNull] string message, [CanBeNull] Exception innerException)
            : base(message, innerException)
        {
            Check.NotEmpty(message, "message");

            _stateEntries = new List<StateEntry>();
        }

        public DbUpdateException([NotNull] string message, [NotNull] IReadOnlyList<StateEntry> stateEntries)
            : base(message)
        {
            Check.NotEmpty(message, "message");
            Check.NotEmpty(stateEntries, "stateEntries");

            _stateEntries = stateEntries;
        }

        public DbUpdateException([NotNull] string message, [CanBeNull] Exception innerException, [NotNull] IReadOnlyList<StateEntry> stateEntries)
            : base(message, innerException)
        {
            Check.NotEmpty(message, "message");
            Check.NotEmpty(stateEntries, "stateEntries");

            _stateEntries = stateEntries;
        }

        public virtual IReadOnlyList<StateEntry> StateEntries
        {
            get { return _stateEntries; }
        }
    }
}
