// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
