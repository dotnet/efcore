// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding;

/// <summary>
///     Represents a scaffolded file.
/// </summary>
public class ScaffoldedFile
{
    /// <summary>
    ///     Gets or sets the path.
    /// </summary>
    /// <value> The path. </value>
    public virtual string Path { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the scaffolded code.
    /// </summary>
    /// <value> The scaffolded code. </value>
    public virtual string Code { get; set; } = null!;
}
