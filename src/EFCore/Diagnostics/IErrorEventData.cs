// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     An interface implemented by any <see cref="EventData" /> subclass that represents an
    ///     error event with an <see cref="Exception" />.
    /// </summary>
    public interface IErrorEventData
    {
        /// <summary>
        ///     The exception that was thrown to signal the error.
        /// </summary>
        Exception Exception { get; }
    }
}
