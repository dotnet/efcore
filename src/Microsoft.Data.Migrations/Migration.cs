// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Migrations
{
    public abstract class Migration
    {
        public abstract void Up();
        public abstract void Down();
    }
}
