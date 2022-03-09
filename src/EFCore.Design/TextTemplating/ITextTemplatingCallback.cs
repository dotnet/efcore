// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CodeDom.Compiler;
using System.Text;

namespace Microsoft.EntityFrameworkCore.TextTemplating;

/// <summary>
/// Callback interface to be implemented by clients of <see cref="ITextTemplating"/> that wish to process errors and information.
/// </summary>
public interface ITextTemplatingCallback
{
    /// <summary>
    /// Receives errors and warnings.
    /// </summary>
    /// <param name="error">An error or warning.</param>
    void ErrorCallback(CompilerError error);

    /// <summary>
    /// Receives the file name extension that is expected for the generated text output.
    /// </summary>
    /// <param name="extension">The extension.</param>
    void SetFileExtension(string extension);

    /// <summary>
    /// Receives the encoding that is expected for the generated text output.
    /// </summary>
    /// <param name="encoding">The encoding.</param>
    /// <param name="fromOutputDirective">A value indicating whether the encoding was specified in the encoding parameter of the output directive.</param>
    void SetOutputEncoding(Encoding encoding, bool fromOutputDirective);
}
