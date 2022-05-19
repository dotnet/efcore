// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CodeDom.Compiler;
using System.Text;

namespace Microsoft.EntityFrameworkCore.TextTemplating.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class TextTemplatingCallback : ITextTemplatingCallback
{
    private CompilerErrorCollection? _errors;
    private string _extension = ".cs";
    private Encoding _outputEncoding = Encoding.UTF8;
    private bool _fromOutputDirective;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Extension
        => _extension;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual CompilerErrorCollection Errors
        => _errors ??= new CompilerErrorCollection();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Encoding OutputEncoding
        => _outputEncoding;

    void ITextTemplatingCallback.ErrorCallback(CompilerError error)
        => Errors.Add(error);

    void ITextTemplatingCallback.SetFileExtension(string extension)
        => _extension = extension;

    void ITextTemplatingCallback.SetOutputEncoding(Encoding? encoding, bool fromOutputDirective)
    {
        if (_fromOutputDirective)
        {
            return;
        }

        _outputEncoding = encoding ?? Encoding.UTF8;
        _fromOutputDirective = fromOutputDirective;
    }
}
