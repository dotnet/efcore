using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class MongoDbModelAnnotations
    {
        public MongoDbModelAnnotations([NotNull] IModel model)
        {
            Model = Check.NotNull(model, nameof(model));
        }

        public virtual IModel Model { get; }

        protected virtual MongoDbEntityTypeAnnotations GetAnnotations([NotNull] IEntityType entityType)
            => new MongoDbEntityTypeAnnotations(Check.NotNull(entityType, nameof(entityType)));

        public virtual IList<IEntityType> ComplexTypes
        {
            get
            {
                var complexTypes = Model.GetAnnotation<IList<IEntityType>>(MongoDbAnnotationNames.ComplexTypes);
                if (complexTypes == null)
                {
                    Model.SetAnnotation(MongoDbAnnotationNames.ComplexTypes, complexTypes = new List<IEntityType>());
                }
                return complexTypes;
            }
        }

        public virtual string Database
        {
            get { return Model.GetAnnotation<string>(MongoDbAnnotationNames.Database); }
            [param: NotNull] set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException(message: "Database name is not null, empty, or exclusively white-space.", paramName: nameof(value));
                }
                Model.SetAnnotation(MongoDbAnnotationNames.Database, value);
            }
        }
    }
}