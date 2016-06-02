// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     The action that is taken when a warning is encountered.
    /// </summary>
    public enum WarningBehavior
    {
        /// <summary>
        ///     A warning is logged.
        /// </summary>
        Log,

        /// <summary>
        ///     No action is taken.
        /// </summary>
        Ignore,

        /// <summary>
        ///     An exception is thrown.
        /// </summary>
        Throw
    }
}
