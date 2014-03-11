// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class EntitySetInitializer
    {
        private readonly EntitySetFinder _setFinder;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected EntitySetInitializer()
        {
        }

        public EntitySetInitializer([NotNull] EntitySetFinder setFinder)
        {
            Check.NotNull(setFinder, "setFinder");

            _setFinder = setFinder;
        }

        public virtual void InitializeSets([NotNull] EntityContext context)
        {
            Check.NotNull(context, "context");

            // TODO: Consider caching and/or compiled model support for initializing, possibly by rewriting the
            // context EntitySet properties to include in-line initialization
            foreach (var setProperty in _setFinder.FindSets(context))
            {
                if (setProperty.SetMethod != null)
                {
                    setProperty.SetMethod.Invoke(context, new[] { Activator.CreateInstance(setProperty.PropertyType, context) });
                }
            }
        }
    }
}
