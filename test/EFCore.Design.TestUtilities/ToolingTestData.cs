// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public static class ToolingTestData
{
    public static IEnumerable<TheoryDataRow<ToolingProjectLayout>> Layouts
        => Enum.GetValues<ToolingProjectLayout>()
            .Select(layout => new TheoryDataRow<ToolingProjectLayout>(layout) { Label = layout.ToString(), Timeout = 600_000 }
                .WithTrait("Layout", layout.ToString()));
}
