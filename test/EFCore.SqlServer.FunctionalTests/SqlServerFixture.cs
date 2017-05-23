// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerFixture
    {
        public readonly IServiceProvider ServiceProvider;

        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public SqlServerFixture()
        {
            ServiceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                .BuildServiceProvider();
        }
    }
}
