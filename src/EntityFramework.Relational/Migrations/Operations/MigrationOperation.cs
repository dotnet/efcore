// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.Operations
{
    // TODO: Reuse IAnnotaiton?
    public abstract class MigrationOperation
    {
        private readonly IDictionary<string, string> _annotations = new Dictionary<string, string>();

        protected MigrationOperation([CanBeNull] IReadOnlyDictionary<string, string> annotations)
        {
            if (annotations != null)
            {
                foreach (var item in annotations)
                {
                    _annotations.Add(item.Key, item.Value);
                }
            }
        }

        public virtual bool IsDestructiveChange => false;
        public virtual IDictionary<string, string> Annotations => _annotations;

        public virtual string this[[NotNull] string annotationName]
        {
            get
            {
                Check.NotEmpty(annotationName, nameof(annotationName));

                string value;
                _annotations.TryGetValue(annotationName, out value);

                return value;
            }
            [param: CanBeNull] set { _annotations[annotationName] = value; }
        }
    }
}
