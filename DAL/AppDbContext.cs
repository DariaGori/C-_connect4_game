using System;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class AppDbContext: DbContext
    {
        public DbSet<GameState> GameStates { get; set; } = default!;
        
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions? options): base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder
                .UseSqlite("Data Source=C:\\Users\\Daria\\RiderProjects\\charp2019fall\\Connect_4\\connect4.db");
        }

    }
}