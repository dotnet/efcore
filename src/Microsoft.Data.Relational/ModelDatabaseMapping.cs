// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Relational.Model;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational
{
    public class ModelDatabaseMapping
    {
        private readonly IModel _model;
        private readonly Database _database;

        // TODO: Consider adding base interface for database objects.

        private readonly Dictionary<IMetadata, object> _modelToDatabaseMap 
            = new Dictionary<IMetadata, object>();
        private readonly Dictionary<object, IMetadata> _databaseToModelMap 
            = new Dictionary<object, IMetadata>();

        public ModelDatabaseMapping([NotNull] IModel model, [NotNull] Database database)
        {
            Check.NotNull(model, "model");
            Check.NotNull(database, "database");

            _model = model;
            _database = database;
        }

        public virtual IModel Model
        {
            get { return _model; }
        }

        public virtual Database Database
        {
            get { return _database; }
        }

        public virtual void Map([NotNull] IMetadata modelObject, [NotNull] object databaseObject)
        {
            Check.NotNull(modelObject, "modelObject");
            Check.NotNull(databaseObject, "databaseObject");

            // TODO: The one to one mapping will change in the future.

            _modelToDatabaseMap.Add(modelObject, databaseObject);
            _databaseToModelMap.Add(databaseObject, modelObject);
        }

        public virtual T GetDatabaseObject<T>([NotNull] IMetadata modelObject)
        {
            Check.NotNull(modelObject, "modelObject");

            return (T)_modelToDatabaseMap[modelObject];
        }

        public virtual T GetModelObject<T>([NotNull] object databaseObject)
            where T : IMetadata
        {
            Check.NotNull(databaseObject, "databaseObject");

            return (T)_databaseToModelMap[databaseObject];
        }
    }
}
