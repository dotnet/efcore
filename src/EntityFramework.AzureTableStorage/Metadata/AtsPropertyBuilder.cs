// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.AzureTableStorage.Metadata
{
    public class AtsPropertyBuilder
    {
        private readonly Property _property;

        public AtsPropertyBuilder([NotNull] Property property)
        {
            Check.NotNull(property, "property");

            _property = property;
        }

        public virtual AtsPropertyBuilder Column([CanBeNull] string columnName)
        {
            Check.NullButNotEmpty(columnName, "columnName");

            _property.AzureTableStorage().Column = columnName;

            return this;
        }
    }
}
