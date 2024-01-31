// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design.Internal;

/// <summary>
///     The parameter object for a <see cref="ICSharpRuntimeAnnotationCodeGenerator" />
/// </summary>
public sealed record CSharpRuntimeAnnotationCodeGeneratorParameters
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <remarks>
    ///     Do not call this constructor directly from either provider or application code as it may change
    ///     as new dependencies are added. Instead, use this type in your constructor so that an instance
    ///     will be created and injected automatically by the dependency injection container. To create
    ///     an instance with some dependent services replaced, first resolve the object from the dependency
    ///     injection container, then replace selected services using the C# 'with' operator. Do not call
    ///     the constructor at any point in this process.
    /// </remarks>
    [EntityFrameworkInternal]
    public CSharpRuntimeAnnotationCodeGeneratorParameters(
        string targetName,
        string className,
        IndentedStringBuilder mainBuilder,
        IndentedStringBuilder methodBuilder,
        ISet<string> namespaces,
        ISet<string> scopeVariables,
        Dictionary<ITypeBase, string> configurationClassNames,
        bool nullable)
    {
        TargetName = targetName;
        ClassName = className;
        MainBuilder = mainBuilder;
        MethodBuilder = methodBuilder;
        Namespaces = namespaces;
        ScopeVariables = scopeVariables;
        ConfigurationClassNames = configurationClassNames;
        UseNullableReferenceTypes = nullable;
    }

    /// <summary>
    ///     The set of annotations from which to generate fluent API calls.
    /// </summary>
    public IDictionary<string, object?> Annotations { get; init; } = null!;

    /// <summary>
    ///     The name of the target variable.
    /// </summary>
    public string TargetName { get; init; }

    /// <summary>
    ///     The name of the current class.
    /// </summary>
    public string ClassName { get; init; }

    /// <summary>
    ///     The builder for the code building the metadata item.
    /// </summary>
    public IndentedStringBuilder MainBuilder { get; init; }

    /// <summary>
    ///     The builder that could be used to add members to the current class.
    /// </summary>
    public IndentedStringBuilder MethodBuilder { get; init; }

    /// <summary>
    ///     A collection of namespaces for <see langword="using" /> generation.
    /// </summary>
    public ISet<string> Namespaces { get; init; }

    /// <summary>
    ///     A collection of variable names in the current scope.
    /// </summary>
    public ISet<string> ScopeVariables { get; init; }

    /// <summary>
    ///     The configuration class names corresponding to the structural types.
    /// </summary>
    public IReadOnlyDictionary<ITypeBase, string> ConfigurationClassNames { get; init; }

    /// <summary>
    ///     Indicates whether the given annotations are runtime annotations.
    /// </summary>
    public bool IsRuntime { get; init; }

    /// <summary>
    ///     Gets or sets a value indicating whether nullable reference types are enabled.
    /// </summary>
    /// <value>A value indicating whether nullable reference types are enabled.</value>
    public bool UseNullableReferenceTypes { get; init; }
}
