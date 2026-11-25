// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

public class RelationalParameterProcessorTest
{
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
