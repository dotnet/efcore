// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class RelationalKeyBuilderAnnotations : RelationalKeyAnnotations
    {
        public RelationalKeyBuilderAnnotations(
            [NotNull] InternalKeyBuilder internalBuilder,
            ConfigurationSource configurationSource,
            [CanBeNull] string providerPrefix)
            : base(new RelationalAnnotationsBuilder(internalBuilder, configurationSource, providerPrefix))
        {
        }

        public new virtual bool Name([CanBeNull] string value) => SetName(value);
    }
}
