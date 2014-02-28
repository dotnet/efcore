// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class EntityEqualityComparer : IEqualityComparer<object>
    {
        private readonly IModel _model;

        public EntityEqualityComparer([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            _model = model;
        }

        bool IEqualityComparer<object>.Equals(object x, object y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x == null
                || y == null
                || x.GetType() != y.GetType())
            {
                return false;
            }

            var entityType = _model.TryGetEntityType(x.GetType());

            return entityType != null
                   && entityType.Key.All(property => Equals(property.GetValue(x), property.GetValue(y)));
        }

        int IEqualityComparer<object>.GetHashCode(object obj)
        {
            if (obj == null)
            {
                return 0;
            }

            var entityType = _model.TryGetEntityType(obj.GetType());

            if (entityType == null)
            {
                return 0;
            }

            unchecked
            {
                var hashCode = obj.GetType().GetHashCode();

                return entityType.Key.Select(property => property.GetValue(obj))
                    .Aggregate(hashCode, (t, v) => (t * 397) ^ (v != null ? v.GetHashCode() : 0));
            }
        }
    }
}
