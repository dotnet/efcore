// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.XuGu.Infrastructure
{
    public enum ServerType
    {
        /// <summary>
        /// Custom server implementation
        /// </summary>
        Custom = -1,

        /// <summary>
        /// MySQL server
        /// </summary>
        XG,

        /// <summary>
        /// MariaDB server
        /// </summary>
        MariaDb
    }
}
