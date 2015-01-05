// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations
{
    public class MigrationOperationProcessor
    {
        private readonly IRelationalMetadataExtensionProvider _extensionProvider;
        private readonly RelationalTypeMapper _typeMapper;
        private readonly MigrationOperationFactory _operationFactory;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected MigrationOperationProcessor()
        {
        }

        public MigrationOperationProcessor(
            [NotNull] IRelationalMetadataExtensionProvider extensionProvider,
            [NotNull] RelationalTypeMapper typeMapper,
            [NotNull] MigrationOperationFactory operationFactory)
        {
            Check.NotNull(extensionProvider, "extensionProvider");
            Check.NotNull(typeMapper, "typeMapper");
            Check.NotNull(operationFactory, "operationFactory");

            _extensionProvider = extensionProvider;
            _typeMapper = typeMapper;
            _operationFactory = operationFactory;
        }

        public virtual IRelationalMetadataExtensionProvider ExtensionProvider
        {
            get { return _extensionProvider; }
        }

        protected virtual RelationalNameBuilder NameBuilder
        {
            get { return ExtensionProvider.NameBuilder; }
        }

        public virtual RelationalTypeMapper TypeMapper
        {
            get { return _typeMapper; }
        }

        public virtual MigrationOperationFactory OperationFactory
        {
            get { return _operationFactory; }
        }

        public virtual IReadOnlyList<MigrationOperation> Process(
            [NotNull] MigrationOperationCollection operations,
            [NotNull] IModel sourceModel,
            [NotNull] IModel targetModel)
        {
            return operations.GetAll();
        }
    }
}
