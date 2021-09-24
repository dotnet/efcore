// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a reference to a metadata object. If the metadata object instance is replaced
    ///     this will be updated with the new object.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information.
    /// </remarks>
    /// <typeparam name="T">The metadata type</typeparam>
    public interface IMetadataReference<out T> : IDisposable
    {
        /// <summary>
        ///     The referenced object.
        /// </summary>
        [MaybeNull]
        T Object { get; }
    }
}
