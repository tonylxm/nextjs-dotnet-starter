using Microsoft.EntityFrameworkCore;

namespace Starter.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
}
