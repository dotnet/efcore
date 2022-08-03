// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CompiledModelCodeGeneratorSelector : LanguageBasedSelector<ICompiledModelCodeGenerator>,
    ICompiledModelCodeGeneratorSelector
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CompiledModelCodeGeneratorSelector(IEnumerable<ICompiledModelCodeGenerator> services)
        : base(services)
    {
    }

    /// <inheritdoc />
    public virtual ICompiledModelCodeGenerator Select(CompiledModelCodeGenerationOptions options)
        => base.Select(options.Language, Services);
}
