using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PersonalShoppingAPI.Dto;

#nullable disable

namespace PersonalShoppingAPI.Model
{
    public partial class SHOPPINGLISTContext : DbContext
    {
        public SHOPPINGLISTContext()
        {
        }

        public SHOPPINGLISTContext(DbContextOptions<SHOPPINGLISTContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Month> Months { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<SystemLog> SystemLogs { get; set; }
        public virtual DbSet<Systemdefault> Systemdefaults { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserCodeVM> NextNumber { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Name=DevConnectionString");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Category");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Month>(entity =>
            {
                entity.Property(e => e.MonthName)
                    .IsRequired()
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.DateCreated).HasColumnType("datetime");

                entity.Property(e => e.DateUpdated).HasColumnType("datetime");

                entity.Property(e => e.ImageUrl).IsRequired();

                entity.Property(e => e.ProductDescription).IsRequired();

                entity.Property(e => e.ProductId).IsRequired();

                entity.Property(e => e.ProductName).IsRequired();

                entity.Property(e => e.Store).IsRequired();

                entity.HasOne(d => d.Cateory)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.CateoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Products__Cateor__2A4B4B5E");

                entity.HasOne(d => d.Month)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.MonthId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Products__MonthI__2B3F6F97");
            });

            modelBuilder.Entity<Systemdefault>(entity =>
            {
                entity.ToTable("SYSTEMDEFAULTS");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Smskey)
                    .IsRequired()
                    .HasColumnName("SMSKey");

                entity.Property(e => e.Smsname).HasColumnName("SMSName");

                entity.Property(e => e.SmsuserId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("SMSUserId");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.PhoneNumber })
                    .HasName("PK__Users__3214EC0749F30D3D");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.PhoneNumber).HasMaxLength(20);

                entity.Property(e => e.DateCreated).HasColumnType("datetime");

                entity.Property(e => e.DateUpdated).HasColumnType("datetime");

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.ImageUrl).IsRequired();

                entity.Property(e => e.Password).IsRequired();

                entity.Property(e => e.Role)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<UserCodeVM>(entity =>
            {
                entity.HasNoKey();

            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
