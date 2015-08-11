// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public class ReverseEngineeringConfiguration
    {
        public virtual IDatabaseMetadataModelProvider Provider { get; [param: NotNull] set; }
        public virtual string ConnectionString { get; [param: NotNull] set; }
        public virtual string CustomTemplatePath { get; [param: NotNull] set; }
        public virtual string ProjectPath { get;[param: NotNull] set; }
        public virtual string RelativeOutputPath { get;[param: CanBeNull] set; }
        public virtual string Namespace { get; [param: NotNull] set; }
        public virtual string ContextClassName { get; [param: CanBeNull] set; }
    }
}
