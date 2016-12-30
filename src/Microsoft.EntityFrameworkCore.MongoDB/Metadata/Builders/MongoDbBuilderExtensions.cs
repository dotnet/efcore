using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public static class MongoDbBuilderExtensions
    {
        public static MongoDbDocumentBuilder MongoDb(
                [NotNull] this InternalEntityTypeBuilder internalEntityTypeBuilder,
                ConfigurationSource configurationSource)
            => new MongoDbDocumentBuilder(
                Check.NotNull(internalEntityTypeBuilder, nameof(internalEntityTypeBuilder)),
                configurationSource);

        public static MongoDbModelBuilder MongoDb(
                [NotNull] this InternalModelBuilder internalModelBuilder,
                ConfigurationSource configurationSource)
            => new MongoDbModelBuilder(
                Check.NotNull(internalModelBuilder, nameof(internalModelBuilder)),
                configurationSource);
    }
}