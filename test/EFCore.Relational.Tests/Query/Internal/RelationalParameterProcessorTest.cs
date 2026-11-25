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
     InlineData("SELECT { 0}", 0, false), // string.Format doesn't accept whitespace before the index
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

    [Theory,
     InlineData("SELECT @city", "city", true),
     InlineData("SELECT :city", "city", true),
     InlineData("SELECT $city", "city", true),
     InlineData("SELECT @City", "city", true),
     InlineData("@city", "city", true),
     InlineData("SELECT @city, 1", "city", true),
     InlineData("SELECT @city1, @city", "city", true),
     InlineData("SELECT ?city", "city", true),
     InlineData("SELECT @@city", "city", true),
     InlineData("WHERE [City] = 'x'", "city", false),
     InlineData("SELECT city", "city", false),
     InlineData("SELECT #city", "city", false),
     InlineData("SELECT @city1", "city", false),
     InlineData("SELECT @city_name", "city", false),
     InlineData("SELECT @mycity", "city", false),
     InlineData("SELECT {0}", "city", false),
     InlineData("SELECT 1", "city", false),
     InlineData("", "city", false),
     InlineData("SELECT @city", "", false)]
    public void ReferencesParameterByName(string sql, string name, bool expected)
        => Assert.Equal(expected, RelationalParameterProcessor.ReferencesParameterByName(sql, name));
}
