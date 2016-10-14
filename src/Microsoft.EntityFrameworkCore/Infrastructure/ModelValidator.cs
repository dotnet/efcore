// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

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
        ///     Creates a new instance of <see cref="ModelValidator"/>.
        /// </summary>
        /// <param name="logger"> The logger. </param>
        protected ModelValidator([NotNull] ILogger<ModelValidator> logger)
        {
            Logger = logger;
        }

        /// <summary>
        ///     Gets the logger.
        /// </summary>
        /// <value>
        ///     The logger.
        /// </value>
        protected virtual ILogger Logger { get; }

        /// <summary>
        ///      Validates a model, throwing an exception if any errors are found.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        public virtual void Validate(IModel model)
        {
        }

        /// <summary>
        ///     Throws an exception with the specified message.
        /// </summary>
        /// <param name="message"> The validation error. </param>
        protected virtual void ShowError([NotNull] string message)
        {
            throw new InvalidOperationException(message);
        }
    }
}
