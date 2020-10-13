// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

#if NET5_0
using System.IO;
#endif

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
    public class CosmosDbConfiguredConditionAttribute : Attribute, ITestCondition
    {
        public string SkipReason
            => "Unable to connect to Cosmos DB. Please install/start the emulator service or configure a valid endpoint.";

        private static bool? _connectionAvailable;

        public async ValueTask<bool> IsMetAsync()
        {
            if (_connectionAvailable == null)
            {
                _connectionAvailable = await TryConnectAsync();
            }

            return _connectionAvailable.Value;
        }

        private static async Task<bool> TryConnectAsync()
        {
            CosmosTestStore testStore = null;
            try
            {
                testStore = CosmosTestStore.CreateInitialized("NonExistent");

                return true;
            }
            catch (AggregateException aggregate)
            {
                if (aggregate.Flatten().InnerExceptions.Any(IsNotConfigured))
                {
                    return false;
                }

                throw;
            }
            catch (Exception e)
            {
                if (IsNotConfigured(e))
                {
                    return false;
                }

                throw;
            }
            finally
            {
                if (testStore != null)
                {
                    await testStore.DisposeAsync();
                }
            }
        }

        private static bool IsNotConfigured(Exception exception)
            => exception switch
            {
                HttpRequestException re => re.InnerException is SocketException
#if NET5_0
                    || (re.InnerException is IOException networkException
                        && networkException.InnerException is SocketException)
#endif
                    ,
                _ => exception.Message.Contains(
                    "The input authorization token can't serve the request. Please check that the expected payload is built as per the protocol, and check the key being used.",
                    StringComparison.Ordinal),
            };
    }
}
