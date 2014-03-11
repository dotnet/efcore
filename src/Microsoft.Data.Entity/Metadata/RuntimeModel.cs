// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class RuntimeModel : IModel
    {
        private readonly EntityKeyFactorySource _keyFactorySource;
        private readonly IModel _model;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected RuntimeModel()
        {
        }

        public RuntimeModel([NotNull] IModel model, [NotNull] EntityKeyFactorySource keyFactorySource)
        {
            Check.NotNull(model, "model");
            Check.NotNull(keyFactorySource, "keyFactorySource");

            _model = model;
            _keyFactorySource = keyFactorySource;
        }

        public virtual EntityKeyFactory GetKeyFactory([NotNull] IReadOnlyList<IProperty> keyProperties)
        {
            Check.NotNull(keyProperties, "keyProperties");

            return _keyFactorySource.GetKeyFactory(keyProperties);
        }

        public virtual string this[string annotationName]
        {
            get { return _model[annotationName]; }
        }

        public virtual IReadOnlyList<IAnnotation> Annotations
        {
            get { return _model.Annotations; }
        }

        public virtual IEntityType TryGetEntityType(Type type)
        {
            return _model.TryGetEntityType(type);
        }

        public virtual IEntityType GetEntityType(Type type)
        {
            return _model.GetEntityType(type);
        }

        public virtual IReadOnlyList<IEntityType> EntityTypes
        {
            get { return _model.EntityTypes; }
        }

        public virtual IEntityType TryGetEntityType(string name)
        {
            return _model.TryGetEntityType(name);
        }

        public virtual IEntityType GetEntityType(string name)
        {
            return _model.GetEntityType(name);
        }
    }
}
