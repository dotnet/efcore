// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;

namespace Microsoft.Data.Entity.Query
{
    public class EntityQueryable<TResult>
        : QueryableBase<TResult>, IAsyncEnumerable<TResult>, IAnnotatable
    {
        private readonly EntityQueryProvider _provider;
        private readonly Annotatable _annotatable;

        public EntityQueryable([NotNull] EntityQueryProvider provider)
            : base(Check.NotNull(provider, nameof(provider)))
        {
            _provider = provider;
            _annotatable = new Annotatable();
        }

        public EntityQueryable([NotNull] EntityQueryProvider provider, [NotNull] Expression expression)
            : base(
                Check.NotNull(provider, nameof(provider)),
                Check.NotNull(expression, nameof(expression)))
        {
            _provider = provider;
            _annotatable = new Annotatable();
        }

        public virtual EntityQueryable<TResult> Clone()
        {
            var clone = new EntityQueryable<TResult>(_provider);

            foreach (var annotation in _annotatable.Annotations)
            {
                clone.AddAnnotation(annotation.Name, annotation.Value);
            }

            return clone;
        }

        IAsyncEnumerator<TResult> IAsyncEnumerable<TResult>.GetEnumerator()
        {
            return ((IAsyncQueryProvider)Provider).ExecuteAsync<TResult>(Expression).GetEnumerator();
        }

        public virtual Annotation AddAnnotation([NotNull] string annotationName, [NotNull] string value) => _annotatable.AddAnnotation(annotationName, value);

        public virtual string this[[NotNull]string annotationName] => _annotatable[annotationName];

        public virtual IEnumerable<IAnnotation> Annotations => _annotatable.Annotations;

        public virtual Annotation GetAnnotation([NotNull]string annotationName) => _annotatable.GetAnnotation(annotationName);

        public override string ToString()
        {
            return _annotatable.Annotations.Count() == 0
                ? base.ToString()
                : string.Format("{0} ({1})",
                    base.ToString(),
                    string.Join(", ", _annotatable.Annotations.Select(annotation =>
                        string.Format("{0} = {1}", annotation.Name, annotation.Value))));
        }
    }
}
