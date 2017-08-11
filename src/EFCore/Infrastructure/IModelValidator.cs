// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Validates a model after it is built.
    /// </summary>
    public interface IModelValidator
    {
        /// <summary>
        ///     Validates a model, throwing an exception if any errors are found.
        /// </summary>
        void Validate([NotNull] IModel model);
    }
}
