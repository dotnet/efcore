namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class MongoDbAnnotationNames
    {
        private const string Prefix = "MongoDb:";

        public const string CollectionName = Prefix + nameof(CollectionName);

        public const string CollectionSettings = Prefix + nameof(CollectionSettings);

        public const string ComplexTypes = Prefix + nameof(ComplexTypes);

        public const string Database = Prefix + nameof(Database);

        public const string Discriminator = Prefix + nameof(Discriminator);

        public const string Namespace = Prefix + nameof(Namespace);
    }
}