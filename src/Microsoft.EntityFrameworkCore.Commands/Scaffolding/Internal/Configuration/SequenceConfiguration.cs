// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal.Configuration
{
    public class SequenceConfiguration
    {
        public virtual string NameIdentifier { get; [param: NotNull] set; }

        public virtual string SchemaNameIdentifier { get; [param: NotNull] set; }
        public virtual string TypeIdentifier { get; [param: NotNull] set; }
        public virtual List<FluentApiConfiguration> FluentApiConfigurations { get; } = new List<FluentApiConfiguration>();
    }
}
