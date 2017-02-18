using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Annotations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MongoDatabaseAttribute : Attribute, IModelAttribute
    {
        public MongoDatabaseAttribute([NotNull] string database)
        {
            if (string.IsNullOrWhiteSpace(database))
            {
                throw new ArgumentException(message: "Database name cannot be null, empty, or exclusively white-space.", paramName: nameof(database));
            }
            Database = database;
        }

        public virtual string Database { get; }

        public virtual void Apply([NotNull] InternalModelBuilder modelBuilder)
            => Check.NotNull(modelBuilder, nameof(modelBuilder))
                .MongoDb(ConfigurationSource.Convention)
                .FromDatabase(Database);
    }
}