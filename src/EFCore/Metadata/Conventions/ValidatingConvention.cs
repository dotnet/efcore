// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that checks whether the model is valid.
    /// </summary>
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
        ///     Parameter object containing service dependencies.
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
