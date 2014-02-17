// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Metadata
{
    internal class MetadataName
    {
        private readonly string _name;
        private string _storageName;

        public MetadataName(string name)
        {
            _name = name;
        }

        public virtual string Name
        {
            get { return _name; }
        }

        public virtual string StorageName
        {
            get { return _storageName ?? Name; }
            set { _storageName = value; }
        }
    }
}
