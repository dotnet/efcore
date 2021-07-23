// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the entity type to which a queryable function is mapped.
    /// </summary>
    public class TableValuedDbFunctionConvention : IModelFinalizingConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="TableValuedDbFunctionConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public TableValuedDbFunctionConvention(
            ProviderConventionSetBuilderDependencies dependencies,
            RelationalConventionSetBuilderDependencies relationalDependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <inheritdoc />
        public virtual void ProcessModelFinalizing(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var function in modelBuilder.Metadata.GetDbFunctions())
            {
                ProcessDbFunctionAdded(function.Builder, context);
            }
        }

        /// <summary>
        ///     Called when an <see cref="IConventionDbFunction" /> is added to the model.
        /// </summary>
        /// <param name="dbFunctionBuilder"> The builder for the <see cref="IConventionDbFunction" />. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        private void ProcessDbFunctionAdded(
            IConventionDbFunctionBuilder dbFunctionBuilder,
            IConventionContext context)
        {
            var function = dbFunctionBuilder.Metadata;
            if (function.IsScalar)
            {
                return;
            }

            var elementType = function.ReturnType.TryGetElementType(typeof(IQueryable<>))!;
            if (!elementType.IsValidEntityType())
            {
                throw new InvalidOperationException(
                    RelationalStrings.DbFunctionInvalidIQueryableReturnType(
                        function.ModelName, function.ReturnType.ShortDisplayName()));
            }

            var model = function.Model;
            var entityType = model.FindEntityType(elementType);
            if (entityType?.IsOwned() == true
                || model.IsOwned(elementType)
                || (entityType == null && model.FindEntityTypes(elementType).Any()))
            {
                return;
            }

            dbFunctionBuilder.ModelBuilder.Entity(elementType);
        }
    }
}
