// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Tests.TestUtilities
{
    public class ListLogger<T> : ListLogger, ILogger<T>
    {
        public ListLogger(List<Tuple<LogLevel, string>> logMessages)
            : base(logMessages)
        {
        }
    }
}
