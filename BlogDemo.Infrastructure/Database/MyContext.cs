using BlogDemo.Core.Entites;
using BlogDemo.Infrastructure.Database.EntityConfiguration;
using Microsoft.EntityFrameworkCore;

namespace BlogDemo.Infrastructure.Database
{
    public class MyContext : DbContext
    {
        public MyContext(DbContextOptions<MyContext> Option) : base(Option)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new PostConfiguration());
            modelBuilder.ApplyConfiguration(new PostImageConfiguration());
        }

        public DbSet<Post> Posts {get;set;}
        public DbSet<PostImage> PostImage {get;set;}
    }

   
}
