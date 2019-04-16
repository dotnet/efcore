// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    /// <summary>
    ///     Represents a scaffolded model.
    /// </summary>
    public class ScaffoldedModel
    {
        /// <summary>
        ///     Gets or sets the generated file containing the <see cref="DbContext" />.
        /// </summary>
        /// <value> The generated file containing the <see cref="DbContext" />. </value>
        public virtual ScaffoldedFile ContextFile { get; [param: NotNull] set; }

        /// <summary>
        ///     Gets any additional generated files for the model.
        /// </summary>
        /// <value> Any additional generated files for the model. </value>
        public virtual IList<ScaffoldedFile> AdditionalFiles { get; } = new List<ScaffoldedFile>();
    }
}
