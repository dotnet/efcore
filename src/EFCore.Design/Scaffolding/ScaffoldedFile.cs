// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding;

/// <summary>
///     Represents a scaffolded file.
/// </summary>
public class ScaffoldedFile
{
    /// <summary>
    ///     Constructs a new instance of <see cref="ScaffoldedFile" />.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="code">The scaffolded code</param>
    public ScaffoldedFile(string path, string code)
    {
        Path = path;
        Code = code;
    }

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
