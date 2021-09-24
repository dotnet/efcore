// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention configure type mapping for <see cref="IDbFunction" /> instances.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> and
    ///     <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information.
    /// </remarks>
    [Obsolete("Use IModelRuntimeInitializer.Initialize instead.")]
    public class DbFunctionTypeMappingConvention : IModelFinalizingConvention
    {
        private readonly IRelationalTypeMappingSource _relationalTypeMappingSource;

        /// <summary>
        ///     Creates a new instance of <see cref="DbFunctionTypeMappingConvention" />.
        /// </summary>
        /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
        /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
        public DbFunctionTypeMappingConvention(
            ProviderConventionSetBuilderDependencies dependencies,
            RelationalConventionSetBuilderDependencies relationalDependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));
            Check.NotNull(relationalDependencies, nameof(relationalDependencies));

            Dependencies = dependencies;
            RelationalDependencies = relationalDependencies;

            _relationalTypeMappingSource = (IRelationalTypeMappingSource)dependencies.TypeMappingSource;
        }

        /// <summary>
        ///     Dependencies for this service.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Relational provider-specific dependencies for this service.
        /// </summary>
        protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

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
                // Also in DbFunctionParameter.TypeMapping
                foreach (var parameter in dbFunction.Parameters)
                {
                    parameter.Builder!.HasTypeMapping(
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
