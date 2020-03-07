// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that precomputes a relational model.
    /// </summary>
    public class RelationalModelConvention : IModelFinalizedConvention
    {
        /// <inheritdoc />
        public virtual IModel ProcessModelFinalized(IModel model)
            => model is IConventionModel conventionModel ? RelationalModel.AddRelationalModel(conventionModel) : model;
    }
}
