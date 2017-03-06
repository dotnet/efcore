using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E2E.Sqlite
{
    public partial class Groups
    {
        public Groups()
        {
            UsersGroups = new HashSet<UsersGroups>();
        }

        public string Id { get; set; }

        [InverseProperty("Group")]
        public ICollection<UsersGroups> UsersGroups { get; set; }
    }
}
