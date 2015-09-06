// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.SqlServer.Metadata.Internal
{
    public class SqlServerModelBuilderAnnotations : SqlServerModelAnnotations
    {
        public SqlServerModelBuilderAnnotations(
            [NotNull] InternalModelBuilder internalBuilder,
            ConfigurationSource configurationSource)
            : base(new RelationalAnnotationsBuilder(internalBuilder, configurationSource, SqlServerAnnotationNames.Prefix))
        {
        }

        public new virtual bool HiLoSequenceName([CanBeNull] string value) => SetHiLoSequenceName(value);

        public new virtual bool HiLoSequenceSchema([CanBeNull] string value) => SetHiLoSequenceSchema(value);

        public new virtual bool HiLoSequencePoolSize(int? value) => SetHiLoSequencePoolSize(value);

        public new virtual bool IdentityStrategy(SqlServerIdentityStrategy? value) => SetIdentityStrategy(value);
    }
}
