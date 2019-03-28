// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a reference to a metadata object. If the metadata object instance is replaced
    ///     this will be updated with the new object.
    /// </summary>
    /// <typeparam name="T"> The metadata type </typeparam>
    public interface IMetadataReference<out T> : IDisposable
    {
        /// <summary>
        ///     The referenced object.
        /// </summary>
        T Object { get; }
    }
}
