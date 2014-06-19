// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Relational.Update
{
    public class ParameterNameGenerator
    {
        private int _count;

        public virtual string GenerateNext()
        {
            return "@p" + _count++;
        }
    }
}
