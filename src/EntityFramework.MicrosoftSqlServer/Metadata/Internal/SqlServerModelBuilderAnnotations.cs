// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Internal
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

        public new virtual bool ValueGenerationStrategy(SqlServerValueGenerationStrategy? value) => SetValueGenerationStrategy(value);
    }
}
