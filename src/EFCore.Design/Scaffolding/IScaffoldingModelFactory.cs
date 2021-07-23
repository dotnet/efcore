// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
