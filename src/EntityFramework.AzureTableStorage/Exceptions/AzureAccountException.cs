// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.Data.Entity.AzureTableStorage
{
    public class AzureAccountException : Exception
    {
        public AzureAccountException([NotNull] string message)
            : base(Check.NotNull(message, "message"))
        {
        }
    }
}
