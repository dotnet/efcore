// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.AzureTableStorage.Metadata
{
    public class ReadOnlyAtsPropertyExtensions : IAtsPropertyExtensions
    {
        protected const string AtsColumnAnnotation = AtsAnnotationNames.Prefix + AtsAnnotationNames.ColumnName;

        private readonly IProperty _property;

        public ReadOnlyAtsPropertyExtensions([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            _property = property;
        }

        public virtual string Column
        {
            get { return _property[AtsColumnAnnotation] ?? _property.Name; }
        }

        protected virtual IProperty Property
        {
            get { return _property; }
        }
    }
}
