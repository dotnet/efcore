// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Design.Internal;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class RuntimeModelLinqToCSharpSyntaxTranslator : LinqToCSharpSyntaxTranslator
{
    private Dictionary<MemberAccess, ExpressionSyntax>? _memberAccessReplacements;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RuntimeModelLinqToCSharpSyntaxTranslator(SyntaxGenerator syntaxGenerator) : base(syntaxGenerator)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SyntaxNode TranslateStatement(
        Expression node,
        Dictionary<object, ExpressionSyntax>? constantReplacements,
        Dictionary<MemberAccess, ExpressionSyntax>? memberAccessReplacements,
        ISet<string> collectedNamespaces)
    {
        _memberAccessReplacements = memberAccessReplacements;
        var result = TranslateStatement(node, constantReplacements, collectedNamespaces);
        _memberAccessReplacements = null;
        return result;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SyntaxNode TranslateExpression(
        Expression node,
        Dictionary<object, ExpressionSyntax>? constantReplacements,
        Dictionary<MemberAccess, ExpressionSyntax>? memberAccessReplacements,
        ISet<string> collectedNamespaces)
    {
        _memberAccessReplacements = memberAccessReplacements;
        var result = TranslateExpression(node, constantReplacements, collectedNamespaces);
        _memberAccessReplacements = null;
        return result;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ExpressionSyntax GenerateValue(object? value)
        => value switch
        {
            Snapshot snapshot
                when snapshot == Snapshot.Empty
                => MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    Generate(typeof(Snapshot)),
                    IdentifierName(nameof(Snapshot.Empty))),

            _ => base.GenerateValue(value)
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void TranslateNonPublicFieldAccess(MemberExpression member)
    {
        if (_memberAccessReplacements?.TryGetValue(new MemberAccess(member.Member, assignment: false), out var methodName) == true)
        {
            Result = InvocationExpression(
                methodName,
                ArgumentList(SeparatedList(new[] { Argument(Translate<ExpressionSyntax>(member.Expression)) })));
        }
        else
        {
            base.TranslateNonPublicFieldAccess(member);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void TranslateNonPublicFieldAssignment(MemberExpression member, Expression value)
    {
        if (_memberAccessReplacements?.TryGetValue(new MemberAccess(member.Member, assignment: true), out var methodName) == true)
        {
            Result = InvocationExpression(
                methodName,
                ArgumentList(SeparatedList(new[]
                    {
                        Argument(Translate<ExpressionSyntax>(member.Expression)),
                        Argument(Translate<ExpressionSyntax>(value))
                    })));
        }
        else
        {
            base.TranslateNonPublicFieldAssignment(member, value);
        }
    }
}
