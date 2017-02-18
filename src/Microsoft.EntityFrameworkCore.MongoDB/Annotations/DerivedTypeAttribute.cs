using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MongoDB.Bson.Serialization;

namespace Microsoft.EntityFrameworkCore.Annotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class DerivedTypeAttribute : Attribute, IEntityTypeAttribute, IBsonClassMapAttribute
    {
        private string _discriminator;

        public DerivedTypeAttribute([NotNull] Type derivedType)
        {
            if (derivedType == null)
            {
                throw new ArgumentNullException(nameof(derivedType));
            }
            DerivedType = derivedType;
        }

        public virtual Type DerivedType { get; }

        public virtual string Discriminator
        {
            get { return _discriminator ?? MongoDbUtilities.ToCamelCase(DerivedType.Name); }
            [param: NotNull] set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException(message: "Discriminator cannot be null, empty, or exclusively white-space.", paramName: nameof(value));
                }
                _discriminator = value;
            }
        }

        public virtual void Apply([NotNull] InternalEntityTypeBuilder entityTypeBuilder)
        {
            if (entityTypeBuilder == null)
            {
                throw new ArgumentNullException(nameof(entityTypeBuilder));
            }
            entityTypeBuilder.ModelBuilder
                .Entity(DerivedType, ConfigurationSource.DataAnnotation)
                .MongoDb(ConfigurationSource.DataAnnotation)
                .HasDiscriminator(Discriminator);
        }

        public virtual void Apply([NotNull] BsonClassMap classMap)
        {
            if (classMap == null)
            {
                throw new ArgumentNullException(nameof(classMap));
            }
            classMap.AddKnownType(DerivedType);
            classMap.SetDiscriminator(Discriminator);
        }
    }
}