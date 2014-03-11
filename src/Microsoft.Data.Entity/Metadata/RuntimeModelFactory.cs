// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class RuntimeModelFactory
    {
        private readonly EntityKeyFactorySource _keyFactorySource;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected RuntimeModelFactory()
        {
        }

        public RuntimeModelFactory([NotNull] EntityKeyFactorySource keyFactorySource)
        {
            Check.NotNull(keyFactorySource, "keyFactorySource");

            _keyFactorySource = keyFactorySource;
        }

        public virtual RuntimeModel Create([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            return new RuntimeModel(model, _keyFactorySource);
        }
    }
}
