using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using McqTask.Models;

namespace McqTask.Data
{
    public class McqTaskContext : DbContext
    {
        public McqTaskContext (DbContextOptions<McqTaskContext> options)
            : base(options)
        {
        }

        public DbSet<McqTask.Models.Category> Category { get; set; } = default!;
    }
}
