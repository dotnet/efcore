// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.SqlServer
{
    using Microsoft.Data.Entity;
    using Microsoft.Data.Entity.Utilities;

    public class RelationalDataStore : DataStore
    {
        private readonly string _nameOrConnectionString;

        public RelationalDataStore(string nameOrConnectionString)
        {
            Check.NotEmpty(nameOrConnectionString, "nameOrConnectionString");

            _nameOrConnectionString = nameOrConnectionString;
        }

        public string NameOrConnectionString
        {
            get { return _nameOrConnectionString; }
        }
    }
}
