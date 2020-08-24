// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class AbstractInternalUsageDiagnosticAnalyzer : DiagnosticAnalyzer
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

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(_descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterOperationAction(
                AnalyzeNode,
                OperationKind.FieldReference,
                OperationKind.PropertyReference,
                OperationKind.MethodReference,
                OperationKind.EventReference,
                OperationKind.Invocation,
                OperationKind.ObjectCreation,
                OperationKind.VariableDeclaration,
                OperationKind.TypeOf);

            context.RegisterSymbolAction(
                AnalyzeSymbol,
                SymbolKind.NamedType,
                SymbolKind.Method,
                SymbolKind.Property,
                SymbolKind.Field,
                SymbolKind.Event);
        }

        protected abstract SyntaxNodeOrToken GetLocationForDiagnostic(SyntaxNode syntax);
        protected abstract SyntaxNodeOrToken GetLocationForBaseTypeDiagnostic(SyntaxNode syntax);
        protected abstract SyntaxNodeOrToken GetLocationForTypeDiagnostic(SyntaxNode syntax);

        /// <summary>
        ///     Given a syntax node, pattern matches some known types and returns a narrowed-down node for the type syntax which
        ///     should be reported in diagnostics.
        /// </summary>
        protected abstract SyntaxNodeOrToken NarrowDownSyntax(SyntaxNode syntax);

        private void AnalyzeNode(OperationAnalysisContext context)
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

        private void AnalyzeMember(OperationAnalysisContext context, ISymbol symbol)
        {
            // ReSharper disable once RedundantCast
            if ((object)symbol.ContainingAssembly == context.Compilation.Assembly)
            {
                // Skip all methods inside the same assembly - internal access is fine
                return;
            }

            var containingType = symbol.ContainingType;

            if (HasInternalAttribute(symbol))
            {
                ReportDiagnostic(context, symbol.Name == ".ctor" ? (object)containingType : $"{containingType}.{symbol.Name}");
                return;
            }

            if (IsInternal(context, containingType))
            {
                ReportDiagnostic(context, containingType);
            }
        }

        private void AnalyzeInvocation(OperationAnalysisContext context, IInvocationOperation invocation)
        {
            // First check for any internal type parameters
            foreach (var a in invocation.TargetMethod.TypeArguments)
            {
                if (IsInternal(context, a))
                {
                    context.ReportDiagnostic(Diagnostic.Create(_descriptor, context.Operation.Syntax.GetLocation(), a));
                }
            }

            // Then check the method being invoked
            AnalyzeMember(context, invocation.TargetMethod);
        }

        private void AnalyzeVariableDeclaration(OperationAnalysisContext context, IVariableDeclarationOperation variableDeclaration)
        {
            foreach (var declarator in variableDeclaration.Declarators)
            {
                if (IsInternal(context, declarator.Symbol.Type))
                {
                    var syntax = GetLocationForDiagnostic(context.Operation.Syntax);
                    context.ReportDiagnostic(Diagnostic.Create(_descriptor, syntax.GetLocation(), declarator.Symbol.Type));
                    return;
                }
            }
        }

        private void AnalyzeTypeof(OperationAnalysisContext context, ITypeOfOperation typeOf)
        {
            if (IsInternal(context, typeOf.TypeOperand))
            {
                ReportDiagnostic(context, typeOf.TypeOperand);
            }
        }

        private void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            switch (context.Symbol)
            {
                case INamedTypeSymbol symbol:
                    AnalyzeNamedTypeSymbol(context, symbol);
                    break;

                case IMethodSymbol symbol:
                    AnalyzeMethodTypeSymbol(context, symbol);
                    break;

                case IFieldSymbol symbol:
                    AnalyzeMemberDeclarationTypeSymbol(context, symbol, symbol.Type);
                    break;

                case IPropertySymbol symbol:
                    AnalyzeMemberDeclarationTypeSymbol(context, symbol, symbol.Type);
                    break;

                case IEventSymbol symbol:
                    AnalyzeMemberDeclarationTypeSymbol(context, symbol, symbol.Type);
                    break;

                default:
                    throw new ArgumentException($"Unexpected {nameof(ISymbol)}: {context.Symbol.GetType().Name}");
            }
        }

        private void AnalyzeNamedTypeSymbol(SymbolAnalysisContext context, INamedTypeSymbol symbol)
        {
            if (symbol.BaseType is ITypeSymbol baseSymbol
                && IsInternal(context, baseSymbol))
            {
                foreach (var declaringSyntax in symbol.DeclaringSyntaxReferences)
                {
                    var syntax = GetLocationForBaseTypeDiagnostic(declaringSyntax.GetSyntax());
                    context.ReportDiagnostic(Diagnostic.Create(_descriptor, syntax.GetLocation(), baseSymbol));
                }
            }

            foreach (var iface in symbol.Interfaces.Where(i => IsInternal(context, i)))
            {
                foreach (var declaringSyntax in symbol.DeclaringSyntaxReferences)
                {
                    var syntax = GetLocationForDiagnostic(declaringSyntax.GetSyntax());
                    context.ReportDiagnostic(Diagnostic.Create(_descriptor, syntax.GetLocation(), iface));
                }
            }
        }

        private void AnalyzeMethodTypeSymbol(SymbolAnalysisContext context, IMethodSymbol symbol)
        {
            if (symbol.MethodKind == MethodKind.PropertyGet
                || symbol.MethodKind == MethodKind.PropertySet)
            {
                // Property getters/setters are handled via IPropertySymbol
                return;
            }

            if (IsInternal(context, symbol.ReturnType))
            {
                foreach (var declaringSyntax in symbol.DeclaringSyntaxReferences)
                {
                    var syntax = GetLocationForTypeDiagnostic(declaringSyntax.GetSyntax());
                    context.ReportDiagnostic(Diagnostic.Create(_descriptor, syntax.GetLocation(), symbol.ReturnType));
                }
            }

            foreach (var paramSymbol in symbol.Parameters.Where(ps => IsInternal(context, ps.Type)))
            {
                foreach (var declaringSyntax in paramSymbol.DeclaringSyntaxReferences)
                {
                    var syntax = GetLocationForTypeDiagnostic(declaringSyntax.GetSyntax());
                    context.ReportDiagnostic(Diagnostic.Create(_descriptor, syntax.GetLocation(), paramSymbol.Type));
                }
            }
        }

        private void AnalyzeMemberDeclarationTypeSymbol(
            SymbolAnalysisContext context,
            ISymbol declarationSymbol,
            ITypeSymbol typeSymbol)
        {
            if (IsInternal(context, typeSymbol))
            {
                foreach (var declaringSyntax in declarationSymbol.DeclaringSyntaxReferences)
                {
                    ReportDiagnostic(context, declaringSyntax.GetSyntax(), typeSymbol);
                }
            }
        }

        private void ReportDiagnostic(OperationAnalysisContext context, object messageArg)
            => context.ReportDiagnostic(
                Diagnostic.Create(_descriptor, NarrowDownSyntax(context.Operation.Syntax).GetLocation(), messageArg));

        private void ReportDiagnostic(SymbolAnalysisContext context, SyntaxNode syntax, object messageArg)
            => context.ReportDiagnostic(Diagnostic.Create(_descriptor, NarrowDownSyntax(syntax).GetLocation(), messageArg));

        private static bool IsInternal(SymbolAnalysisContext context, ITypeSymbol symbol)
            // ReSharper disable once RedundantCast
            => (object)symbol.ContainingAssembly != context.Compilation.Assembly
                && (IsInInternalNamespace(symbol) || HasInternalAttribute(symbol));

        private static bool IsInternal(OperationAnalysisContext context, ITypeSymbol symbol)
            // ReSharper disable once RedundantCast
            => (object)symbol.ContainingAssembly != context.Compilation.Assembly
                && (IsInInternalNamespace(symbol) || HasInternalAttribute(symbol));

        private static bool HasInternalAttribute(ISymbol symbol)
            => symbol != null
                && symbol.GetAttributes().Any(
                    a =>
                        a.AttributeClass.ToDisplayString()
                        == "Microsoft.EntityFrameworkCore.Infrastructure.EntityFrameworkInternalAttribute");

        private static bool IsInInternalNamespace(ISymbol symbol)
        {
            if (symbol?.ContainingNamespace?.ToDisplayString() is string ns)
            {
                var i = ns.IndexOf("EntityFrameworkCore");

                return
                    i != -1
                    && (i == 0 || ns[i - 1] == '.')
                    && i + EFLen < ns.Length
                    && ns[i + EFLen] == '.'
                    && ns.EndsWith(".Internal", StringComparison.Ordinal);
            }

            return false;
        }
    }
}
