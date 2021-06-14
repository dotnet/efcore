// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Design.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
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
    }
}
