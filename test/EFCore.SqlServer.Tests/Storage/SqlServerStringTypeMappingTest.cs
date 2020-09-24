// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal
{
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
        [InlineData("\r", "CHAR(13)")]
        [InlineData("\n", "CHAR(10)")]
        [InlineData("\r\n", "CONCAT(CHAR(13), CHAR(10))")]
        [InlineData("\n'sup", "CONCAT(CHAR(10), '''sup')")]
        [InlineData("I'm\n", "CONCAT('I''m', CHAR(10))")]
        [InlineData("lovin'\n", "CONCAT('lovin''', CHAR(10))")]
        [InlineData("it\n", "CONCAT('it', CHAR(10))")]
        [InlineData("\nit", "CONCAT(CHAR(10), 'it')")]
        [InlineData("\nit\n", "CONCAT(CHAR(10), 'it', CHAR(10))")]
        [InlineData("'\n", "CONCAT('''', CHAR(10))")]
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
        [InlineData("\r", "NCHAR(13)")]
        [InlineData("\n", "NCHAR(10)")]
        [InlineData("\r\n", "CONCAT(NCHAR(13), NCHAR(10))")]
        [InlineData("\n'sup", "CONCAT(NCHAR(10), N'''sup')")]
        [InlineData("I'm\n", "CONCAT(N'I''m', NCHAR(10))")]
        [InlineData("lovin'\n", "CONCAT(N'lovin''', NCHAR(10))")]
        [InlineData("it\n", "CONCAT(N'it', NCHAR(10))")]
        [InlineData("\nit", "CONCAT(NCHAR(10), N'it')")]
        [InlineData("\nit\n", "CONCAT(NCHAR(10), N'it', NCHAR(10))")]
        [InlineData("'\n", "CONCAT(N'''', NCHAR(10))")]
        public void GenerateProviderValueSqlLiteral_works_unicode(string value, string expected)
        {
            var mapping = new SqlServerStringTypeMapping("nvarchar(max)", unicode: true);
            Assert.Equal(expected, mapping.GenerateProviderValueSqlLiteral(value));
        }
    }
}
