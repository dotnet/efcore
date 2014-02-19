// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Migrations.Utilities;

namespace Microsoft.Data.Migrations.Model
{
    public abstract class MigrationOperation<TTarget, TInverse> : MigrationOperation<TInverse>
        where TInverse : MigrationOperation
    {
        private readonly TTarget _target;

        protected MigrationOperation([NotNull] TTarget target)
        {
            Check.NotNull(target, "target");

            _target = target;
        }

        public virtual TTarget Target
        {
            get { return _target; }
        }
    }
}
