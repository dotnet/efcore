// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Data.Entity.Internal;

namespace Microsoft.Data.Entity.Scaffolding.Internal
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
                throw new ArgumentException(RelationalDesignStrings.ConnectionStringRequired);
            }

            if (string.IsNullOrEmpty(ProjectPath))
            {
                throw new ArgumentException(RelationalDesignStrings.ProjectPathRequired);
            }

            if (!string.IsNullOrWhiteSpace(ContextClassName)
                && (!SyntaxFacts.IsValidIdentifier(ContextClassName)
                    || CSharpUtilities.Instance.IsCSharpKeyword(ContextClassName)))
            {
                throw new ArgumentException(
                    RelationalDesignStrings.ContextClassNotValidCSharpIdentifier(ContextClassName));
            }

            if (string.IsNullOrEmpty(ProjectRootNamespace))
            {
                throw new ArgumentException(RelationalDesignStrings.RootNamespaceRequired);
            }
        }
    }
}
