// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    /// <summary>
    ///     Represents the files added for a model.
    /// </summary>
    public class ModelFiles
    {
        /// <summary>
        ///     Gets or sets the path of the file containing the <see cref="DbContext"/>.
        /// </summary>
        /// <value> The path of the file containing the <see cref="DbContext"/>. </value>
        public virtual string ContextFile { get; [param: NotNull] set; }

        /// <summary>
        ///     Get the paths of additional files used by the model.
        /// </summary>
        /// <value> The paths of additional files used by the model. </value>
        public virtual IList<string> AdditionalFiles { get; } = new List<string>();
    }
}
