// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that checks whether the model is valid.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information.
    /// </remarks>
    [Obsolete("The validation is no longer performed by a convention")]
    public class ValidatingConvention : IModelFinalizedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ValidatingConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public ValidatingConvention(ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies for this service.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <inheritdoc />
        public virtual IModel ProcessModelFinalized(IModel model)
        {
            Dependencies.ModelValidator.Validate(model, Dependencies.ValidationLogger);
            return model;
        }
    }
}
