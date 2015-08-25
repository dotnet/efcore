// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public class ReverseEngineeringConfiguration
    {
        public virtual string ConnectionString { get; [param: NotNull] set; }
        public virtual string ContextClassName { get;[param: CanBeNull] set; }
        public virtual string CustomTemplatePath { get; [param: NotNull] set; }
        public virtual string ProjectPath { get;[param: NotNull] set; }
        public virtual string ProjectRootNamespace { get;[param: NotNull] set; }
        public virtual string RelativeOutputPath { get;[param: CanBeNull] set; }

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

            if (RelativeOutputPath != null
                && (Path.IsPathRooted(RelativeOutputPath)
                    || !Path.GetFullPath(
                            Path.Combine(ProjectPath, RelativeOutputPath))
                        .StartsWith(Path.GetFullPath(ProjectPath))))
            {
                throw new ArgumentException(Strings.NotRelativePath(RelativeOutputPath, ProjectPath));
            }

            if (string.IsNullOrEmpty(ProjectRootNamespace))
            {
                throw new ArgumentException(Strings.RootNamespaceRequired);
            }
        }
    }
}
