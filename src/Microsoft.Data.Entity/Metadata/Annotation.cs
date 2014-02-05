// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class Annotation : MetadataBase
    {
        private readonly object _value;

        public Annotation(string name, object value)
            : base(name)
        {
            Check.NotNull(value, "value");

            _value = value;
        }

        public virtual object Value
        {
            get { return _value; }
        }
    }
}
