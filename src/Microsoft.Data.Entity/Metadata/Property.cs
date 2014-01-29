// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Metadata
{
    using System.Reflection;
    using JetBrains.Annotations;
    using Microsoft.Data.Entity.Utilities;

    public class Property : MetadataBase
    {
        private readonly PropertyInfo _propertyInfo;

        public Property([NotNull] PropertyInfo propertyInfo)
            : base(Check.NotNull(propertyInfo, "propertyInfo", p => p.Name))
        {
            _propertyInfo = propertyInfo;
        }

        public virtual PropertyInfo PropertyInfo
        {
            get { return _propertyInfo; }
        }
    }
}
