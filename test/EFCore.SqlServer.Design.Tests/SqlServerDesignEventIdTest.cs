// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests;
using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.Design
{
    public class SqlServerDesignEventIdTest
    {
        [Fact]
        public void Every_eventId_has_a_logger_method_and_logs_when_level_enabled()
        {
            var fakeFactories = new Dictionary<Type, Func<object>>
            {
                { typeof(string), () => "Fake" }
            };

            SqlServerTestHelpers.Instance.TestEventLogging(
                typeof(SqlServerDesignEventId),
                typeof(SqlServerDesignLoggerExtensions),
                fakeFactories);
        }
    }
}
