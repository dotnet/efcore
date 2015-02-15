// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ValueGeneration.Internal
{
    public class SingleValueGeneratorPool : IValueGeneratorPool
    {
        private readonly IValueGenerator _generator;

        public SingleValueGeneratorPool([NotNull] IValueGeneratorFactory factory, [NotNull] IProperty property)
        {
            _generator = factory.Create(property);
        }

        public virtual IValueGenerator GetGenerator() => _generator;
    }
}
