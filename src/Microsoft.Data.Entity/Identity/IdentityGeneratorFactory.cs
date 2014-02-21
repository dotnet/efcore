// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Identity
{
    public abstract class IdentityGeneratorFactory
    {
        public abstract IIdentityGenerator Create([NotNull] IProperty property);
    }
}
