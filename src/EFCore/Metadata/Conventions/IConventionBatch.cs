// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an object that delays any convention invocations until it is run or disposed.
    /// </summary>
    public interface IConventionBatch : IDisposable
    {
        /// <summary>
        ///     Runs the delayed conventions while tracking changes to the given foreign key.
        /// </summary>
        /// <param name="foreignKey"> The foreign key to track. </param>
        /// <returns> The new foreign key object if the given one was replaced by a convention. </returns>
        IConventionForeignKey? Run(IConventionForeignKey foreignKey);

        /// <summary>
        ///     Starts tracking changes to the given foreign key.
        /// </summary>
        /// <param name="foreignKey"> The foreign key to track. </param>
        /// <returns>
        ///     An object that will contain the reference to the new foreign key instance if the given one was replaced by a convention.
        ///     Otherwise, returns the original foreign key.
        /// </returns>
        IMetadataReference<IConventionForeignKey> Track(IConventionForeignKey foreignKey);
    }
}
