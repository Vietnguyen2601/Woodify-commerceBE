using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;

namespace PaymentService.Infrastructure.Data;

/// <summary>
/// DbContext cho Payment Service
/// </summary>
public class PaymentDbContext : DbContext
{
    public PaymentDbContext() { }

    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
    {
    }

    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<Wallet> Wallets { get; set; } = null!;
    public DbSet<WalletTransaction> WalletTransactions { get; set; } = null!;

    private static string GetConnectionString(string connectionStringName)
    {
        var rootEnvPath = FindEnvFile();
        if (rootEnvPath != null && File.Exists(rootEnvPath))
        {
            foreach (var line in File.ReadAllLines(rootEnvPath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;
                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                    Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
            }
        }

        // Kiểm tra biến môi trường trước
        string envConnectionString = Environment.GetEnvironmentVariable($"ConnectionStrings__{connectionStringName}");
        if (!string.IsNullOrEmpty(envConnectionString))
        {
            return envConnectionString;
        }

        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        return config.GetConnectionString(connectionStringName);
    }

    private static string FindEnvFile()
    {
        var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        while (dir != null)
        {
            var envFile = Path.Combine(dir.FullName, ".env");
            if (File.Exists(envFile))
                return envFile;
            dir = dir.Parent;
        }
        return null;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(GetConnectionString("PaymentService"))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Payment configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payment");
            entity.HasKey(e => e.PaymentId);

            entity.Property(e => e.PaymentId)
                .HasColumnName("payment_id");

            entity.Property(e => e.OrderId)
                .HasColumnName("order_id");

            entity.Property(e => e.AccountId)
                .HasColumnName("account_id");

            entity.Property(e => e.Provider)
                .HasColumnName("provider")
                .HasMaxLength(50);

            entity.Property(e => e.ProviderPaymentId)
                .HasColumnName("provider_payment_id")
                .HasMaxLength(255);

            entity.Property(e => e.AmountCents)
                .HasColumnName("amount_cents");

            entity.Property(e => e.Currency)
                .HasColumnName("currency")
                .HasMaxLength(10)
                .HasDefaultValue("VND");

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(PaymentStatus.Created);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.Property(e => e.ProviderResponse)
                .HasColumnName("provider_response");

            // Indexes
            entity.HasIndex(e => e.ProviderPaymentId);
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.AccountId);
        });

        // Wallet configuration
        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.ToTable("Wallet");
            entity.HasKey(e => e.WalletId);

            entity.Property(e => e.WalletId)
                .HasColumnName("wallet_id");

            entity.Property(e => e.AccountId)
                .HasColumnName("account_id");

            entity.Property(e => e.BalanceCents)
                .HasColumnName("balance_cents")
                .HasDefaultValue(0);

            entity.Property(e => e.Currency)
                .HasColumnName("currency")
                .HasMaxLength(10)
                .HasDefaultValue("VND");

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(WalletStatus.Active);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasIndex(e => e.AccountId);
        });

        // WalletTransaction configuration
        modelBuilder.Entity<WalletTransaction>(entity =>
        {
            entity.ToTable("Wallet_Transaction");
            entity.HasKey(e => e.WalletTxId);

            entity.Property(e => e.WalletTxId)
                .HasColumnName("wallet_tx_id");

            entity.Property(e => e.WalletId)
                .HasColumnName("wallet_id");

            entity.Property(e => e.TxType)
                .HasColumnName("tx_type")
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(e => e.AmountCents)
                .HasColumnName("amount_cents");

            entity.Property(e => e.BalanceBeforeCents)
                .HasColumnName("balance_before_cents");

            entity.Property(e => e.BalanceAfterCents)
                .HasColumnName("balance_after_cents");

            entity.Property(e => e.RelatedOrderId)
                .HasColumnName("related_order_id");

            entity.Property(e => e.RelatedPaymentId)
                .HasColumnName("related_payment_id");

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(WalletTransactionStatus.Pending);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(e => e.CompletedAt)
                .HasColumnName("completed_at");

            entity.Property(e => e.Note)
                .HasColumnName("note");

            // Relationships
            entity.HasOne(e => e.Wallet)
                .WithMany(w => w.Transactions)
                .HasForeignKey(e => e.WalletId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.WalletId);
            entity.HasIndex(e => e.RelatedOrderId);
            entity.HasIndex(e => e.RelatedPaymentId);
        });
    }
}
