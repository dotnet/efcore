// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design
{
    public class AnnotationCodeGeneratorTest
    {
        [ConditionalFact]
        public void IsTableExcludedFromMigrations_false_is_handled_by_convention()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity("foo").ToTable("foo");
            var entityType = modelBuilder.Model.GetEntityTypes().Single();

            var annotations = entityType.GetAnnotations().ToDictionary(a => a.Name, a => a);
            CreateGenerator().RemoveAnnotationsHandledByConventions((IEntityType)entityType, annotations);

            Assert.DoesNotContain(RelationalAnnotationNames.IsTableExcludedFromMigrations, annotations.Keys);
        }

        private ModelBuilder CreateModelBuilder()
            => RelationalTestHelpers.Instance.CreateConventionBuilder();

        private AnnotationCodeGenerator CreateGenerator()
            => new(
                new AnnotationCodeGeneratorDependencies(
                    new TestRelationalTypeMappingSource(
                        TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                        TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>())));
    }
}
