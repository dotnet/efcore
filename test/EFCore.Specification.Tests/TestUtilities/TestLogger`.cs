// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestLogger<TCategory, TDefinitions> : TestLogger<TDefinitions>, IDiagnosticsLogger<TCategory>
        where TCategory : LoggerCategory<TCategory>, new()
        where TDefinitions : LoggingDefinitions, new()
    {
        public IInterceptors Interceptors { get; }
    }
}
