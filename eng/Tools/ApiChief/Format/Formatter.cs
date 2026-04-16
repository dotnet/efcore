// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using ApiChief.Processing;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.Output;
using ICSharpCode.Decompiler.TypeSystem;

namespace ApiChief.Format;

internal static class Formatter
{
    private static readonly CSharpAmbience _typeAmbience = new()
    {
        ConversionFlags = ConversionFlags.All & ~(ConversionFlags.ShowAccessibility | ConversionFlags.PlaceReturnTypeAfterParameterList)
    };

    private static readonly CSharpAmbience _fieldAmbience = new()
    {
        ConversionFlags =
                    ConversionFlags.ShowModifiers |
                    ConversionFlags.ShowParameterDefaultValues |
                    ConversionFlags.ShowParameterList |
                    ConversionFlags.ShowParameterModifiers |
                    ConversionFlags.ShowTypeParameterList |
                    ConversionFlags.ShowReturnType |
                    ConversionFlags.SupportInitAccessors |
                    ConversionFlags.SupportRecordClasses |
                    ConversionFlags.UseFullyQualifiedEntityNames |
                    ConversionFlags.UseNullableSpecifierForValueTypes |
                    ConversionFlags.UseFullyQualifiedTypeNames
    };

    private static readonly CSharpAmbience _propertyAmbience = new()
    {
        ConversionFlags =
            ConversionFlags.All
                & ~(ConversionFlags.PlaceReturnTypeAfterParameterList
                | ConversionFlags.ShowAccessibility
                | ConversionFlags.ShowDeclaringType
                | ConversionFlags.ShowTypeParameterVarianceModifier),
    };

    private static readonly CSharpAmbience _methodAmbience = new()
    {
        ConversionFlags =
            ConversionFlags.All
                & ~(ConversionFlags.PlaceReturnTypeAfterParameterList
                | ConversionFlags.ShowAccessibility
                | ConversionFlags.ShowDeclaringType
                | ConversionFlags.ShowTypeParameterVarianceModifier),
    };

    private static readonly CSharpAmbience _implicitOperatorAmbience = new()
    {
        ConversionFlags =
            ConversionFlags.All
                    & ~(ConversionFlags.PlaceReturnTypeAfterParameterList
                    | ConversionFlags.ShowAccessibility
                    | ConversionFlags.ShowDeclaringType
                    | ConversionFlags.ShowReturnType
                    | ConversionFlags.ShowTypeParameterVarianceModifier),
    };

    public static CSharpFormattingOptions BaselineFormatting { get; } = GetBaselineFormatting();

    public static CSharpFormattingOptions FormattingWithXmlComments { get; } = GetFormattingWithXmlComments();

    public static string TypeToString(ITypeDefinition type, CSharpDecompiler decompiler)
    {
        if (type.Kind == TypeKind.Delegate)
        {
            return _typeAmbience.ConvertSymbol(type).WithSpaceBetweenParameters().Replace(";", string.Empty);
        }

        var syntaxTree = decompiler.DecompileType(type.FullTypeName);

        using var writer = new StringWriter();
        var visitor = new FullyQualifiedTypeNameVisitor(writer, BaselineFormatting);
        syntaxTree.AcceptVisitor(visitor);

        return writer.ToString();
    }

    public static string NestedTypeToString(ITypeDefinition nested)
        => _typeAmbience.ConvertSymbol(nested).WithSpaceBetweenParameters();

    public static string PropertyToString(IProperty property)
    {
        using var writer = new StringWriter();
        _propertyAmbience.ConvertSymbol(property, new TextWriterTokenWriter(writer), BaselineFormatting);

        var propertyString = writer.ToString()
            .WithSpaceBetweenParameters();

        return property.Setter?.Accessibility switch
        {
            Accessibility.Private => propertyString.Replace("set;", "private set;"),
            Accessibility.Protected => propertyString.Replace("set;", "protected set;"),
            Accessibility.Internal => propertyString.Replace("set;", "internal set;"),
            Accessibility.ProtectedOrInternal => propertyString.Replace("set;", "protected internal set;"),
            _ => propertyString
        };
    }

    public static string MethodToString(IMethod method)
    {
        var ambience = method.IsOperator && (method.Name == "op_Implicit" || method.Name == "op_Explicit")
            ? _implicitOperatorAmbience
            : _methodAmbience;

        using var writer = new StringWriter();
        ambience.ConvertSymbol(method, new TextWriterTokenWriter(writer), BaselineFormatting);

        return writer.ToString()
             .WithSpaceBetweenParameters()
             .WithNumbersWithoutLiterals();
    }

    public static string FieldToString(IField field)
        => _fieldAmbience.ConvertSymbol(field).WithSpaceBetweenParameters();

    private static CSharpFormattingOptions GetBaselineFormatting()
    {
        var formatting = FormattingOptionsFactory.CreateAllman().Clone();
        formatting.IndentationString = "    ";
        formatting.MinimumBlankLinesBetweenFields = 0;
        formatting.MinimumBlankLinesBetweenMembers = 0;

        return formatting;
    }

    private static CSharpFormattingOptions GetFormattingWithXmlComments()
    {
        var formatting = FormattingOptionsFactory.CreateAllman().Clone();
        formatting.IndentationString = "    ";
        formatting.MinimumBlankLinesBetweenFields = 1;
        formatting.MinimumBlankLinesBetweenMembers = 1;

        return formatting;
    }
}
