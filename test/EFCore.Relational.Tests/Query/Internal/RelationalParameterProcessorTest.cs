// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

public class RelationalParameterProcessorTest
{
    [Theory,
     InlineData("SELECT {0}", 0, true),
     InlineData("SELECT {0} + {1}", 0, true),
     InlineData("SELECT {0} + {1}", 1, true),
     InlineData("SELECT {0,5}", 0, true),
     InlineData("SELECT {0 ,5}", 0, true),
     InlineData("SELECT {0:x}", 0, true),
     InlineData("SELECT {0 }", 0, true),
     InlineData("SELECT {00}", 0, true),
     InlineData("SELECT {01}", 1, true),
     InlineData("SELECT {10}", 10, true),
     InlineData("SELECT {{{0}}}", 0, true),
     InlineData("SELECT {{ {0}", 0, true),
     InlineData("{0}", 0, true),
     InlineData("SELECT {0}", 1, false),
     InlineData("SELECT {1}", 0, false),
     InlineData("SELECT {10}", 1, false),
     InlineData("SELECT {1}", 10, false),
     InlineData("SELECT {0", 0, false),
     InlineData("SELECT {0x}", 0, false),
     InlineData("SELECT {a}", 0, false),
     InlineData("SELECT {}", 0, false),
     InlineData("SELECT {", 0, false),
     InlineData("SELECT {{0}}", 0, false),
     InlineData("SELECT {{0,5}}", 0, false),
     InlineData("SELECT @city", 0, false),
     InlineData("SELECT {99999999999}", 0, false),
     InlineData("", 0, false)]
    public void ReferencesArgumentByPosition(string sql, int index, bool expected)
        => Assert.Equal(expected, RelationalParameterProcessor.ReferencesArgumentByPosition(sql, index));
}
