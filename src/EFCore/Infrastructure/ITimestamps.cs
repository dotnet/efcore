// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    /// An interface to save create and delete timestamps in models
    /// </summary>
    public interface ITimestamps
    {
        /// <summary>
        /// To save record create time
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// To save record update time
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
