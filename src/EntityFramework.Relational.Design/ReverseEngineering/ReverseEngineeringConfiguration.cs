// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Data.Entity.Relational.Design.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public class ReverseEngineeringConfiguration
    {
        public virtual string ConnectionString { get; [param: NotNull] set; }
        public virtual string ContextClassName { get;[param: CanBeNull] set; }
        public virtual string CustomTemplatePath { get; [param: NotNull] set; }
        public virtual string ProjectPath { get;[param: NotNull] set; }
        public virtual string ProjectRootNamespace { get;[param: NotNull] set; }
        public virtual string OutputPath { get;[param: CanBeNull] set; }
        public virtual TableSelectionSet TableSelectionSet { get;[param: CanBeNull] set; }
        public virtual bool UseFluentApiOnly { get; set; }

        public virtual void CheckValidity()
        {
            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new ArgumentException(Strings.ConnectionStringRequired);
            }

            if (string.IsNullOrEmpty(ProjectPath))
            {
                throw new ArgumentException(Strings.ProjectPathRequired);
            }

            if (!string.IsNullOrWhiteSpace(ContextClassName)
                && (!SyntaxFacts.IsValidIdentifier(ContextClassName)
                    || CSharpUtilities.Instance.IsCSharpKeyword(ContextClassName)))
            {
                throw new ArgumentException(
                    Strings.ContextClassNotValidCSharpIdentifier(ContextClassName));
            }

            if (string.IsNullOrEmpty(ProjectRootNamespace))
            {
                throw new ArgumentException(Strings.RootNamespaceRequired);
            }
        }
    }
}
