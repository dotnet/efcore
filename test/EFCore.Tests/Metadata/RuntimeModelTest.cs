// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

public class RuntimeModelTest
{
    [Fact]
    public void Foreign_key_IsConstrained_is_preserved_in_runtime_model()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        modelBuilder.Entity<RuntimePrincipal>().HasKey(e => e.Id);
        modelBuilder.Entity<RuntimeDependent>(
            b =>
            {
                b.HasKey(e => e.Id);
                b.HasOne<RuntimePrincipal>().WithMany().HasForeignKey(e => e.PrincipalId).IsConstrained(false);
            });

        var runtimeModel = modelBuilder.FinalizeModel();

        var fk = runtimeModel.FindEntityType(typeof(RuntimeDependent))!.GetForeignKeys().Single();
        Assert.False(fk.IsConstrained);
    }

    private class RuntimePrincipal
    {
        public int Id { get; set; }
    }

    private class RuntimeDependent
    {
        public int Id { get; set; }
        public int PrincipalId { get; set; }
    }
}
