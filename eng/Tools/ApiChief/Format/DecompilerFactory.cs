// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;

namespace ApiChief.Format;

internal static class DecompilerFactory
{
    private static readonly DecompilerSettings _decompilerSettings = new()
    {
        DecompileMemberBodies = false,
        ShowXmlDocumentation = false,
        ExpandUsingDeclarations = false,
        UsingDeclarations = false,
        ReadOnlyMethods = true,
        AlwaysShowEnumMemberValues = true,
        FileScopedNamespaces = true,
        InitAccessors = true,
        IntroduceRefModifiersOnStructs = true,
        IntroduceReadonlyAndInModifiers = true,
        RecordClasses = true,
        CovariantReturns = true,
        AutomaticProperties = true,
        GetterOnlyAutomaticProperties = true,
        NullPropagation = true,
        NullableReferenceTypes = true,
        OptionalArguments = true,
        OutVariables = true,
        LiftNullables = true,
        CSharpFormattingOptions = Formatter.BaselineFormatting
    };

    public static CSharpDecompiler Create(string path) => new(path, _decompilerSettings);

    public static CSharpDecompiler CreateWithXmlComments(string path)
    {
        var xmlCommentsSettings = _decompilerSettings.Clone();

        xmlCommentsSettings.CSharpFormattingOptions = Formatter.FormattingWithXmlComments;
        xmlCommentsSettings.ShowXmlDocumentation = true;

        return new(path, xmlCommentsSettings);
    }
}
