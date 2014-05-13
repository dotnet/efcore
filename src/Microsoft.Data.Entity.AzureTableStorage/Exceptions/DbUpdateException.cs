// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    public class DbUpdateException : Exception
    {
        public DbUpdateException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public DbUpdateException(string message)
            : base(message)
        {
        }
    }
}
