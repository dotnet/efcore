// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.Operations
{
    // TODO: Reuse IAnnotation?
    public class ColumnModel
    {
        private readonly IDictionary<string, string> _annotations = new Dictionary<string, string>();

        public ColumnModel(
            [CanBeNull] string name,
            [CanBeNull] string storeType,
            bool nullable,
            [CanBeNull] object defaultValue,
            [CanBeNull] string defaultValueSql,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null)
        {
            Name = name;
            StoreType = storeType;
            Nullable = nullable;
            DefaultValue = defaultValue;
            DefaultValueSql = defaultValueSql;

            if (annotations != null)
            {
                foreach (var item in annotations)
                {
                    _annotations.Add(item.Key, item.Value);
                }
            }
        }

        public virtual string Name { get;[param: NotNull] set; }
        public virtual string StoreType { get;[param: NotNull] set; }
        public virtual bool Nullable { get; }
        public virtual object DefaultValue { get;[param: CanBeNull] set; }
        public virtual string DefaultValueSql { get;[param: CanBeNull] set; }
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
