using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using IdentityGuesser.Models;

namespace IdentityGuesser.Models
{
    public class PoemContext: DbContext
    {
        public PoemContext(): base("DefaultConnection"){ }

        public DbSet<PoemModel> PoemModels { get; set; }
    }
}