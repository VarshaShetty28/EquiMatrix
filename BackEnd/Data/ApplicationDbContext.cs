using Microsoft.EntityFrameworkCore;
using BackEnd.Models;

namespace BackEnd.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Award> Awards { get; set; }
        public DbSet<ExerciseRequest> ExerciseRequests { get; set; }
        public DbSet<VestingSchedule> VestingSchedules { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<TaxCountry> TaxCountries { get; set; }
        public DbSet<TaxRegime> TaxRegimes { get; set; }
        public DbSet<TaxSlab> TaxSlabs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // =============================
    // FIX CASCADE DELETE FOR ExerciseRequests
    // =============================
    modelBuilder.Entity<ExerciseRequest>()
        .HasOne(er => er.Employee)
        .WithMany(e => e.ExerciseRequests)
        .HasForeignKey(er => er.Employee_EmpId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<ExerciseRequest>()
        .HasOne(er => er.Award)
        .WithMany(a => a.ExerciseRequests)
        .HasForeignKey(er => er.Awards_AwardId)
        .OnDelete(DeleteBehavior.Restrict);


    // =============================
    // FIX CASCADE DELETE FOR VestingSchedules
    // =============================
    modelBuilder.Entity<VestingSchedule>()
        .HasOne(vs => vs.Employee)
        .WithMany(e => e.VestingSchedules)
        .HasForeignKey(vs => vs.Employee_EmpId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<VestingSchedule>()
        .HasOne(vs => vs.Award)
        .WithMany(a => a.VestingSchedules)
        .HasForeignKey(vs => vs.Awards_AwardId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<ExerciseRequest>(entity =>
    {
        entity.Property(e => e.CurrentSharePrice).HasPrecision(18, 2);
        entity.Property(e => e.ExerciseAmountUsd).HasPrecision(18, 2);
        entity.Property(e => e.TaxableGainUsd).HasPrecision(18, 2);
        entity.Property(e => e.TaxAmountUsd).HasPrecision(18, 2);
        entity.Property(e => e.NetAmountUsd).HasPrecision(18, 2);
    });

    modelBuilder.Entity<TaxCountry>(entity =>
    {
        entity.Property(e => e.ReferenceFxRate).HasPrecision(18, 2);
    });

    modelBuilder.Entity<TaxRegime>(entity =>
    {
        entity.Property(e => e.CessRate).HasPrecision(18, 2);
        entity.Property(e => e.RebateThresholdUsd).HasPrecision(18, 2);
        entity.Property(e => e.RebateAmountUsd).HasPrecision(18, 2);
    });

    modelBuilder.Entity<TaxSlab>(entity =>
    {
        entity.Property(e => e.LowerBoundUsd).HasPrecision(18, 2);
        entity.Property(e => e.UpperBoundUsd).HasPrecision(18, 2);
        entity.Property(e => e.Rate).HasPrecision(18, 2);
    });
}
    }
}