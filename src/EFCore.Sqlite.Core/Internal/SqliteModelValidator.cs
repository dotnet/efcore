// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
                ShowWarning(
                    SqliteEventId.SchemaConfiguredWarning,
                    SqliteStrings.SchemaConfigured(entityType.DisplayName(), entityType.Sqlite().Schema));
            }
        }

        protected virtual void EnsureNoSequences([NotNull] IModel model)
        {
            foreach (var sequence in model.Sqlite().Sequences)
            {
                ShowWarning(SqliteEventId.SequenceWarning, SqliteStrings.SequenceConfigured(sequence.Name));
            }
        }

        protected virtual void ShowWarning(SqliteEventId eventId, [NotNull] string message)
            => Dependencies.Logger.LogWarning(eventId, () => message);
    }
}
