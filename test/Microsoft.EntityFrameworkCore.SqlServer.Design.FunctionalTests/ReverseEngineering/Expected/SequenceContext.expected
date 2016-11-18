using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace E2ETest.Namespace
{
    public partial class SequenceContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                #warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer(@"{{connectionString}}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence("CountByTwo").IncrementsBy(2);

            modelBuilder.HasSequence("CyclicalCountByThree")
                .StartsAt(6)
                .IncrementsBy(3)
                .HasMin(0)
                .HasMax(27)
                .IsCyclic();

            modelBuilder.HasSequence<int>("IntSequence");

            modelBuilder.HasSequence<short>("SmallIntSequence");

            modelBuilder.HasSequence<byte>("TinyIntSequence");
        }
    }
}