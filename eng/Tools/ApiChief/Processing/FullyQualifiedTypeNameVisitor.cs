// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace ApiChief.Processing;

/// <summary>
/// Visitor that produces a fully-qualified type signature.
/// </summary>
/// <remarks>
/// This probably doesn't do the right thing for nested types, we'll eventually see...
/// </remarks>
internal sealed class FullyQualifiedTypeNameVisitor : CSharpOutputVisitor
{
    public FullyQualifiedTypeNameVisitor(TextWriter tw, CSharpFormattingOptions formatting)
        : base(tw, formatting)
    {
    }

    protected override void WriteCommaSeparatedList(IEnumerable<AstNode> list)
    {
        var isFirst = true;
        foreach (AstNode node in list)
        {
            if (isFirst)
            {
                isFirst = false;
            }
            else
            {
                Comma(node);
                Space();
            }

            node.AcceptVisitor(this);

            if (node.Role == Roles.TypeParameter && node.ToString().EndsWith('?'))
            {
                WriteToken(ComposedType.NullableRole);
            }
        }
    }

    protected override void WriteModifiers(IEnumerable<CSharpModifierToken> modifierTokens)
    {
        foreach (CSharpModifierToken modifier in modifierTokens)
        {
            if (modifier.Modifier == Modifiers.Public)
            {
                continue;
            }

            modifier.AcceptVisitor(this);
            Space();
        }
    }

    public override void VisitNamespaceDeclaration(NamespaceDeclaration namespaceDeclaration)
    {
        StartNode(namespaceDeclaration);

        foreach (var member in namespaceDeclaration.Members)
        {
            member.AcceptVisitor(this);
        }

        EndNode(namespaceDeclaration);
    }

    public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
    {
        StartNode(typeDeclaration);
        WriteModifiers(typeDeclaration.ModifierTokens);

        switch (typeDeclaration.ClassType)
        {
            case ClassType.Enum:
                WriteKeyword(Roles.EnumKeyword);
                break;
            case ClassType.Interface:
                WriteKeyword(Roles.InterfaceKeyword);
                break;
            case ClassType.Struct:
                WriteKeyword(Roles.StructKeyword);
                break;
            case ClassType.RecordClass:
                WriteKeyword(Roles.RecordKeyword);
                break;
            default:
                WriteKeyword(Roles.ClassKeyword);
                break;
        }

        var @namespace = typeDeclaration.Parent as NamespaceDeclaration;

        if (@namespace != null)
        {
            var all = @namespace.Identifiers.Append(typeDeclaration.Name).Select(static s => Identifier.Create(s));
            WriteQualifiedIdentifier(all);
        }
        else
        {
            WriteIdentifier(typeDeclaration.NameToken);
        }

        WriteTypeParameters(typeDeclaration.TypeParameters);

        if (typeDeclaration.PrimaryConstructorParameters.Count > 0)
        {
            Space(policy.SpaceBeforeMethodDeclarationParentheses);
            WriteCommaSeparatedListInParenthesis(typeDeclaration.PrimaryConstructorParameters, policy.SpaceWithinMethodDeclarationParentheses);
        }

        if (typeDeclaration.BaseTypes.Any())
        {
            Space();
            WriteToken(Roles.Colon);
            Space();

            WriteCommaSeparatedList(typeDeclaration.BaseTypes);
        }

        foreach (var constraint in typeDeclaration.Constraints)
        {
            constraint.AcceptVisitor(this);
        }

        EndNode(typeDeclaration);
    }
}
