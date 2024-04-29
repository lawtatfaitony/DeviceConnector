using System;
using LanguageResource.Modal;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using MySql.Data.EntityFrameworkCore.Extensions;

namespace LanguageResource.Models
{
    public partial class LangResourceContext : DbContext
    { 
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // optionsBuilder.UseMySql("data source=81.71.74.135;database=camera_guard_business;user id=root;password=Admin@62595738;pooling=true;port=3306;");
                optionsBuilder.UseMySql("name=DataBaseContext");
            }
        } 
 
        public virtual DbSet<Language> Language { get; set; }
 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        { 
            
            modelBuilder.Entity<Language>(entity =>
            {
                entity.HasKey(e => e.KeyName)
                    .HasName("PRIMARY");

                entity.ToTable("language");

                entity.HasIndex(e => e.KeyName)
                    .HasName("key_name_Index");

                entity.Property(e => e.KeyName)
                    .HasColumnName("key_name")
                    .HasColumnType("varchar(50)")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.En_US)
                    .HasColumnName("en_us")
                    .HasColumnType("varchar(500)")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.IndustryIdArr)
                    .HasColumnType("varchar(500)")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.KeyType)
                    .HasColumnName("key_type")
                    .HasColumnType("varchar(50)")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.MainComIdArr)
                    .HasColumnType("varchar(500)")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.Remark)
                    .HasColumnName("remarks")
                    .HasColumnType("varchar(500)")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.Zh_CN)
                    .HasColumnName("zh_cn")
                    .HasColumnType("varchar(500)")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.Zh_HK)
                    .HasColumnName("zh_hk")
                    .HasColumnType("varchar(500)")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
