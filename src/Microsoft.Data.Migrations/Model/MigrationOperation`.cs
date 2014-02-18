// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Migrations.Model
{
    public abstract class MigrationOperation<TInverse> : MigrationOperation
        where TInverse : MigrationOperation
    {
        public new virtual TInverse Inverse
        {
            get { return (TInverse)base.Inverse; }
        }
    }
}
