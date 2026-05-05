// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.SqlAzure.Model;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.SqlAzure;

#nullable disable

[SqlServerCondition(SqlServerCondition.IsSqlAzure)]
#pragma warning disable CS9113 // Parameter is unread.
public class SqlAzureConnectionTest(SqlAzureFixture fixture) : IClassFixture<SqlAzureFixture>
#pragma warning restore CS9113 // Parameter is unread.
{
    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Connect_with_encryption(bool encryptionEnabled)
    {
        var connectionStringBuilder =
            new SqlConnectionStringBuilder(SqlServerTestStore.CreateConnectionString("adventureworks")) { Encrypt = encryptionEnabled };
        var options = new DbContextOptionsBuilder();
        options.UseSqlServer(connectionStringBuilder.ConnectionString, b => b.ApplyConfiguration());

        using var context = new AdventureWorksContext(options.Options);
        context.Database.OpenConnection();
        Assert.Equal(ConnectionState.Open, context.Database.GetDbConnection().State);
    }
}
