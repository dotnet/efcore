// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

public class SqliteStringTypeMappingTest
{
    [ConditionalTheory]
    [InlineData("", "''")]
    [InlineData("'Sup", "'''Sup'")]
    [InlineData("I'm", "'I''m'")]
    [InlineData("lovin'", "'lovin'''")]
    [InlineData("it", "'it'")]
    [InlineData("'", "''''")]
    [InlineData("''", "''''''")]
    [InlineData("'m lovin'", "'''m lovin'''")]
    [InlineData("I'm lovin' it", "'I''m lovin'' it'")]
    [InlineData("\r", "CHAR(13)")]
    [InlineData("\n", "CHAR(10)")]
    [InlineData("\r\n", "(CHAR(13) || CHAR(10))")]
    [InlineData("\n'sup", "(CHAR(10) || '''sup')")]
    [InlineData("I'm\n", "('I''m' || CHAR(10))")]
    [InlineData("lovin'\n", "('lovin''' || CHAR(10))")]
    [InlineData("it\n", "('it' || CHAR(10))")]
    [InlineData("\nit", "(CHAR(10) || 'it')")]
    [InlineData("\nit\n", "(CHAR(10) || ('it' || CHAR(10)))")]
    [InlineData("'\n", "('''' || CHAR(10))")]
    public void GenerateProviderValueSqlLiteral_works(string value, string expected)
    {
        var mapping = new SqliteStringTypeMapping("TEXT");
        Assert.Equal(expected, mapping.GenerateProviderValueSqlLiteral(value));
    }
}
