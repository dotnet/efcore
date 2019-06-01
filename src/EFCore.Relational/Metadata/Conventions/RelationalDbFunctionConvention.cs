// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the name and schema for a <see cref="IDbFunction"/> based on the applied
    ///     <see cref="DbFunctionAttribute"/>.
    /// </summary>
    public class RelationalDbFunctionConvention : IModelAnnotationChangedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="RelationalDbFunctionConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public RelationalDbFunctionConvention(
            [NotNull] ProviderConventionSetBuilderDependencies dependencies,
            [NotNull] RelationalConventionSetBuilderDependencies relationalDependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Called after an annotation is changed on an model.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="name"> The annotation name. </param>
        /// <param name="annotation"> The new annotation. </param>
        /// <param name="oldAnnotation"> The old annotation.  </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessModelAnnotationChanged(
            IConventionModelBuilder modelBuilder,
            string name,
            IConventionAnnotation annotation,
            IConventionAnnotation oldAnnotation,
            IConventionContext<IConventionAnnotation> context)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(name, nameof(name));

            if (name.StartsWith(RelationalAnnotationNames.DbFunction, StringComparison.Ordinal)
                && annotation?.Value != null
                && oldAnnotation == null)
            {
                ProcessDbFunctionAdded(new DbFunctionBuilder((IMutableDbFunction)annotation.Value), context);
            }
        }

        /// <summary>
        ///     Called when an <see cref="IMutableDbFunction"/> is added to the model.
        /// </summary>
        /// <param name="dbFunctionBuilder"> The builder for the <see cref="IMutableDbFunction"/>. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        protected virtual void ProcessDbFunctionAdded(
            [NotNull] IConventionDbFunctionBuilder dbFunctionBuilder, [NotNull] IConventionContext context)
        {
            var methodInfo = dbFunctionBuilder.Metadata.MethodInfo;
            var dbFunctionAttribute = methodInfo.GetCustomAttributes<DbFunctionAttribute>().SingleOrDefault();

            dbFunctionBuilder.HasName(dbFunctionAttribute?.FunctionName ?? methodInfo.Name);
            dbFunctionBuilder.HasSchema(dbFunctionAttribute?.Schema);
        }
    }
}
