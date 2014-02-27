// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class EntitySetInitializer
    {
        private readonly EntitySetFinder _setFinder;

        // Intended only for creation of test doubles
        internal EntitySetInitializer()
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
