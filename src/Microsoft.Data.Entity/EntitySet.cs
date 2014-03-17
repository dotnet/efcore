// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public abstract class EntitySet : IQueryable
    {
        private readonly EntityContext _context;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected EntitySet()
        {
        }

        protected EntitySet([NotNull] EntityContext context)
        {
            Check.NotNull(context, "context");

            _context = context;
        }

        protected internal EntityContext Context
        {
            get { return _context; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            // TODO: Consider something better here
            return ((IEnumerable)this).GetEnumerator();
        }

        public abstract Expression Expression { get; }
        public abstract Type ElementType { get; }
        public abstract IQueryProvider Provider { get; }

        // TODO: Decide whether/how to implement non-generic API
        // TODO: Consider the role of EntitySet when entity is in shadow
    }
}
