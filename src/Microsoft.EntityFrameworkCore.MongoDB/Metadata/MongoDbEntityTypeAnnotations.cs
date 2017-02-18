using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Driver;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class MongoDbEntityTypeAnnotations
    {
        public MongoDbEntityTypeAnnotations([NotNull] IEntityType entityType)
        {
            EntityType = Check.NotNull(entityType, nameof(entityType));
        }

        public virtual IEntityType EntityType { get; }

        protected virtual IEntityType RootEntityType =>
            EntityType.BaseType == null
                ? EntityType
                : EntityType.RootType();

        public virtual bool AssignIdOnInsert
        {
            get
            {
                return CollectionSettings?.AssignIdOnInsert ?? false;
            }
            set
            {
                GetOrCreateCollectionSettings().AssignIdOnInsert = value;
            }
        }

        public virtual string CollectionName
        {
            get
            {
                return RootEntityType.GetAnnotation<string>(MongoDbAnnotationNames.CollectionName)
                       ?? MongoDbUtilities.Pluralize(MongoDbUtilities.ToCamelCase(EntityType.ClrType.Name));
            }
            [param: NotNull]
            set
            {
                RootEntityType.SetAnnotation(MongoDbAnnotationNames.CollectionName, Check.NotEmpty(value, nameof(CollectionName)));
            }
        }

        public virtual MongoCollectionSettings CollectionSettings
        {
            get
            {
                return RootEntityType.GetAnnotation<MongoCollectionSettings>(MongoDbAnnotationNames.CollectionSettings);
            }
            [param: NotNull]
            set
            {
                RootEntityType.SetAnnotation(MongoDbAnnotationNames.CollectionSettings, Check.NotNull(value, nameof(CollectionSettings)));
            }
        }

        public virtual string Discriminator
        {
            get
            {
                return EntityType.GetAnnotation<string>(MongoDbAnnotationNames.Discriminator)
                       ?? EntityType.ClrType.Name;
            }
            [param: NotNull]
            set
            {
                EntityType.SetAnnotation(MongoDbAnnotationNames.Discriminator, Check.NotEmpty(value, nameof(Discriminator)));
            }
        }

        private MongoCollectionSettings GetOrCreateCollectionSettings()
            => CollectionSettings ?? (CollectionSettings = new MongoCollectionSettings());
    }
}