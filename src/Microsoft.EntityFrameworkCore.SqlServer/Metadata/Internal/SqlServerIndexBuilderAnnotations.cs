// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class SqlServerIndexBuilderAnnotations : SqlServerIndexAnnotations
    {
        public SqlServerIndexBuilderAnnotations(
            [NotNull] InternalIndexBuilder internalBuilder,
            ConfigurationSource configurationSource)
            : base(new RelationalAnnotationsBuilder(internalBuilder, configurationSource))
        {
        }

#pragma warning disable 109
        public new virtual bool Name([CanBeNull] string value) => SetName(value);

        public new virtual bool IsClustered(bool value) => SetIsClustered(value);
#pragma warning restore 109
    }
}
