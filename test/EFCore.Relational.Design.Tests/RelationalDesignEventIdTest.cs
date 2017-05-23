// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class RelationalDesignEventIdTest
    {
        [Fact]
        public void Every_eventId_has_a_logger_method_and_logs_when_level_enabled()
        {
            var fakeFactories = new Dictionary<Type, Func<object>>
            {
                { typeof(IList<string>), () => new List<string> { "A", "B" } },
                { typeof(string), () => "Fake" }
            };

            RelationalTestHelpers.Instance.TestEventLogging(
                typeof(RelationalDesignEventId),
                typeof(RelationalDesignLoggerExtensions),
                fakeFactories);
        }
    }
}
