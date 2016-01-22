// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;

namespace Microsoft.EntityFrameworkCore.Internal
{
    // TODO revert to anonymous types when https://github.com/dotnet/corefx/issues/4672 is fixed
    internal class RelationalDiagnosticSourceMessage
    {
        public DbCommand Command { get; set; }
        public string ExecuteMethod { get; set; }
        public bool IsAsync { get; set; }
        public Exception Exception { get; set; }
    }
}
