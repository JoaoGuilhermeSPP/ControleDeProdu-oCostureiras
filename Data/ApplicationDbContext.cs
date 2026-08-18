using CosturaProducao.Models;
using Microsoft.EntityFrameworkCore;

namespace CosturaProducao.Data;
public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Company> Companies => Set<Company>();

    public DbSet<Seamstress> Seamstresses => Set<Seamstress>();

    public DbSet<Client> Clients => Set<Client>();

    public DbSet<ServiceProcess> ServiceProcesses => Set<ServiceProcess>();

    public DbSet<PieceModel> PieceModels => Set<PieceModel>();

    public DbSet<PieceVariant> PieceVariants => Set<PieceVariant>();

    public DbSet<PieceService> PieceServices => Set<PieceService>();

    public DbSet<Production> Productions => Set<Production>();

    public DbSet<ProductionProcess> ProductionProcesses => Set<ProductionProcess>();

    public DbSet<Assignment> Assignments => Set<Assignment>();

    public DbSet<PieceSize> PieceSizes => Set<PieceSize>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------- PieceVariant ----------
        modelBuilder.Entity<PieceVariant>()
            .HasOne(x => x.PieceModel)
            .WithMany(x => x.Variants)
            .HasForeignKey(x => x.PieceModelId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PieceVariant>()
            .HasIndex(x => new { x.PieceModelId, x.Cor, x.Tamanho })
            .IsUnique();

        // ---------- Production ----------
        modelBuilder.Entity<Production>()
            .HasOne(x => x.PieceVariant)
            .WithMany(x => x.Productions)
            .HasForeignKey(x => x.PieceVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        // ---------- PieceSize ----------
        modelBuilder.Entity<PieceSize>()
            .HasOne(x => x.PieceModel)
            .WithMany(x => x.PieceSizes)
            .HasForeignKey(x => x.PieceModelId)
            .OnDelete(DeleteBehavior.Cascade);

        // ---------- PieceService (agora com chave Id) ----------
        modelBuilder.Entity<PieceService>()
            .HasKey(x => x.Id);  // chave primária simples

        // Índice único para evitar duplicatas
        modelBuilder.Entity<PieceService>()
            .HasIndex(x => new { x.PieceModelId, x.ServiceProcessId })
            .IsUnique();

        modelBuilder.Entity<PieceService>()
            .HasOne(x => x.PieceModel)
            .WithMany(x => x.PieceServices)
            .HasForeignKey(x => x.PieceModelId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PieceService>()
            .HasOne(x => x.ServiceProcess)
            .WithMany(x => x.PieceServices)
            .HasForeignKey(x => x.ServiceProcessId)
            .OnDelete(DeleteBehavior.Cascade);
    }
    }