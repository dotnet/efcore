using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    public class MongoDbModelBuilder
    {
        public MongoDbModelBuilder([NotNull] InternalModelBuilder internalModelBuilder,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(internalModelBuilder, nameof(internalModelBuilder));
            if (!Enum.IsDefined(typeof(ConfigurationSource), configurationSource))
            {
                throw new ArgumentOutOfRangeException(nameof(configurationSource),
                    $"{configurationSource} is not a valid {nameof(Microsoft.EntityFrameworkCore.Metadata.Internal.ConfigurationSource)} value.");
            }
            InternalModelBuilder = internalModelBuilder;
            ConfigurationSource = configurationSource;
            MongoDbModelAnnotations = new MongoDbModelAnnotations(internalModelBuilder.Metadata);
        }

        public virtual InternalModelBuilder InternalModelBuilder { get; }

        public virtual ConfigurationSource ConfigurationSource { get; }

        public virtual Model Model
            => InternalModelBuilder.Metadata;

        public virtual MongoDbModelAnnotations MongoDbModelAnnotations { get; }

        public virtual string DatabaseName
        {
            get { return MongoDbModelAnnotations.Database; }
            [param: NotNull] set { FromDatabase(value); }
        }

        public virtual MongoDbModelBuilder FromDatabase([NotNull] string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new ArgumentException(message: "Database name cannot be null, empty, or exclusively white-space.", paramName: nameof(databaseName));
            }
            MongoDbModelAnnotations.Database = databaseName;
            return this;
        }
    }
}