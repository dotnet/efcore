// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class ReverseEngineeringConfiguration
    {
        public virtual string ConnectionString { get; [param: NotNull] set; }
        public virtual string ContextClassName { get; [param: CanBeNull] set; }
        public virtual string CustomTemplatePath { get; [param: NotNull] set; }
        public virtual string ProjectPath { get; [param: NotNull] set; }
        public virtual string ProjectRootNamespace { get; [param: NotNull] set; }
        public virtual string OutputPath { get; [param: CanBeNull] set; }
        public virtual TableSelectionSet TableSelectionSet { get; [param: CanBeNull] set; }
        public virtual bool UseFluentApiOnly { get; set; }
        public virtual bool OverwriteFiles { get; set; }

        public virtual void CheckValidity()
        {
            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new ArgumentException(ToolsCoreStrings.ConnectionStringRequired);
            }

            if (string.IsNullOrEmpty(ProjectPath))
            {
                throw new ArgumentException(ToolsCoreStrings.ProjectPathRequired);
            }

            if (!string.IsNullOrWhiteSpace(ContextClassName)
                && (!CSharpUtilities.Instance.IsValidIdentifier(ContextClassName)
                    || CSharpUtilities.Instance.IsCSharpKeyword(ContextClassName)))
            {
                throw new ArgumentException(
                    ToolsCoreStrings.ContextClassNotValidCSharpIdentifier(ContextClassName));
            }

            if (string.IsNullOrEmpty(ProjectRootNamespace))
            {
                throw new ArgumentException(ToolsCoreStrings.RootNamespaceRequired);
            }
        }
    }
}
