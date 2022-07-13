// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding;

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
    public SavedModelFiles(string contextFile, IEnumerable<string> additionalFiles)
    {
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
