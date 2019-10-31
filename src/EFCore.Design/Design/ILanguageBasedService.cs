// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
        string Language { get; }
    }
}
