// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.EntityFrameworkCore.SourceGenerators
{
    [Generator]
    public class ModelSourceGenerator : ISourceGenerator
    {
        public void Execute(SourceGeneratorContext context)
        {
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver)
                || receiver.ContextCandidateClasses.Count == 0)
            {
                return;
            }

            var compilation = context.Compilation;
            var attributeSymbol = compilation.GetTypeByMetadataName("Microsoft.EntityFrameworkCore.CompileTimeContextAttribute");

            foreach (var contextType in receiver.ContextCandidateClasses)
            {
                var sematicModel = compilation.GetSemanticModel(contextType.SyntaxTree);
                var contextTypeSymbol = (ITypeSymbol)sematicModel.GetDeclaredSymbol(contextType);

                var compileTimeContextAttributeData = contextTypeSymbol.GetAttributes()
                    .FirstOrDefault(ad => ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default));
                if (compileTimeContextAttributeData == null)
                {
                    continue;
                }

                var args = compileTimeContextAttributeData.ConstructorArguments;
                var typeArgument = compileTimeContextAttributeData.ConstructorArguments[0].Value as INamedTypeSymbol;

                var compileTimeAssembly = Assembly.Load(typeArgument.ContainingAssembly.Identity.GetDisplayName()); // This throws
                var compileTimeContextType = compileTimeAssembly.GetType(typeArgument.ToString());

                var compileTimeContext = Activator.CreateInstance(compileTimeContextType);
                var modelProperty = compileTimeContextType.GetProperty("Model");
                var model = modelProperty.GetValue(compileTimeContext);

                // This is what we want to generate dynamically using the model
                var source = new StringBuilder($@"
namespace {contextTypeSymbol.ContainingNamespace.ToDisplayString()}
{{
    public partial class {contextTypeSymbol.Name}
    {{
        private static readonly IModel CompiledModel = CreateCompiledModel();

        private static IModel CreateCompiledModel()
        {{
            var modelBuilder = new ModelBuilder();
            modelBuilder.Entity<MyEntity>(eb =>
            {{
                eb.Property<int>(""Id"");
                eb.Property<int>(""ShadowProp"");
            }});

            return modelBuilder.Model;
        }}

        public override IModel GetCompiledModel()
        {{
            return CompiledModel;
        }}
    }}
}}
");

                context.AddSource($"{contextTypeSymbol.Name}_model.cs", SourceText.From(source.ToString(), Encoding.UTF8));
            }
        }

        public void Initialize(InitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        /// <summary>
        ///     Created on demand before each generation pass
        /// </summary>
        class SyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> ContextCandidateClasses { get; } = new List<ClassDeclarationSyntax>();

            /// <summary>
            ///     Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
            /// </summary>
            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                // Any class that derives from RuntimeDbContext is a candidate for model generation
                if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax
                    && (classDeclarationSyntax.BaseList?.Types
                        .Any(t => t.Type is SimpleNameSyntax simpleName && simpleName.Identifier.ValueText == "RuntimeDbContext") ?? false))
                {
                    ContextCandidateClasses.Add(classDeclarationSyntax);
                }
            }
        }
    }
}
