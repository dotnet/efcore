// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.SqlServer.Design.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <remarks>
    ///     The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///     is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///     This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    /// </remarks>
#pragma warning disable EF1001 // Internal EF Core API usage.
    public class SqlServerCSharpRuntimeAnnotationCodeGenerator : RelationalCSharpRuntimeAnnotationCodeGenerator
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerCSharpRuntimeAnnotationCodeGenerator(
            CSharpRuntimeAnnotationCodeGeneratorDependencies dependencies,
            RelationalCSharpRuntimeAnnotationCodeGeneratorDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        /// <inheritdoc />
        public override void Generate(IModel model, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            if (!parameters.IsRuntime)
            {
                var annotations = parameters.Annotations;
                annotations.Remove(SqlServerAnnotationNames.IdentityIncrement);
                annotations.Remove(SqlServerAnnotationNames.IdentitySeed);
                annotations.Remove(SqlServerAnnotationNames.MaxDatabaseSize);
                annotations.Remove(SqlServerAnnotationNames.PerformanceLevelSql);
                annotations.Remove(SqlServerAnnotationNames.ServiceTierSql);
            }

            base.Generate(model, parameters);
        }

        /// <inheritdoc />
        public override void Generate(IProperty property, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            if (!parameters.IsRuntime)
            {
                var annotations = parameters.Annotations;
                annotations.Remove(SqlServerAnnotationNames.IdentityIncrement);
                annotations.Remove(SqlServerAnnotationNames.IdentitySeed);
                annotations.Remove(SqlServerAnnotationNames.Sparse);

                if (!annotations.ContainsKey(SqlServerAnnotationNames.ValueGenerationStrategy))
                {
                    annotations[SqlServerAnnotationNames.ValueGenerationStrategy] = property.GetValueGenerationStrategy();
                }
            }

            base.Generate(property, parameters);
        }

        /// <inheritdoc />
        public override void Generate(IIndex index, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            if (!parameters.IsRuntime)
            {
                var annotations = parameters.Annotations;
                annotations.Remove(SqlServerAnnotationNames.Clustered);
                annotations.Remove(SqlServerAnnotationNames.CreatedOnline);
                annotations.Remove(SqlServerAnnotationNames.Include);
                annotations.Remove(SqlServerAnnotationNames.FillFactor);
            }

            base.Generate(index, parameters);
        }

        /// <inheritdoc />
        public override void Generate(IKey key, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            if (!parameters.IsRuntime)
            {
                var annotations = parameters.Annotations;
                annotations.Remove(SqlServerAnnotationNames.Clustered);
            }

            base.Generate(key, parameters);
        }

        /// <inheritdoc />
        public override void Generate(IEntityType entityType, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
        {
            if (!parameters.IsRuntime)
            {
                var annotations = parameters.Annotations;
                annotations.Remove(SqlServerAnnotationNames.TemporalHistoryTableName);
                annotations.Remove(SqlServerAnnotationNames.TemporalHistoryTableSchema);
                annotations.Remove(SqlServerAnnotationNames.TemporalPeriodEndColumnName);
                annotations.Remove(SqlServerAnnotationNames.TemporalPeriodEndPropertyName);
                annotations.Remove(SqlServerAnnotationNames.TemporalPeriodStartColumnName);
                annotations.Remove(SqlServerAnnotationNames.TemporalPeriodStartPropertyName);
            }

            base.Generate(entityType, parameters);
        }
    }
}
