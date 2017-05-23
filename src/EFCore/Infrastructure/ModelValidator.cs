// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         A base class for provider implementations of <see cref="IModelValidator" />
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public abstract class ModelValidator : IModelValidator
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ModelValidator" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        protected ModelValidator([NotNull] ModelValidatorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies used to create a <see cref="ModelValidator" />
        /// </summary>
        protected virtual ModelValidatorDependencies Dependencies { get; }

        /// <summary>
        ///     Validates a model, throwing an exception if any errors are found.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        public virtual void Validate(IModel model)
        {
        }
    }
}
