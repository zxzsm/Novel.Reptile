using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Novel.Reptile.Entities
{
    public partial class BookContext : DbContext
    {
        public BookContext()
        {
        }

        public BookContext(DbContextOptions<BookContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Book> Book { get; set; }
        public virtual DbSet<BookCategory> BookCategory { get; set; }
        public virtual DbSet<BookGroupCategroyRelation> BookGroupCategroyRelation { get; set; }
        public virtual DbSet<BookIndex> BookIndex { get; set; }
        public virtual DbSet<BookItem> BookItem { get; set; }
        public virtual DbSet<BookReptileTask> BookReptileTask { get; set; }
        public virtual DbSet<BookShelf> BookShelf { get; set; }
        public virtual DbSet<BookThumbsup> BookThumbsup { get; set; }
        public virtual DbSet<Sign> Sign { get; set; }
        public virtual DbSet<TaskToDo> TaskToDo { get; set; }
        public virtual DbSet<UserInfo> UserInfo { get; set; }
        public virtual DbSet<UserRead> UserRead { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.0-rtm-35687");

            modelBuilder.Entity<Book>(entity =>
            {
                entity.Property(e => e.BookAuthor)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.BookImage).HasMaxLength(200);

                entity.Property(e => e.BookName)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.BookReleaseTime).HasColumnType("datetime");

                entity.Property(e => e.BookSummary).HasMaxLength(800);

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UpdateTime).HasColumnType("datetime");
            });

            modelBuilder.Entity<BookCategory>(entity =>
            {
                entity.HasKey(e => e.CategoryId);

                entity.Property(e => e.CategoryName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UpdateTime).HasColumnType("datetime");
            });

            modelBuilder.Entity<BookGroupCategroyRelation>(entity =>
            {
                entity.ToTable("Book_GroupCategroy_Relation");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UpdateTime).HasColumnType("datetime");
            });

            modelBuilder.Entity<BookIndex>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .ForSqlServerIsClustered(false);

                entity.Property(e => e.BookName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.DataYm).HasColumnName("DataYM");

                entity.Property(e => e.Date).HasColumnType("datetime");
            });

            modelBuilder.Entity<BookItem>(entity =>
            {
                entity.HasKey(e => e.ItemId);

                entity.HasIndex(e => new { e.ItemId, e.ItemName, e.BookId })
                    .HasName("IX_BookItem");

                entity.Property(e => e.CreateTime)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ItemName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Pri).HasColumnName("PRI");

                entity.Property(e => e.UpdateTime).HasColumnType("datetime");
            });

            modelBuilder.Entity<BookReptileTask>(entity =>
            {
                entity.Property(e => e.BookName).HasMaxLength(50);

                entity.Property(e => e.Created)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CurrentRecod)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Remark).HasColumnType("text");

                entity.Property(e => e.Updated).HasColumnType("datetime");

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasMaxLength(500);
            });

            modelBuilder.Entity<BookShelf>(entity =>
            {
                entity.Property(e => e.CreateTime).HasColumnType("datetime");

                entity.Property(e => e.UpdateTime).HasColumnType("datetime");
            });

            modelBuilder.Entity<BookThumbsup>(entity =>
            {
                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.Ip)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Sign>(entity =>
            {
                entity.Property(e => e.CreateTime).HasColumnType("datetime");

                entity.Property(e => e.UpdateTime).HasColumnType("datetime");
            });

            modelBuilder.Entity<TaskToDo>(entity =>
            {
                entity.Property(e => e.EndTime).HasColumnType("datetime");

                entity.Property(e => e.Remark).HasMaxLength(50);
            });

            modelBuilder.Entity<UserInfo>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.CreateTime).HasColumnType("datetime");

                entity.Property(e => e.Uesrpwd)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.UpdateTime).HasColumnType("datetime");

                entity.Property(e => e.UserEmail).HasMaxLength(20);

                entity.Property(e => e.UserMoblie).HasMaxLength(11);

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(30);
            });

            modelBuilder.Entity<UserRead>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreateTime).HasColumnType("datetime");

                entity.Property(e => e.UserId).ValueGeneratedOnAdd();
            });
        }
    }
}
