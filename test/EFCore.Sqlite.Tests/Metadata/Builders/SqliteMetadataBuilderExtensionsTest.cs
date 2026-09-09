// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SqliteMetadataBuilderExtensionsTest
{
    private IConventionModelBuilder CreateBuilder()
        => new InternalModelBuilder(new Model());

    [ConditionalFact]
    public void Can_change_entity_type_UseSqlReturningClause()
    {
        var typeBuilder = CreateBuilder().Entity(typeof(Splot))!;

        Assert.Null(typeBuilder.Metadata.GetUseSqlReturningClauseConfigurationSource());

        Assert.NotNull(typeBuilder.UseSqlReturningClause(true));
        Assert.True(typeBuilder.Metadata.IsSqlReturningClauseUsed());
        Assert.Equal(ConfigurationSource.Convention, typeBuilder.Metadata.GetUseSqlReturningClauseConfigurationSource());

        Assert.NotNull(typeBuilder.UseSqlReturningClause(false, fromDataAnnotation: true));
        Assert.False(typeBuilder.Metadata.IsSqlReturningClauseUsed());
        Assert.Equal(ConfigurationSource.DataAnnotation, typeBuilder.Metadata.GetUseSqlReturningClauseConfigurationSource());

        Assert.Null(typeBuilder.UseSqlReturningClause(true));
        Assert.False(typeBuilder.Metadata.IsSqlReturningClauseUsed());
        Assert.NotNull(typeBuilder.UseSqlReturningClause(false));

        Assert.NotNull(typeBuilder.UseSqlReturningClause(null, fromDataAnnotation: true));
        Assert.True(typeBuilder.Metadata.IsSqlReturningClauseUsed());
        Assert.Null(typeBuilder.Metadata.GetUseSqlReturningClauseConfigurationSource());

        var fragmentId = StoreObjectIdentifier.Table("Split");

        Assert.Null(typeBuilder.Metadata.GetUseSqlReturningClauseConfigurationSource(fragmentId));

        Assert.NotNull(typeBuilder.UseSqlReturningClause(true, fragmentId));
        Assert.True(typeBuilder.Metadata.IsSqlReturningClauseUsed(fragmentId));
        Assert.Equal(ConfigurationSource.Convention, typeBuilder.Metadata.GetUseSqlReturningClauseConfigurationSource(fragmentId));

        Assert.NotNull(typeBuilder.UseSqlReturningClause(false, fragmentId, fromDataAnnotation: true));
        Assert.False(typeBuilder.Metadata.IsSqlReturningClauseUsed(fragmentId));
        Assert.Equal(ConfigurationSource.DataAnnotation, typeBuilder.Metadata.GetUseSqlReturningClauseConfigurationSource(fragmentId));

        Assert.Null(typeBuilder.UseSqlReturningClause(true, fragmentId));
        Assert.False(typeBuilder.Metadata.IsSqlReturningClauseUsed(fragmentId));
        Assert.NotNull(typeBuilder.UseSqlReturningClause(false, fragmentId));

        Assert.NotNull(typeBuilder.UseSqlReturningClause(null, fragmentId, fromDataAnnotation: true));
        Assert.True(typeBuilder.Metadata.IsSqlReturningClauseUsed(fragmentId));
        Assert.Null(typeBuilder.Metadata.GetUseSqlReturningClauseConfigurationSource(fragmentId));
    }

    private class Splot;
}
