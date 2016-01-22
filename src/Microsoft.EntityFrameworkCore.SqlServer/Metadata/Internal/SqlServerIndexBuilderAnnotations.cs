// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class SqlServerIndexBuilderAnnotations : SqlServerIndexAnnotations
    {
        public SqlServerIndexBuilderAnnotations(
            [NotNull] InternalIndexBuilder internalBuilder,
            ConfigurationSource configurationSource)
            : base(new RelationalAnnotationsBuilder(internalBuilder, configurationSource, SqlServerAnnotationNames.Prefix))
        {
        }

        public new virtual bool Name([CanBeNull] string value) => SetName(value);

        public virtual bool Clustered(bool value) => SetIsClustered(value);
    }
}
