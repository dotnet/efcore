// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    /// <summary>
    ///     Represents the files added for a model.
    /// </summary>
    public class SavedModelFiles
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SavedModelFiles" /> class.
        /// </summary>
        /// <param name="contextFile">The path of the file containing the <see cref="DbContext" />.</param>
        /// <param name="additionalFiles">The paths of additional files used by the model.</param>
        public SavedModelFiles([NotNull] string contextFile, [NotNull] IEnumerable<string> additionalFiles)
        {
            Check.NotNull(contextFile, nameof(contextFile));
            Check.NotNull(additionalFiles, nameof(additionalFiles));

            ContextFile = contextFile;
            AdditionalFiles = new List<string>(additionalFiles);
        }

        /// <summary>
        ///     Gets or sets the path of the file containing the <see cref="DbContext" />.
        /// </summary>
        /// <value> The path of the file containing the <see cref="DbContext" />. </value>
        public virtual string ContextFile { get; }

        /// <summary>
        ///     Get the paths of additional files used by the model.
        /// </summary>
        /// <value> The paths of additional files used by the model. </value>
        public virtual IList<string> AdditionalFiles { get; }
    }
}
