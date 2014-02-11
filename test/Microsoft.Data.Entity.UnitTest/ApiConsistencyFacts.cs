// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;

namespace Microsoft.Data.Entity
{
    public class ApiConsistencyFacts : ApiConsistencyFactsBase
    {
        protected override Assembly TargetAssembly
        {
            get { return typeof(Metadata.Entity).Assembly; }
        }
    }
}
