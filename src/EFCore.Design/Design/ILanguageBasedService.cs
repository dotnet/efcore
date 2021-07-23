// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     Represents a service that gets resolved based on the programming language it supports.
    /// </summary>
    public interface ILanguageBasedService
    {
        /// <summary>
        ///     Gets the programming language supported by this service.
        /// </summary>
        /// <value> The language. </value>
        string? Language { get; }
    }
}
