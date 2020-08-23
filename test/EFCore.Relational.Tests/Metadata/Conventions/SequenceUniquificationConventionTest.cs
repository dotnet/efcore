// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class SequenceUniquificationConventionTest
    {
        [ConditionalFact]
        public virtual void Sequence_names_are_truncated_and_uniquified()
        {
            var modelBuilder = GetModelBuilder();
            modelBuilder.GetInfrastructure().HasMaxIdentifierLength(10);
            modelBuilder.HasSequence("UniquifyMeToo");
            modelBuilder.HasSequence("UniquifyMeToo", "TestSchema");
            modelBuilder.HasSequence("UniquifyM!Too");
            modelBuilder.HasSequence("UniquifyM!Too", "TestSchema");
            // the below ensure we deal with clashes with existing
            // sequence names that look like candidate uniquified names
            modelBuilder.HasSequence("UniquifyM~");
            modelBuilder.HasSequence("UniquifyM~", "TestSchema");

            var model = modelBuilder.Model;
            model.FinalizeModel();

            Assert.Collection(
                model.GetSequences(),
                s0 =>
                {
                    Assert.Equal("Uniquify~1", s0.Name);
                    Assert.Null(s0.Schema);
                },
                s1 =>
                {
                    Assert.Equal("Uniquify~1", s1.Name);
                    Assert.Equal("TestSchema", s1.Schema);
                },
                s2 =>
                {
                    Assert.Equal("Uniquify~2", s2.Name);
                    Assert.Null(s2.Schema);
                },
                s3 =>
                {
                    Assert.Equal("Uniquify~2", s3.Name);
                    Assert.Equal("TestSchema", s3.Schema);
                },
                s4 =>
                {
                    Assert.Equal("UniquifyM~", s4.Name);
                    Assert.Null(s4.Schema);
                },
                s5 =>
                {
                    Assert.Equal("UniquifyM~", s5.Name);
                    Assert.Equal("TestSchema", s5.Schema);
                });
        }

        private ModelBuilder GetModelBuilder()
        {
            var conventionSet = new ConventionSet();

            var dependencies = CreateDependencies()
                .With(new CurrentDbContext(new DbContext(new DbContextOptions<DbContext>())));
            var relationalDependencies = CreateRelationalDependencies();
            conventionSet.ModelFinalizingConventions.Add(
                new SequenceUniquificationConvention(dependencies, relationalDependencies));

            return new ModelBuilder(conventionSet);
        }

        private ProviderConventionSetBuilderDependencies CreateDependencies()
            => RelationalTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>();

        private RelationalConventionSetBuilderDependencies CreateRelationalDependencies()
            => RelationalTestHelpers.Instance.CreateContextServices().GetRequiredService<RelationalConventionSetBuilderDependencies>();
    }
}
