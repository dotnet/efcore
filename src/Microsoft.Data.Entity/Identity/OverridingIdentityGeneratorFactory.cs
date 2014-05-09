// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Identity
{
    public class OverridingIdentityGeneratorFactory : IdentityGeneratorFactory
    {
        private readonly IdentityGeneratorFactory _overridingFactory;
        private readonly IdentityGeneratorFactory _defaultFactory;

        public OverridingIdentityGeneratorFactory(
            [NotNull] IdentityGeneratorFactory overridingFactory, [NotNull] IdentityGeneratorFactory defaultFactory)
        {
            Check.NotNull(overridingFactory, "overridingFactory");
            Check.NotNull(defaultFactory, "defaultFactory");

            _overridingFactory = overridingFactory;
            _defaultFactory = defaultFactory;
        }

        public override IIdentityGenerator Create(IProperty property)
        {
            return _overridingFactory.Create(property) ?? _defaultFactory.Create(property);
        }
    }
}
