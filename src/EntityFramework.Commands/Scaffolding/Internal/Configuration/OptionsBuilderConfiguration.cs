// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Scaffolding.Internal.Configuration
{
    public class OptionsBuilderConfiguration : IFluentApiConfiguration
    {
        public OptionsBuilderConfiguration([NotNull] ICollection<string> methodBodyLines)
        {
            Check.NotNull(methodBodyLines, nameof(methodBodyLines));

            FluentApiLines = methodBodyLines;
        }

        public virtual bool HasAttributeEquivalent { get; } = false;

        public virtual ICollection<string> FluentApiLines { get;[param: NotNull] set; }
    }
}
