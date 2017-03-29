using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using IdentityGuesser.Models;

namespace IdentityGuesser.Models
{
    public class PoemContext : DbContext
    {
        public PoemContext() : base("name=PoemContext")
        {
        }

        // Add a DbSet for each one of your Entities
        public DbSet<PoemModel> PoemModels { get; set; }
    }
}