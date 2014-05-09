// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Update
{
    public class DbUpdateException : InvalidOperationException
    {
        public DbUpdateException()
        {
        }

        public DbUpdateException([NotNull] string message)
            : base(message)
        {
            Check.NotEmpty(message, "message");
        }

        public DbUpdateException([NotNull] string message, [CanBeNull] Exception innerException)
            : base(message, innerException)
        {
            Check.NotEmpty(message, "message");
        }
    }
}
