// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class EntityAttacherFactory
    {
        public virtual IEntityAttacher CreateForAttach() => new KeyValueEntityAttacher(updateExistingEntities: false);

        public virtual IEntityAttacher CreateForUpdate() => new KeyValueEntityAttacher(updateExistingEntities: true);
    }
}
