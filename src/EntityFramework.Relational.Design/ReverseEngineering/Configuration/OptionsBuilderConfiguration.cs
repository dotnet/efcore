// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration
{
    public class OptionsBuilderConfiguration : IFluentApiConfiguration
    {
        public OptionsBuilderConfiguration([NotNull] string methodBody)
        {
            Check.NotEmpty(methodBody, nameof(methodBody));

            FluentApi = methodBody;
        }

        public virtual bool HasAttributeEquivalent { get; } = false;
        public virtual string For { get; }

        public virtual string FluentApi { get;[param: NotNull] set; }

    }
}
