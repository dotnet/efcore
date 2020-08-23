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
    public class DbFunctionTypeMappingConvention : IModelFinalizingConvention
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

        /// <inheritdoc />
        public virtual void ProcessModelFinalizing(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(context, nameof(context));

            foreach (var dbFunction in modelBuilder.Metadata.GetDbFunctions())
            {
                // TODO: This check needs to be updated to skip over enumerable parameter of aggregate.
                foreach (var parameter in dbFunction.Parameters)
                {
                    parameter.Builder.HasTypeMapping(
                        !string.IsNullOrEmpty(parameter.StoreType)
                            ? _relationalTypeMappingSource.FindMapping(parameter.StoreType)
                            : _relationalTypeMappingSource.FindMapping(parameter.ClrType));
                }

                if (dbFunction.IsScalar)
                {
                    dbFunction.Builder.HasTypeMapping(
                        !string.IsNullOrEmpty(dbFunction.StoreType)
                            ? _relationalTypeMappingSource.FindMapping(dbFunction.StoreType)
                            : _relationalTypeMappingSource.FindMapping(dbFunction.ReturnType));
                }
            }
        }
    }
}
