// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class DbSetInitializer
    {
        private readonly DbSetFinder _setFinder;
        private readonly ClrPropertySetterSource _setSetters;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected DbSetInitializer()
        {
        }

        public DbSetInitializer([NotNull] DbSetFinder setFinder, [NotNull] ClrPropertySetterSource setSetters)
        {
            Check.NotNull(setFinder, "setFinder");
            Check.NotNull(setSetters, "setSetters");

            _setFinder = setFinder;
            _setSetters = setSetters;
        }

        public virtual void InitializeSets([NotNull] DbContext context)
        {
            Check.NotNull(context, "context");

            foreach (var setInfo in _setFinder.FindSets(context).Where(p => p.HasSetter))
            {
                _setSetters
                    .GetAccessor(setInfo.ContextType, setInfo.Name)
                    .SetClrValue(context, context.Set(setInfo.EntityType));
            }
        }
    }
}
