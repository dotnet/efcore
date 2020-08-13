// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention configure type mapping for <see cref="IDbFunction" /> instances.
    /// </summary>
    public class DbFunctionTypeMappingConvention : IModelFinalizedConvention
    {
        private readonly IRelationalTypeMappingSource _relationalTypeMappingSource;

        /// <summary>
        ///     Creates a new instance of <see cref="DbFunctionTypeMappingConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public DbFunctionTypeMappingConvention(
            [NotNull] ProviderConventionSetBuilderDependencies dependencies,
            [NotNull] RelationalConventionSetBuilderDependencies relationalDependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));
            Check.NotNull(relationalDependencies, nameof(relationalDependencies));

            _relationalTypeMappingSource = (IRelationalTypeMappingSource)dependencies.TypeMappingSource;
        }

        /// <summary>
        ///     Called after a model is finalized.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessModelFinalized(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(context, nameof(context));

            foreach (var dbFunction in modelBuilder.Metadata.GetDbFunctions())
            {
                var typeMapping = !string.IsNullOrEmpty(dbFunction.StoreType)
                    ? _relationalTypeMappingSource.FindMapping(dbFunction.StoreType)
                    : _relationalTypeMappingSource.FindMapping(dbFunction.MethodInfo.ReturnType);

                dbFunction.Builder.HasTypeMapping(typeMapping);

                foreach (var parameter in dbFunction.Parameters)
                {
                    typeMapping = !string.IsNullOrEmpty(parameter.StoreType)
                        ? _relationalTypeMappingSource.FindMapping(parameter.StoreType)
                        : _relationalTypeMappingSource.FindMapping(parameter.ClrType);

                    parameter.Builder.HasTypeMapping(typeMapping);
                }
            }
        }
    }
}
