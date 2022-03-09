// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.VisualStudio.TextTemplating;

namespace Microsoft.EntityFrameworkCore.TextTemplating;

/// <summary>
/// The text template transformation service.
/// </summary>
public interface ITextTemplating : ITextTemplatingSessionHost
{
    /// <summary>
    /// Transforms the contents of a text template file to produce the generated text output.
    /// </summary>
    /// <param name="inputFile">The path of the template file.</param>
    /// <param name="content">The contents of the template file.</param>
    /// <param name="callback">The callback used to process errors and information.</param>
    /// <returns>The output.</returns>
    string ProcessTemplate(string inputFile, string content, ITextTemplatingCallback? callback = null);
}
