// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.Relational.Metadata;

namespace Microsoft.Data.Entity.SqlServer.Metadata.ModelConventions
{
    public class SqlServerValueGenerationStrategyConvention : IModelConvention
    {
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            modelBuilder.Annotation(
                SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGeneration,
                SqlServerValueGenerationStrategy.Sequence.ToString(),
                ConfigurationSource.Convention);

            var sequence = new Sequence(Sequence.DefaultName) { Model = modelBuilder.Metadata };
            modelBuilder.Annotation(
                SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.Sequence + sequence.Schema + "." + sequence.Name,
                sequence.Serialize(),
                ConfigurationSource.Convention
                );

            modelBuilder.Annotation(
                SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.DefaultSequenceName,
                sequence.Name,
                ConfigurationSource.Convention);

            modelBuilder.Annotation(
                SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.DefaultSequenceSchema,
                sequence.Schema,
                ConfigurationSource.Convention);

            return modelBuilder;
        }
    }
}
