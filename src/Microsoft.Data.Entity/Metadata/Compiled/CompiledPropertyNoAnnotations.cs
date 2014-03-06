// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public class CompiledPropertyNoAnnotations<TEntity, TProperty>
    {
        public IReadOnlyList<IAnnotation> Annotations
        {
            get { return ImmutableList<Annotation>.Empty; }
        }

        public string this[[NotNull] string annotationName]
        {
            get
            {
                Check.NotEmpty(annotationName, "annotationName");

                return null;
            }
        }

        public Type PropertyType
        {
            get { return typeof(TProperty); }
        }

        public Type DeclaringType
        {
            get { return typeof(TEntity); }
        }

        public ValueGenerationStrategy ValueGenerationStrategy
        {
            get { return ValueGenerationStrategy.None; }
        }

        public bool IsNullable
        {
            get { return typeof(TProperty).IsNullableType(); }
        }

        public IEntityType EntityType
        {
            // TODO
            get { return null; }
        }
    }
}
