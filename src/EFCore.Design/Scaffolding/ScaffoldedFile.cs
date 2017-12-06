// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    /// <summary>
    ///     Represents a scaffolded file.
    /// </summary>
    public class ScaffoldedFile
    {
        /// <summary>
        ///     Gets or sets the path.
        /// </summary>
        /// <value> The path. </value>
        public virtual string Path { get; [param: NotNull] set; }

        /// <summary>
        ///     Gets or sets the scaffolded code.
        /// </summary>
        /// <value> The scaffolded code. </value>
        public virtual string Code { get; [param: NotNull] set; }
    }
}
