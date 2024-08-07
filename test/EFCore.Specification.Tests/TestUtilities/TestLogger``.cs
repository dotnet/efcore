// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestLogger<TCategory, TDefinitions>(LoggingDefinitions definitions)
    : TestLogger<TDefinitions>(definitions), IDiagnosticsLogger<TCategory>
    where TCategory : LoggerCategory<TCategory>, new()
    where TDefinitions : LoggingDefinitions, new()
{
    public TestLogger()
        : this(new TDefinitions())
    {
    }
}
