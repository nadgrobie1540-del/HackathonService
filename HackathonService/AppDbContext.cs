using Microsoft.EntityFrameworkCore;

namespace HackathonService
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ObjectType> ObjectTypes { get; set; }
        public DbSet<Attribute> Attributes { get; set; }
        public DbSet<ControlObject> ControlObjects { get; set; }
        public DbSet<AttributeValue> AttributeValues { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Document> Documents { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ControlObject>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<AttributeValue>()
                .Property(e =>  e.ValueText)
                .HasColumnType ("nvarchar(max)");
            modelBuilder.Entity<ControlObject>()
                .HasIndex(e => e.Status);
            modelBuilder.Entity<Assignment>()
                .HasIndex(e =>  e.DueDate);
            modelBuilder.Entity< AttributeValue>()
                .HasOne(e => e.Object)
                .WithMany(e => e.AttributeValues)
                .HasForeignKey (e => e.ObjectId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AttributeValue>()
                .HasOne(e =>  e.Attribute)
                .WithMany(e => e.Values)
                .HasForeignKey( e => e.AttributeId)
                .OnDelete( DeleteBehavior.Restrict);
            modelBuilder.Entity<Assignment>()
                .HasOne(e =>  e.Object)
                .WithMany(e => e.Assignments)
                .HasForeignKey(e => e.ObjectId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Document>()
                .HasOne(e  => e.Object)
                .WithMany(e => e.Documents)
                .HasForeignKey(e => e.ObjectId )
               .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Document> ()
                .HasOne(e => e.Assignment) 
                .WithMany(e => e.Documents )
                .HasForeignKey(e =>  e.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}