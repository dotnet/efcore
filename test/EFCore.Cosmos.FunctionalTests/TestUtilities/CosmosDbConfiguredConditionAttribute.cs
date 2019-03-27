// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.Cosmos.TestUtilities
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
    public class CosmosDbConfiguredConditionAttribute : Attribute, ITestCondition
    {
        private static bool? _connectionAvailable;

        public string SkipReason => "Unable to connect to CosmosDb Emulator. Please install/start emulator service.";

        public bool IsMet
        {
            get
            {
                if (_connectionAvailable == null)
                {
                    _connectionAvailable = TryConnect();
                }
                return _connectionAvailable.Value;
            }
        }

        private static bool TryConnect()
        {
            try
            {
                using (CosmosTestStore.CreateInitialized("NonExistent"))
                {
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
