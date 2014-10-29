// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    public class AtsTestStore : TestStore
    {
        private readonly string _tableSuffix;

        public AtsTestStore(string tableSuffix)
        {
            _tableSuffix = tableSuffix;
        }

        public static Task<AtsTestStore> GetOrCreateSharedAsync(string name, Func<Task> initializeDatabase)
        {
            return new AtsTestStore(name).CreateSharedAsync(initializeDatabase);
        }

        private async Task<AtsTestStore> CreateSharedAsync(Func<Task> initializeDatabase)
        {
            await CreateSharedAsync(typeof(AtsTestStore).Name + _tableSuffix, initializeDatabase);

            return this;
        }

        public string ConnectionString
        {
            get { return TestConfig.Instance.ConnectionString; }
        }

        public Action CleanupAction { get; set; }

        public string TableSuffix
        {
            get { return _tableSuffix; }
        }

        public override void Dispose()
        {
            if (CleanupAction != null)
            {
                try
                {
                    CleanupAction();
                    CleanupAction = null;
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
