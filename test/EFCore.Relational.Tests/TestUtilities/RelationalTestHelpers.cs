// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class RelationalTestHelpers : TestHelpers
    {
        protected RelationalTestHelpers()
        {
        }

        public static RelationalTestHelpers Instance { get; } = new RelationalTestHelpers();

        public override IServiceCollection AddProviderServices(IServiceCollection services)
            => FakeRelationalOptionsExtension.AddEntityFrameworkRelationalDatabase(services);

        protected override void UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
        {
            var extension = optionsBuilder.Options.FindExtension<FakeRelationalOptionsExtension>()
                            ?? new FakeRelationalOptionsExtension();

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(
                extension.WithConnection(new FakeDbConnection("Database=Fake")));
        }

        public override IModelValidator CreateModelValidator()
        {
            var typeMappingSource = new TestRelationalTypeMappingSource(
                TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>());

            return new RelationalModelValidator(
                new ModelValidatorDependencies(
                    typeMappingSource,
                    new MemberClassifier(
                        typeMappingSource,
                        TestServiceFactory.Instance.Create<IParameterBindingFactories>())),
                new RelationalModelValidatorDependencies(typeMappingSource));
        }

        public override LoggingDefinitions LoggingDefinitions { get; } = new TestRelationalLoggingDefinitions();
    }
}
