using Microsoft.EntityFrameworkCore;

namespace ICoaster.Model
{
    public partial class CoasterContext : DbContext
    {
        public CoasterContext()
        {
        }

        public CoasterContext(DbContextOptions<CoasterContext> options)
            : base(options)
        {
        }

        public virtual DbSet<UserLogin> UserLogin { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserLogin>(entity =>
            {
                entity.ToTable("user_login", "coaster");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Credentials)
                    .IsRequired()
                    .HasColumnName("credentials")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Identifier)
                    .IsRequired()
                    .HasColumnName("identifier")
                    .HasMaxLength(25)
                    .IsUnicode(false);
            });
        }
    }
}
