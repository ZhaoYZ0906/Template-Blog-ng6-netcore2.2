using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BlogDemo.Infrastructure.Database
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MyContext>
    {
        public MyContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<MyContext>();
            builder.UseSqlServer("Data Source=.; Database=BlogDemo;User Id=sa;Password=123");
            return new MyContext(builder.Options);
        }
    }
}
