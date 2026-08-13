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
    public DbSet<PieceService> PieceServices => Set<PieceService>();
    public DbSet<Production> Productions => Set<Production>();
    public DbSet<ProductionProcess> ProductionProcesses => Set<ProductionProcess>();
    public DbSet<Assignment> Assignments => Set<Assignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Chave composta de PieceService
        modelBuilder.Entity<PieceService>()
            .HasKey(ps => new
            {
                ps.PieceModelId,
                ps.ServiceProcessId
            });

        // PieceModel -> PieceServices
        modelBuilder.Entity<PieceService>()
            .HasOne(ps => ps.PieceModel)
            .WithMany(pm => pm.PieceServices)
            .HasForeignKey(ps => ps.PieceModelId);

        // ServiceProcess -> PieceServices
        modelBuilder.Entity<PieceService>()
            .HasOne(ps => ps.ServiceProcess)
            .WithMany(sp => sp.PieceServices)
            .HasForeignKey(ps => ps.ServiceProcessId);
    }
}