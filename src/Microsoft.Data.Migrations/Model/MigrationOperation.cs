// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Migrations.Model
{
    public abstract class MigrationOperation
    {
        public virtual MigrationOperation Inverse
        {
            get { return null; }
        }

        public abstract bool IsDestructiveChange { get; }
    }
}
