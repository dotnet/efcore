// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class DocumentDbTestStoreFactory : ITestStoreFactory
    {
        public static DocumentDbTestStoreFactory Instance { get; } = new DocumentDbTestStoreFactory();

        protected DocumentDbTestStoreFactory()
        {
        }

        public virtual TestStore Create(string storeName)
            => DocumentDbTestStore.Create(storeName);

        public virtual TestStore GetOrCreate(string storeName)
            => DocumentDbTestStore.GetOrCreate(storeName);

        public IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkDocumentDb()
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                .AddSingleton<TestStoreIndex>();
    }
}
