// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.EntityFrameworkCore
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class InternalUsageDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        public const string Id = "EF1001";

        public const string MessageFormat
            = "{0} is an internal API that supports the Entity Framework Core infrastructure and "
            + "not subject to the same compatibility standards as public APIs. "
            + "It may be changed or removed without notice in any release.";

        protected const string DefaultTitle = "Internal EF Core API usage.";
        protected const string Category = "Usage";

        private static readonly int EFLen = "EntityFrameworkCore".Length;

        private static readonly DiagnosticDescriptor _descriptor
            = new DiagnosticDescriptor(
                Id,
                title: DefaultTitle,
                messageFormat: MessageFormat,
                category: Category,
                defaultSeverity: DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterOperationAction(AnalyzeNode,
                OperationKind.FieldReference,
                OperationKind.PropertyReference,
                OperationKind.MethodReference,
                OperationKind.EventReference,
                OperationKind.Invocation,
                OperationKind.ObjectCreation,
                OperationKind.VariableDeclaration,
                OperationKind.TypeOf);

            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeNode(OperationAnalysisContext context)
        {
            switch (context.Operation.Kind)
            {
                case OperationKind.FieldReference:
                    AnalyzeMember(context, ((IFieldReferenceOperation)context.Operation).Field);
                    break;
                case OperationKind.PropertyReference:
                    AnalyzeMember(context, ((IPropertyReferenceOperation)context.Operation).Property);
                    break;
                case OperationKind.EventReference:
                    AnalyzeMember(context, ((IEventReferenceOperation)context.Operation).Event);
                    break;
                case OperationKind.MethodReference:
                    AnalyzeMember(context, ((IMethodReferenceOperation)context.Operation).Method);
                    break;
                case OperationKind.ObjectCreation:
                    AnalyzeMember(context, ((IObjectCreationOperation)context.Operation).Constructor);
                    break;

                case OperationKind.Invocation:
                    AnalyzeInvocation(context, (IInvocationOperation)context.Operation);
                    break;

                case OperationKind.VariableDeclaration:
                    AnalyzeVariableDeclaration(context, ((IVariableDeclarationOperation)context.Operation));
                    break;

                case OperationKind.TypeOf:
                    AnalyzeTypeof(context, ((ITypeOfOperation)context.Operation));
                    break;

                default:
                    throw new ArgumentException($"Unexpected {nameof(OperationKind)}: {context.Operation.Kind}");
            }

        }

        private static void AnalyzeMember(OperationAnalysisContext context, ISymbol symbol)
        {
            if ((object)symbol.ContainingAssembly == context.Compilation.Assembly)
            {
                // Skip all methods inside the same assembly - internal access is fine
                return;
            }

            var containingType = symbol.ContainingType;

            switch (symbol)
            {
                case IMethodSymbol _:
                case IFieldSymbol _:
                case IPropertySymbol _:
                case IEventSymbol _:
                    if (HasInternalAttribute(symbol))
                    {
                        ReportDiagnostic(symbol.Name == ".ctor" ? (object)containingType : $"{containingType}.{symbol.Name}");
                        return;
                    }
                    break;
            }

            if (IsTypeInternal(context, containingType))
            {
                ReportDiagnostic(containingType);
            }

            void ReportDiagnostic(object messageArg)
            {
                // For C# member access expressions, report a narrowed-down diagnostic, otherwise take the whole invocation.
                var syntax = context.Operation.Syntax switch
                {
                    InvocationExpressionSyntax invocationSyntax
                        when invocationSyntax.Expression is MemberAccessExpressionSyntax memberAccessSyntax
                        => memberAccessSyntax.Name,
                    MemberAccessExpressionSyntax memberAccessSyntax
                        => memberAccessSyntax.Name,
                    ObjectCreationExpressionSyntax objectCreationSyntax
                        => objectCreationSyntax.Type,
                    _
                        => context.Operation.Syntax
                };

                context.ReportDiagnostic(Diagnostic.Create(_descriptor, syntax.GetLocation(), messageArg));
            }
        }

        private static void AnalyzeInvocation(OperationAnalysisContext context, IInvocationOperation invocation)
        {
            // First check for any internal type parameters
            foreach (var a in invocation.TargetMethod.TypeArguments)
            {
                if (IsTypeInternal(context, a))
                {
                    context.ReportDiagnostic(Diagnostic.Create(_descriptor, context.Operation.Syntax.GetLocation(), a));
                }
            }

            // Then check the method being invoked
            AnalyzeMember(context, invocation.TargetMethod);
        }

        private static void AnalyzeVariableDeclaration(OperationAnalysisContext context, IVariableDeclarationOperation variableDeclaration)
        {
            foreach (var declarator in variableDeclaration.Declarators)
            {
                if (IsTypeInternal(context, declarator.Symbol.Type))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            _descriptor,
                            ((VariableDeclarationSyntax)context.Operation.Syntax).Type.GetLocation(),
                            declarator.Symbol.Type));
                    return;
                }
            }
        }

        private static void AnalyzeTypeof(OperationAnalysisContext context, ITypeOfOperation typeOf)
        {
            if (IsTypeInternal(context, typeOf.TypeOperand))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        _descriptor,
                        ((TypeOfExpressionSyntax)context.Operation.Syntax).Type.GetLocation(),
                        typeOf.TypeOperand));
            }
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            switch (context.Symbol)
            {
                case INamedTypeSymbol namedTypeSymbol:
                    AnalyzeNamedTypeSymbol(context, namedTypeSymbol);
                    break;

                default:
                    throw new ArgumentException($"Unexpected {nameof(ISymbol)}: {context.Symbol.GetType().Name}");
            }
        }

        private static void AnalyzeNamedTypeSymbol(SymbolAnalysisContext context, INamedTypeSymbol symbol)
        {
            if (symbol.BaseType is ISymbol baseSymbol
                && (object)baseSymbol.ContainingAssembly != context.Compilation.Assembly
                && (IsInInternalNamespace(baseSymbol) || HasInternalAttribute(baseSymbol)))
            {
                foreach (var declaringSyntax in symbol.DeclaringSyntaxReferences)
                {
                    var syntax = declaringSyntax.GetSyntax();
                    if (syntax is ClassDeclarationSyntax classDeclarationSyntax
                        && classDeclarationSyntax.BaseList?.Types.Count > 0)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(_descriptor, classDeclarationSyntax.BaseList.Types[0].GetLocation(), baseSymbol));
                    }
                    // context.ReportDiagnostic(Diagnostic.Create(_descriptor, location, baseSymbol));
                }
            }
        }

        private static bool IsTypeInternal(OperationAnalysisContext context, ISymbol symbol)
            => (object)symbol.ContainingAssembly != context.Compilation.Assembly
                && (IsInInternalNamespace(symbol) || HasInternalAttribute(symbol));

        private static bool HasInternalAttribute(ISymbol symbol)
            => symbol != null
                && symbol.GetAttributes().Any(a =>
                    a.AttributeClass.ToDisplayString() == "Microsoft.EntityFrameworkCore.Infrastructure.EntityFrameworkInternalAttribute");

        private static bool IsInInternalNamespace(ISymbol symbol)
        {
            if (symbol?.ContainingNamespace?.ToDisplayString() is string ns)
            {
                var i = ns.IndexOf("EntityFrameworkCore");

                return
                    i != -1 &&
                    (i == 0 || ns[i - 1] == '.') &&
                    i + EFLen < ns.Length && ns[i + EFLen] == '.' &&
                    ns.EndsWith(".Internal", StringComparison.Ordinal);
            }

            return false;
        }
    }
}
