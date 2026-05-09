// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class SqlServerQueryStringFactoryTest
{
    [ConditionalFact]
    public void Returns_command_text_unchanged_when_no_parameters()
    {
        var factory = CreateFactory();
        using var command = new SqlCommand("SELECT 1");

        Assert.Equal("SELECT 1", factory.Create(command));
    }

    [ConditionalFact]
    public void Renders_DECLARE_with_value_for_simple_int_parameter()
    {
        var factory = CreateFactory();
        using var command = new SqlCommand("SELECT @p0");
        command.Parameters.Add(new SqlParameter("@p0", SqlDbType.Int) { Value = 42 });

        var result = factory.Create(command);

        Assert.Contains("DECLARE @p0 int = 42;", result);
        Assert.EndsWith("SELECT @p0", result);
    }

    [ConditionalFact]
    public void Renders_DECLARE_with_TypeName_and_no_value_for_structured_parameter()
    {
        var factory = CreateFactory();
        using var command = new SqlCommand("SELECT * FROM @data");
        command.Parameters.Add(
            new SqlParameter("@data", SqlDbType.Structured) { TypeName = "dbo.IntList", Value = new DataTable() });

        var result = factory.Create(command);

        // The DECLARE must use the actual TVP type name, not the literal "structured", and must NOT
        // attempt to assign a value since TVPs cannot be initialized inline in T-SQL. See #33849.
        Assert.Contains("DECLARE @data dbo.IntList;", result);
        Assert.DoesNotContain("structured", result);
        Assert.DoesNotContain("DECLARE @data dbo.IntList = ", result);
        Assert.EndsWith("SELECT * FROM @data", result);
    }

    [ConditionalFact]
    public void Renders_structured_parameter_alongside_other_parameters()
    {
        var factory = CreateFactory();
        using var command = new SqlCommand("SELECT * FROM @data WHERE [Region] = @region");
        command.Parameters.Add(
            new SqlParameter("@data", SqlDbType.Structured) { TypeName = "dbo.IntList", Value = new DataTable() });
        command.Parameters.Add(new SqlParameter("@region", SqlDbType.NVarChar, 32) { Value = "EMEA" });

        var result = factory.Create(command);

        Assert.Contains("DECLARE @data dbo.IntList;", result);
        Assert.Contains("DECLARE @region nvarchar(32) = N'EMEA';", result);
        Assert.EndsWith("SELECT * FROM @data WHERE [Region] = @region", result);
    }

    private static SqlServerQueryStringFactory CreateFactory()
    {
        var optionsBuilder = new DbContextOptionsBuilder();
        optionsBuilder.UseSqlServer();
        var singletonOptions = new SqlServerSingletonOptions();
        singletonOptions.Initialize(optionsBuilder.Options);

        return new SqlServerQueryStringFactory(
            new SqlServerTypeMappingSource(
                TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>(),
                singletonOptions));
    }
}
