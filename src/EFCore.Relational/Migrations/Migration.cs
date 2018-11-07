// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    ///     A base class inherited by each EF Core migration.
    /// </summary>
    public abstract class Migration
    {
        /// <summary>
        ///     The migration identifier for the empty database.
        /// </summary>
        public const string InitialDatabase = "0";

        private readonly LazyRef<IModel> _targetModel;
        private readonly LazyRef<List<MigrationOperation>> _upOperations;
        private readonly LazyRef<List<MigrationOperation>> _downOperations;

        /// <summary>
        ///     Creates a <see cref="Migration" /> instance.
        /// </summary>
        protected Migration()
        {
            _targetModel = new LazyRef<IModel>(
                () =>
                {
                    var modelBuilder = new ModelBuilder(new ConventionSet());
                    BuildTargetModel(modelBuilder);

                    return modelBuilder.Model;
                });
            _upOperations = new LazyRef<List<MigrationOperation>>(() => BuildOperations(Up));
            _downOperations = new LazyRef<List<MigrationOperation>>(() => BuildOperations(Down));
        }

        /// <summary>
        ///     The <see cref="IModel" /> that the database will map to after the migration has been applied.
        /// </summary>
        public virtual IModel TargetModel => _targetModel.Value;

        /// <summary>
        ///     <para>
        ///         The <see cref="MigrationOperation" />s that will migrate the database 'up'.
        ///     </para>
        ///     <para>
        ///         That is, those operations that need to be applied to the database
        ///         to take it from the state left in by the previous migration so that it is up-to-date
        ///         with regard to this migration.
        ///     </para>
        /// </summary>
        public virtual IReadOnlyList<MigrationOperation> UpOperations => _upOperations.Value;

        /// <summary>
        ///     <para>
        ///         The <see cref="MigrationOperation" />s that will migrate the database 'down'.
        ///     </para>
        ///     <para>
        ///         That is, those operations that need to be applied to the database
        ///         to take it from the state left in by this migration so that it returns to the
        ///         state that it was in before this migration was applied.
        ///     </para>
        /// </summary>
        public virtual IReadOnlyList<MigrationOperation> DownOperations => _downOperations.Value;

        /// <summary>
        ///     <para>
        ///         The name of the current database provider.
        ///     </para>
        ///     <para>
        ///         This can be used to write conditional code in the migration such that different changes
        ///         can be made to the database depending on the type of database being used.
        ///     </para>
        /// </summary>
        public virtual string ActiveProvider { get; [param: NotNull] set; }

        /// <summary>
        ///     Implemented to builds the <see cref="TargetModel" />.
        /// </summary>
        /// <param name="modelBuilder"> The <see cref="ModelBuilder" /> to use to build the model. </param>
        protected virtual void BuildTargetModel([NotNull] ModelBuilder modelBuilder)
        {
        }

        /// <summary>
        ///     <para>
        ///         Builds the operations that will migrate the database 'up'.
        ///     </para>
        ///     <para>
        ///         That is, builds the operations that will take the database from the state left in by the
        ///         previous migration so that it is up-to-date with regard to this migration.
        ///     </para>
        ///     <para>
        ///         This method must be overridden in each class the inherits from <see cref="Migration" />.
        ///     </para>
        /// </summary>
        /// <param name="migrationBuilder"> The <see cref="MigrationBuilder" /> that will build the operations. </param>
        protected abstract void Up([NotNull] MigrationBuilder migrationBuilder);

        /// <summary>
        ///     <para>
        ///         Builds the operations that will migrate the database 'down'.
        ///     </para>
        ///     <para>
        ///         That is, builds the operations that will take the database from the state left in by
        ///         this migration so that it returns to the state that it was in before this migration was applied.
        ///     </para>
        ///     <para>
        ///         This method must be overridden in each class the inherits from <see cref="Migration" /> if
        ///         both 'up' and 'down' migrations are to be supported. If it is not overridden, then calling it
        ///         will throw and it will not be possible to migrate in the 'down' direction.
        ///     </para>
        /// </summary>
        /// <param name="migrationBuilder"> The <see cref="MigrationBuilder" /> that will build the operations. </param>
        protected virtual void Down([NotNull] MigrationBuilder migrationBuilder)
        {
            throw new NotImplementedException();
        }

        private List<MigrationOperation> BuildOperations(Action<MigrationBuilder> buildAction)
        {
            var migrationBuilder = new MigrationBuilder(ActiveProvider);
            buildAction(migrationBuilder);

            return migrationBuilder.Operations;
        }
    }
}
