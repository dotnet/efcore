// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;

using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class Property : MetadataBase
    {
        private readonly PropertyInfo _propertyInfo;

        public Property(PropertyInfo propertyInfo)
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
