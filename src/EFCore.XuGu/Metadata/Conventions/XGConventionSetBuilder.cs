// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.XuGu.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class XGConventionSetBuilder : RelationalConventionSetBuilder
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public XGConventionSetBuilder(
            [NotNull] ProviderConventionSetBuilderDependencies dependencies,
            [NotNull] RelationalConventionSetBuilderDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }


        /// <summary>
        ///     Builds and returns the convention set for the current database provider.
        /// </summary>
        /// <returns> The convention set for the current database provider. </returns>
        public override ConventionSet CreateConventionSet()
        {
            var conventionSet = base.CreateConventionSet();

            conventionSet.Add(new XGValueGenerationStrategyConvention(Dependencies, RelationalDependencies));

            conventionSet.ModelInitializedConventions.Add(new RelationalMaxIdentifierLengthConvention(64, Dependencies, RelationalDependencies));

            conventionSet.EntityTypeAddedConventions.Add(new TableCharSetAttributeConvention(Dependencies));
            conventionSet.EntityTypeAddedConventions.Add(new TableCollationAttributeConvention(Dependencies));
            conventionSet.PropertyAddedConventions.Add(new ColumnCharSetAttributeConvention(Dependencies));
            conventionSet.PropertyAddedConventions.Add(new ColumnCollationAttributeConvention(Dependencies));

            var valueGenerationConvention = new XGValueGenerationConvention(Dependencies, RelationalDependencies);
            ReplaceConvention(conventionSet.EntityTypeBaseTypeChangedConventions, valueGenerationConvention);
            ReplaceConvention(conventionSet.EntityTypeAnnotationChangedConventions, (RelationalValueGenerationConvention)valueGenerationConvention);
            ReplaceConvention(conventionSet.EntityTypePrimaryKeyChangedConventions, valueGenerationConvention);
            ReplaceConvention(conventionSet.ForeignKeyAddedConventions, valueGenerationConvention);
            ReplaceConvention(conventionSet.ForeignKeyRemovedConventions, valueGenerationConvention);
            conventionSet.PropertyAnnotationChangedConventions.Add(valueGenerationConvention);

            ReplaceConvention(
                conventionSet.ModelFinalizedConventions,
                (RuntimeModelConvention)new XGRuntimeModelConvention(Dependencies, RelationalDependencies));

            return conventionSet;
        }

        /// <summary>
        ///     <para>
        ///         Call this method to build a <see cref="ConventionSet" /> for MySQL when using
        ///         the <see cref="ModelBuilder" /> outside of <see cref="DbContext.OnModelCreating" />.
        ///     </para>
        ///     <para>
        ///         Note that it is unusual to use this method.
        ///         Consider using <see cref="DbContext" /> in the normal way instead.
        ///     </para>
        /// </summary>
        /// <returns> The convention set. </returns>
        public static ConventionSet Build()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkXG()
                .AddDbContext<DbContext>(o => o.UseXG("Server=.", XGServerVersion.LatestSupportedServerVersion))
                .BuildServiceProvider();

            using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<DbContext>())
                {
                    return ConventionSet.CreateConventionSet(context);
                }
            }
        }
    }
}
