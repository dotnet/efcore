// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Tests
{
    public class TestMetadataExtensionProvider : IRelationalMetadataExtensionProvider
    {
        public IRelationalEntityTypeAnnotations For(IEntityType entityType) => entityType.TestProvider();
        public IRelationalForeignKeyAnnotations For(IForeignKey foreignKey) => foreignKey.TestProvider();
        public IRelationalIndexAnnotations For(IIndex index) => index.TestProvider();
        public IRelationalKeyAnnotations For(IKey key) => key.TestProvider();
        public IRelationalModelAnnotations For(IModel model) => model.TestProvider();
        public IRelationalPropertyAnnotations For(IProperty property) => property.TestProvider();
    }
}
