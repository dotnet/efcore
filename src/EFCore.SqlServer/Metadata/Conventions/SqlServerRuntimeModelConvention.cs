// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that creates an optimized copy of the mutable model.
    /// </summary>
    public class SqlServerRuntimeModelConvention : RelationalRuntimeModelConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="RelationalModelConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public SqlServerRuntimeModelConvention(
            ProviderConventionSetBuilderDependencies dependencies,
            RelationalConventionSetBuilderDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        /// <summary>
        ///     Updates the model annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="model"> The source model. </param>
        /// <param name="runtimeModel"> The target model that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected override void ProcessModelAnnotations(
            Dictionary<string, object?> annotations, IModel model, RuntimeModel runtimeModel, bool runtime)
        {
            base.ProcessModelAnnotations(annotations, model, runtimeModel, runtime);

            if (!runtime)
            {
                annotations.Remove(SqlServerAnnotationNames.IdentityIncrement);
                annotations.Remove(SqlServerAnnotationNames.IdentitySeed);
                annotations.Remove(SqlServerAnnotationNames.MaxDatabaseSize);
                annotations.Remove(SqlServerAnnotationNames.PerformanceLevelSql);
                annotations.Remove(SqlServerAnnotationNames.ServiceTierSql);
            }
        }

        /// <summary>
        ///     Updates the property annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="property"> The source property. </param>
        /// <param name="runtimeProperty"> The target property that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected override void ProcessPropertyAnnotations(
            Dictionary<string, object?> annotations, IProperty property, RuntimeProperty runtimeProperty, bool runtime)
        {
            base.ProcessPropertyAnnotations(annotations, property, runtimeProperty, runtime);

            if (!runtime)
            {
                annotations.Remove(SqlServerAnnotationNames.IdentityIncrement);
                annotations.Remove(SqlServerAnnotationNames.IdentitySeed);
                annotations.Remove(SqlServerAnnotationNames.Sparse);

                if (!annotations.ContainsKey(SqlServerAnnotationNames.ValueGenerationStrategy))
                {
                    annotations[SqlServerAnnotationNames.ValueGenerationStrategy] = property.GetValueGenerationStrategy();
                }
            }
        }

        /// <summary>
        ///     Updates the index annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="index"> The source index. </param>
        /// <param name="runtimeIndex"> The target index that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected override void ProcessIndexAnnotations(
            Dictionary<string, object?> annotations,
            IIndex index,
            RuntimeIndex runtimeIndex,
            bool runtime)
        {
            base.ProcessIndexAnnotations(annotations, index, runtimeIndex, runtime);

            if (!runtime)
            {
                annotations.Remove(SqlServerAnnotationNames.Clustered);
                annotations.Remove(SqlServerAnnotationNames.CreatedOnline);
                annotations.Remove(SqlServerAnnotationNames.Include);
                annotations.Remove(SqlServerAnnotationNames.FillFactor);
            }
        }

        /// <summary>
        ///     Updates the key annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="key"> The source key. </param>
        /// <param name="runtimeKey"> The target key that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected override void ProcessKeyAnnotations(
            IDictionary<string, object?> annotations,
            IKey key,
            RuntimeKey runtimeKey,
            bool runtime)
        {
            base.ProcessKeyAnnotations(annotations, key, runtimeKey, runtime);

            if (!runtime)
            {
                annotations.Remove(SqlServerAnnotationNames.Clustered);
            }
        }
    }
}
