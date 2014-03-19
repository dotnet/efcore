// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Migrations.Utilities;

namespace Microsoft.Data.Migrations.Model
{
    public class CreateDatabaseOperation : MigrationOperation
    {
        private readonly string _databaseName;

        public CreateDatabaseOperation([NotNull] string databaseName)
        {
            Check.NotEmpty(databaseName, "databaseName");

            _databaseName = databaseName;
        }

        public virtual string DatabaseName
        {
            get { return _databaseName; }
        }

        public override void Accept([NotNull] MigrationOperationVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            visitor.Visit(this);
        }
    }
}
