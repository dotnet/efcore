// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding;

/// <summary>
///     Represents a scaffolded model.
/// </summary>
public class ScaffoldedModel
{
    /// <summary>
    ///     Gets or sets the generated file containing the <see cref="DbContext" />.
    /// </summary>
    /// <value> The generated file containing the <see cref="DbContext" />. </value>
    public virtual ScaffoldedFile ContextFile { get; set; } = null!;

    /// <summary>
    ///     Gets any additional generated files for the model.
    /// </summary>
    /// <value> Any additional generated files for the model. </value>
    public virtual IList<ScaffoldedFile> AdditionalFiles { get; } = new List<ScaffoldedFile>();
}
