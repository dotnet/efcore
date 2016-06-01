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
            _upOperations = new LazyRef<List<MigrationOperation>>(BuildUpOperations);
            _downOperations = new LazyRef<List<MigrationOperation>>(BuildDownOperations);
        }

        private List<MigrationOperation> BuildUpOperations()
        {
            var operations = new List<MigrationOperation>();

            operations.AddRange(BuildOperations(BeforeUp));
            operations.AddRange(BuildOperations(Up));
            operations.AddRange(BuildOperations(AfterUp));

            return operations;
        }

        private List<MigrationOperation> BuildDownOperations()
        {
            var operations = new List<MigrationOperation>();

            operations.AddRange(BuildOperations(BeforeDown));
            operations.AddRange(BuildOperations(Down));
            operations.AddRange(BuildOperations(AfterDown));

            return operations;
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


        protected virtual void BeforeUp([NotNull] MigrationBuilder migrationBuilder)
        {
        }

        protected virtual void AfterUp([NotNull] MigrationBuilder migrationBuilder)
        {
        }


        protected virtual void BeforeDown([NotNull] MigrationBuilder migrationBuilder)
        {
        }

        protected virtual void AfterDown([NotNull] MigrationBuilder migrationBuilder)
        {
        }


        private List<MigrationOperation> BuildOperations(Action<MigrationBuilder> buildAction)
        {
            var migrationBuilder = new MigrationBuilder(ActiveProvider);
            buildAction(migrationBuilder);

            return migrationBuilder.Operations;
        }
    }
}
