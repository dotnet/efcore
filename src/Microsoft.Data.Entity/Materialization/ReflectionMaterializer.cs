// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Materialization
{
    public class ReflectionMaterializer : IMaterializer
    {
        private readonly ThreadSafeLazyRef<Func<object>> _activator;
        private readonly ThreadSafeLazyRef<Func<object, object>[]> _getters;
        private readonly ThreadSafeLazyRef<Action<object, object>[]> _setters;

        public ReflectionMaterializer([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            _activator
                = new ThreadSafeLazyRef<Func<object>>(() =>
                    {
                        // TODO: Shadow entities
                        var defaultCtor = entityType.Type.GetDeclaredConstructor(null);

                        if (defaultCtor == null)
                        {
                            throw new InvalidOperationException(
                                Strings.FormatNoDefaultCtor(entityType.Type));
                        }

                        return () => defaultCtor.Invoke(null);
                    });

            _getters
                = new ThreadSafeLazyRef<Func<object, object>[]>(() =>
                    {
                        var getters = new Func<object, object>[entityType.Properties.Count];

                        for (var i = 0; i < entityType.Properties.Count; i++)
                        {
                            var propertyInfo = entityType.Properties[i].PropertyInfo;

                            if (propertyInfo != null)
                            {
                                getters[i] = propertyInfo.GetValue;
                            }
                            else
                            {
                                // TODO: Shadow state
                                getters[i] = _ => null;
                            }
                        }

                        return getters;
                    });

            _setters
                = new ThreadSafeLazyRef<Action<object, object>[]>(() =>
                    {
                        var setters = new Action<object, object>[entityType.Properties.Count];

                        for (var i = 0; i < entityType.Properties.Count; i++)
                        {
                            var propertyInfo = entityType.Properties[i].PropertyInfo;

                            if (propertyInfo != null)
                            {
                                setters[i] = propertyInfo.SetValue;
                            }
                            else
                            {
                                // TODO: Shadow state
                                setters[i] = (_, __) => { };
                            }
                        }

                        return setters;
                    });
        }

        public object Materialize(object[] values)
        {
            var entity = _activator.Value();
            var setters = _setters.Value;

            for (var i = 0; i < setters.Length; i++)
            {
                // ReSharper disable once PossibleNullReferenceException
                setters[i](entity, values[i]);
            }

            return entity;
        }

        public object[] Shred(object entity)
        {
            var getters = _getters.Value;
            var values = new object[getters.Length];

            for (var i = 0; i < getters.Length; i++)
            {
                values[i] = getters[i](entity);
            }

            return values;
        }
    }
}
