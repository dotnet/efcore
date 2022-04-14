// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

public class SqlServerStringTypeMappingTest
{
    [ConditionalTheory]
    [InlineData("", "''")]
    [InlineData("'Sup", "'''Sup'")]
    [InlineData("I'm", "'I''m'")]
    [InlineData("lovin'", "'lovin'''")]
    [InlineData("it", "'it'")]
    [InlineData("'", "''''")]
    [InlineData("''", "''''''")]
    [InlineData("I'm lovin'", "'I''m lovin'''")]
    [InlineData("I'm lovin' it", "'I''m lovin'' it'")]
    [InlineData("\r", "char(13)")]
    [InlineData("\n", "char(10)")]
    [InlineData("\r\n", "CONCAT(CAST(char(13) AS varchar(max)), char(10))")]
    [InlineData("\n'sup", "CONCAT(CAST(char(10) AS varchar(max)), '''sup')")]
    [InlineData("I'm\n", "CONCAT(CAST('I''m' AS varchar(max)), char(10))")]
    [InlineData("lovin'\n", "CONCAT(CAST('lovin''' AS varchar(max)), char(10))")]
    [InlineData("it\n", "CONCAT(CAST('it' AS varchar(max)), char(10))")]
    [InlineData("\nit", "CONCAT(CAST(char(10) AS varchar(max)), 'it')")]
    [InlineData("\nit\n", "CONCAT(CAST(char(10) AS varchar(max)), 'it', char(10))")]
    [InlineData("'\n", "CONCAT(CAST('''' AS varchar(max)), char(10))")]
    public void GenerateProviderValueSqlLiteral_works(string value, string expected)
    {
        var mapping = new SqlServerStringTypeMapping("varchar(max)");
        Assert.Equal(expected, mapping.GenerateProviderValueSqlLiteral(value));
    }

    [ConditionalTheory]
    [InlineData("", "N''")]
    [InlineData("'Sup", "N'''Sup'")]
    [InlineData("I'm", "N'I''m'")]
    [InlineData("lovin'", "N'lovin'''")]
    [InlineData("it", "N'it'")]
    [InlineData("'", "N''''")]
    [InlineData("''", "N''''''")]
    [InlineData("I'm lovin'", "N'I''m lovin'''")]
    [InlineData("I'm lovin' it", "N'I''m lovin'' it'")]
    [InlineData("\r", "nchar(13)")]
    [InlineData("\n", "nchar(10)")]
    [InlineData("\r\n", "CONCAT(CAST(nchar(13) AS nvarchar(max)), nchar(10))")]
    [InlineData("\n'sup", "CONCAT(CAST(nchar(10) AS nvarchar(max)), N'''sup')")]
    [InlineData("I'm\n", "CONCAT(CAST(N'I''m' AS nvarchar(max)), nchar(10))")]
    [InlineData("lovin'\n", "CONCAT(CAST(N'lovin''' AS nvarchar(max)), nchar(10))")]
    [InlineData("it\n", "CONCAT(CAST(N'it' AS nvarchar(max)), nchar(10))")]
    [InlineData("\nit", "CONCAT(CAST(nchar(10) AS nvarchar(max)), N'it')")]
    [InlineData("\nit\n", "CONCAT(CAST(nchar(10) AS nvarchar(max)), N'it', nchar(10))")]
    [InlineData("'\n", "CONCAT(CAST(N'''' AS nvarchar(max)), nchar(10))")]
    public void GenerateProviderValueSqlLiteral_works_unicode(string value, string expected)
    {
        var mapping = new SqlServerStringTypeMapping("nvarchar(max)", unicode: true);
        Assert.Equal(expected, mapping.GenerateProviderValueSqlLiteral(value));
    }
}
