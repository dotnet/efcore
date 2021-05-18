// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
    public class CosmosDbConfiguredConditionAttribute : Attribute, ITestCondition
    {
        public string SkipReason
            => "Unable to connect to Cosmos DB. Please install/start the emulator service or configure a valid endpoint.";

        public ValueTask<bool> IsMetAsync() => CosmosTestStore.IsConnectionAvailableAsync();
    }
}
