// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public class DbContextGeneratorModel : BaseGeneratorModel
    {
        public virtual IModel MetadataModel { get; [param: NotNull] set; }
        public virtual DbContextCodeGeneratorHelper Helper { get; [param: NotNull] set; }
    }
}
