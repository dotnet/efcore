// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.CodeGeneration
{
    public class CSharpModelCodeGenerator
    {
        private IModel _model;
        private ModelCodeGeneratorContext _codeGenerationContext;

        public CSharpModelCodeGenerator([NotNull]IModel model, [NotNull]ModelCodeGeneratorContext context)
        {
            _model = model;
            _codeGenerationContext = context;
        }

        public IModel Model
        {
            get { return _model; }
        }

        // Generate methods
        public virtual void GenerateClassFromModel(IndentedStringBuilder sb)
        {
            GenerateCommentHeader(sb);
            GenerateUsings(sb);
            GenerateNamespace(sb);
        }

        protected virtual void GenerateCommentHeader(IndentedStringBuilder sb)
        {
            _codeGenerationContext.GenerateCommentHeader(sb);
        }

        protected virtual void GenerateUsings(IndentedStringBuilder sb)
        {
            var namespaces = _codeGenerationContext.GetUsedNamespaces();
            foreach (var ns in namespaces)
            {
                sb.Append("using ");
                sb.Append(ns);
                sb.AppendLine(";");
            }

            if (namespaces.Any())
            {
                sb.AppendLine();
            }
        }

        protected virtual void GenerateNamespace(IndentedStringBuilder sb)
        {
            sb.Append("namespace ");
            sb.AppendLine(_codeGenerationContext.ClassNamespace);
            sb.AppendLine("{");
            using (sb.Indent())
            {
                _codeGenerationContext.GenerateClass(sb);
            }
            sb.AppendLine("}");
        }
    }
}
