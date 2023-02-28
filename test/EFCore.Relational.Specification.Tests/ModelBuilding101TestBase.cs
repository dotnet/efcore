// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public abstract class ModelBuilding101RelationalTestBase : ModelBuilding101TestBase
{
    protected override ModelMetadata GetModelMetadata(Context101 context)
        => new RelationalModelMetadata(context.Model, context.Database.GenerateCreateScript());

    protected class RelationalModelMetadata : ModelMetadata
    {
        public RelationalModelMetadata(IModel model, string schema)
            : base(model)
        {
            Schema = schema;
        }

        public virtual string Schema { get; }

        protected bool Equals(RelationalModelMetadata other)
            => base.Equals(other)
                && Schema == other.Schema;

        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), Schema);
    }
}
