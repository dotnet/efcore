// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    public class DbUpdateException : Exception
    {
        public DbUpdateException([NotNull] string message, [NotNull] Exception innerException)
            : base(Check.NotNull(message, "message"), Check.NotNull(innerException, "innerException"))
        {
        }

        public DbUpdateException([NotNull] string message)
            : base(Check.NotNull(message, "message"))
        {
        }
    }
}
