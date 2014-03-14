// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class EntitySetInitializer
    {
        private readonly EntitySetFinder _setFinder;
        private readonly ClrPropertySetterSource _entitySetSetters;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected EntitySetInitializer()
        {
        }

        public EntitySetInitializer([NotNull] EntitySetFinder setFinder, [NotNull] ClrPropertySetterSource entitySetSetters)
        {
            Check.NotNull(setFinder, "setFinder");
            Check.NotNull(entitySetSetters, "entitySetSetters");

            _setFinder = setFinder;
            _entitySetSetters = entitySetSetters;
        }

        public virtual void InitializeSets([NotNull] EntityContext context)
        {
            Check.NotNull(context, "context");

            // TODO: Consider caching and/or compiled model support for initializing, possibly by rewriting the
            // context EntitySet properties to include in-line initialization
            foreach (var setProperty in _setFinder.FindSets(context).Where(s => s.SetMethod != null))
            {
                _entitySetSetters.GetAccessor(setProperty.DeclaringType, setProperty.Name)
                    .SetClrValue(context, Activator.CreateInstance(setProperty.PropertyType, context));

            }
        }
    }
}
