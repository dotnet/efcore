// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Azure.Documents.Client;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.TestUtilities
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
            var documentClient = new DocumentClient(
                    new Uri(TestEnvironment.DefaultConnection),
                    TestEnvironment.AuthToken);

            try
            {
                documentClient.OpenAsync().GetAwaiter().GetResult();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                documentClient.Dispose();
            }
        }
    }
}
