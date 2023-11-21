// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestLogger<TDefinitions> : TestLogger
    where TDefinitions : LoggingDefinitions, new()
{
    public TestLogger()
        : base(new TDefinitions())
    {
    }

    public TestLogger(LoggingDefinitions definitions)
        : base(definitions)
    {
    }
}
