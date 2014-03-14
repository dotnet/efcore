// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public abstract class NamedMetadataBase : MetadataBase
    {
        private readonly string _name;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected NamedMetadataBase()
        {
        }

        protected NamedMetadataBase([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            _name = name;
        }

        public virtual string Name
        {
            get { return _name; }
        }

        public override string StorageName
        {
            get { return base.StorageName ?? Name; }
            set { base.StorageName = value; }
        }
    }
}
