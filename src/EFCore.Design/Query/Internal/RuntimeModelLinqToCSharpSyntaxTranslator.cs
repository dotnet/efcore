// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
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
    private IReadOnlyDictionary<MemberInfo, QualifiedName>? _memberAccessReplacements;

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
        IReadOnlyDictionary<object, string>? constantReplacements,
        IReadOnlyDictionary<MemberInfo, QualifiedName>? memberAccessReplacements,
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
        IReadOnlyDictionary<object, string>? constantReplacements,
        IReadOnlyDictionary<MemberInfo, QualifiedName>? memberAccessReplacements,
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
    protected override void TranslateNonPublicMemberAccess(MemberExpression memberExpression)
    {
        var member = memberExpression.Member is PropertyInfo propertyInfo ? propertyInfo.GetMethod! : memberExpression.Member;
        if (_memberAccessReplacements?.TryGetValue(member, out var methodName) == true)
        {
            AddNamespace(methodName.Namespace);
            Result = InvocationExpression(
                IdentifierName(methodName.Name),
                ArgumentList(SeparatedList(new[] { Argument(Translate<ExpressionSyntax>(memberExpression.Expression)) })));
        }
        else
        {
            base.TranslateNonPublicMemberAccess(memberExpression);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void TranslateNonPublicMemberAssignment(MemberExpression memberExpression, Expression value, SyntaxKind assignmentKind)
    {
        var propertyInfo = memberExpression.Member as PropertyInfo;
        var member = propertyInfo?.SetMethod! ?? memberExpression.Member;
        if (_memberAccessReplacements?.TryGetValue(member, out var methodName) == true)
        {
            AddNamespace(methodName.Namespace);
            if (propertyInfo != null)
            {
                if (assignmentKind is not SyntaxKind.SimpleAssignmentExpression)
                {
                    throw new NotImplementedException("Compound assignment not supported yet.");
                }

                Result = InvocationExpression(
                    IdentifierName(methodName.Name),
                    ArgumentList(SeparatedList(new[]
                        {
                            Argument(Translate<ExpressionSyntax>(memberExpression.Expression)),
                            Argument(Translate<ExpressionSyntax>(value))
                        })));
            }
            else
            {
                Result = AssignmentExpression(assignmentKind,
                    InvocationExpression(
                        IdentifierName(methodName.Name),
                        ArgumentList(SeparatedList(new[] { Argument(Translate<ExpressionSyntax>(memberExpression.Expression)) }))),
                    Translate<ExpressionSyntax>(value));
            }
        }
        else
        {
            base.TranslateNonPublicMemberAssignment(memberExpression, value, assignmentKind);
        }
    }
}
