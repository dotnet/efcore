using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Driver;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    public class MongoDbDocumentBuilder
    {
        public MongoDbDocumentBuilder([NotNull] InternalEntityTypeBuilder internalEntityTypeBuilder,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(internalEntityTypeBuilder, nameof(internalEntityTypeBuilder));
            if (!Enum.IsDefined(typeof(ConfigurationSource), configurationSource))
            {
                throw new ArgumentOutOfRangeException(nameof(configurationSource),
                    $"{configurationSource} is not a valid {nameof(Microsoft.EntityFrameworkCore.Metadata.Internal.ConfigurationSource)} value.");
            }
            InternalEntityTypeBuilder = internalEntityTypeBuilder;
            ConfigurationSource = configurationSource;
            MongoDbEntityTypeAnnotations = new MongoDbEntityTypeAnnotations(internalEntityTypeBuilder.Metadata);
        }

        public virtual ConfigurationSource ConfigurationSource { get; }

        public virtual EntityType EntityType
            => InternalEntityTypeBuilder.Metadata;

        public virtual InternalEntityTypeBuilder InternalEntityTypeBuilder { get; }

        public virtual MongoDbEntityTypeAnnotations MongoDbEntityTypeAnnotations { get; }

        public virtual string CollectionName
        {
            get { return MongoDbEntityTypeAnnotations.CollectionName; }
            [param: NotNull] set { FromCollection(value); }
        }

        public virtual string Discriminator
        {
            get { return MongoDbEntityTypeAnnotations.Discriminator; }
            [param: NotNull] set { HasDiscriminator(value); }
        }

        public virtual bool IdAssignedOnInsert
        {
            get { return MongoDbEntityTypeAnnotations.AssignIdOnInsert; }
            set { AssignIdOnInsert(value); }
        }

        public virtual MongoDbDocumentBuilder FromCollection([NotNull] string collectionName)
        {
            if (string.IsNullOrWhiteSpace(collectionName))
            {
                throw new ArgumentException(message: "Collection name cannot be null, empty, or exclusively white-space.", paramName: nameof(collectionName));
            }
            MongoDbEntityTypeAnnotations.CollectionName = collectionName;
            return this;
        }

        public virtual MongoDbDocumentBuilder HasDiscriminator([NotNull] string discriminator)
        {
            if (string.IsNullOrWhiteSpace(discriminator))
            {
                throw new ArgumentException(message: "Discriminator cannot be null, empty, or exclusively white-space.", paramName: nameof(discriminator));
            }
            MongoDbEntityTypeAnnotations.Discriminator = discriminator;
            return this;
        }

        public virtual MongoDbDocumentBuilder AssignIdOnInsert(bool assignIdOnInsert)
        {
            MongoDbEntityTypeAnnotations.AssignIdOnInsert = assignIdOnInsert;
            return this;
        }
    }
}