using CosturaProducao.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CosturaProducao.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Seamstress> Seamstresses => Set<Seamstress>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<ServiceProcess> ServiceProcesses => Set<ServiceProcess>();
    public DbSet<PieceModel> PieceModels => Set<PieceModel>();
    public DbSet<PieceService> PieceServices => Set<PieceService>();
    public DbSet<Production> Productions => Set<Production>();
    public DbSet<ProductionProcess> ProductionProcesses => Set<ProductionProcess>();
    public DbSet<Assignment> Assignments => Set<Assignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Company>().Property(x => x.Name).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<Seamstress>().Property(x => x.Name).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<Client>().Property(x => x.Name).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<ServiceProcess>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.DefaultPricePerPiece).HasPrecision(10, 2);
        });
        modelBuilder.Entity<PieceModel>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(40).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
        });
        modelBuilder.Entity<PieceService>().HasKey(x => new { x.PieceModelId, x.ServiceProcessId });
        modelBuilder.Entity<ProductionProcess>(entity =>
        {
            entity.Property(x => x.PricePerPiece).HasPrecision(10, 2);
            entity.HasOne(x => x.Production).WithMany(x => x.Processes).HasForeignKey(x => x.ProductionId);
        });
        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.Property(x => x.PricePerPiece).HasPrecision(10, 2);
            entity.HasOne(x => x.Seamstress).WithMany(x => x.Assignments).HasForeignKey(x => x.SeamstressId);
        });
    }
}