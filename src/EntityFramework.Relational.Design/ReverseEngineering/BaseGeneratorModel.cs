// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public class BaseGeneratorModel
    {
        public virtual string ClassName { get; [param: CanBeNull] set; }
        public virtual string Namespace { get; [param: CanBeNull] set; }
        public virtual string ProviderAssembly { get; [param: NotNull] set; }
        public virtual string ConnectionString { get; [param: NotNull] set; }
        public virtual ReverseEngineeringGenerator Generator { get; [param: NotNull] set; }
    }
}
