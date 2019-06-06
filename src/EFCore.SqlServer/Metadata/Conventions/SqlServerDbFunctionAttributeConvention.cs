// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures model function mappings based on public static methods on the context marked with
    ///     <see cref="DbFunctionAttribute"/> and sets the default schema to 'dbo'.
    /// </summary>
    public class SqlServerDbFunctionAttributeConvention : RelationalDbFunctionAttributeConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="SqlServerDbFunctionAttributeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public SqlServerDbFunctionAttributeConvention(
            [NotNull] ProviderConventionSetBuilderDependencies dependencies,
            [NotNull] RelationalConventionSetBuilderDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        /// <summary>
        ///     Called when an <see cref="IMutableDbFunction"/> is added to the model.
        /// </summary>
        /// <param name="dbFunctionBuilder"> The builder for the <see cref="IMutableDbFunction"/>. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        protected override void ProcessDbFunctionAdded(
            IConventionDbFunctionBuilder dbFunctionBuilder, IConventionContext context)
        {
            base.ProcessDbFunctionAdded(dbFunctionBuilder, context);

            ((DbFunction)dbFunctionBuilder.Metadata).DefaultSchema = "dbo";
        }
    }
}
