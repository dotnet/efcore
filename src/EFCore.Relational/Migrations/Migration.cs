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
    public abstract class Migration
    {
        public const string InitialDatabase = "0";

        private readonly LazyRef<IModel> _targetModel;
        private readonly LazyRef<List<MigrationOperation>> _upOperations;
        private readonly LazyRef<List<MigrationOperation>> _downOperations;

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

        public virtual IModel TargetModel => _targetModel.Value;
        public virtual IReadOnlyList<MigrationOperation> UpOperations => _upOperations.Value;
        public virtual IReadOnlyList<MigrationOperation> DownOperations => _downOperations.Value;
        public virtual string ActiveProvider { get; [param: NotNull] set; }

        protected virtual void BuildTargetModel([NotNull] ModelBuilder modelBuilder)
        {
        }

        protected abstract void Up([NotNull] MigrationBuilder migrationBuilder);

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
