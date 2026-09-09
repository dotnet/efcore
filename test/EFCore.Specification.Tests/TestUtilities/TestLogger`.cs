// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestLogger<TDefinitions>(LoggingDefinitions definitions) : TestLogger(definitions)
    where TDefinitions : LoggingDefinitions, new()
{
    public TestLogger()
        : this(new TDefinitions())
    {
    }
}
