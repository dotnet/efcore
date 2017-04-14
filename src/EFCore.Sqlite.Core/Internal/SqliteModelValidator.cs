// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.Internal;

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
            
            EnsureNoSchemas(model);
            EnsureNoSequences(model);
        }

        protected virtual void EnsureNoSchemas([NotNull] IModel model)
        {
            foreach (var entityType in model.GetEntityTypes().Where(e => e.Sqlite().Schema != null))
            {
                Dependencies.Logger.SchemaConfiguredWarning(entityType, entityType.Sqlite().Schema);
            }
        }

        protected virtual void EnsureNoSequences([NotNull] IModel model)
        {
            foreach (var sequence in model.Sqlite().Sequences)
            {
                Dependencies.Logger.SequenceConfiguredWarning(sequence);
            }
        }
    }
}
