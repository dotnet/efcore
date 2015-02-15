// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ValueGeneration.Internal
{
    public class SingleValueGeneratorPool : IValueGeneratorPool
    {
        private readonly ValueGenerator _generator;

        public SingleValueGeneratorPool([NotNull] ValueGeneratorFactory factory, [NotNull] IProperty property)
        {
            _generator = factory.Create(property);
        }

        public virtual ValueGenerator GetGenerator() => _generator;
    }
}
