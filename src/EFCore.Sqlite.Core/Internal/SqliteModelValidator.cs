// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public class SqliteModelValidator : RelationalModelValidator
    {
        public SqliteModelValidator(
            [NotNull] ModelValidatorDependencies dependencies,
            [NotNull] RelationalModelValidatorDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        public override void Validate(IModel model)
        {
            base.Validate(model);

            ValidateNoSchemas(model);
            ValidateNoSequences(model);
        }

        protected virtual void ValidateNoSchemas([NotNull] IModel model)
        {
            foreach (var entityType in model.GetEntityTypes().Where(e => e.Relational().Schema != null))
            {
                Dependencies.Logger.SchemaConfiguredWarning(entityType, entityType.Relational().Schema);
            }
        }

        protected virtual void ValidateNoSequences([NotNull] IModel model)
        {
            foreach (var sequence in model.Relational().Sequences)
            {
                Dependencies.Logger.SequenceConfiguredWarning(sequence);
            }
        }
    }
}
