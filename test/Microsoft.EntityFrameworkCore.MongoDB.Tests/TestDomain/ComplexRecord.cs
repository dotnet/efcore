#if !(NET451 && DRIVER_NOT_SIGNED)
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests.TestDomain
{
    public class ComplexRecord : IEquatable<ComplexRecord>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ObjectId Id { get; private set; }

        public ComplexSubDocument ComplexSubDocument { get; set; } = new ComplexSubDocument();

        public override int GetHashCode()
            => base.GetHashCode();

        public override bool Equals(object obj)
            => Equals(obj as ComplexRecord);

        public bool Equals(ComplexRecord other)
            => Id.Equals(other?.Id) &&
               Equals(ComplexSubDocument, other?.ComplexSubDocument);
    }
}
#endif //!(NET451 && DRIVER_NOT_SIGNED)