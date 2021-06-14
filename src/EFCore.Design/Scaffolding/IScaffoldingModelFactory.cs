// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    /// <summary>
    ///     Used to create an <see cref="IModel"/> from a <see cref="DatabaseModel"/>.
    /// </summary>
    public interface IScaffoldingModelFactory
    {
        /// <summary>
        ///     Creates an <see cref="IModel"/> from a <see cref="DatabaseModel"/>.
        /// </summary>
        /// <param name="databaseModel"> The database model. </param>
        /// <param name="options"> The options to use while creating the model. </param>
        /// <returns> The model. </returns>
        IModel Create(DatabaseModel databaseModel, ModelReverseEngineerOptions options);
    }
}
