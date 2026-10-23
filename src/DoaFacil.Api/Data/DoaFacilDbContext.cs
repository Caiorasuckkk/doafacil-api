using DoaFacil.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DoaFacil.Api.Data;

public class DoaFacilDbContext(DbContextOptions<DoaFacilDbContext> options) : DbContext(options)
{
    public DbSet<PontoColeta> PontosColeta => Set<PontoColeta>();
    public DbSet<Doacao> Doacoes => Set<Doacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PontoColeta>(entity =>
        {
            entity.ToTable("PontosColeta");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Nome).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Endereco).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Cidade).IsRequired().HasMaxLength(80);
            entity.Property(p => p.Uf).IsRequired().HasMaxLength(2).IsFixedLength();
            entity.Property(p => p.Telefone).HasMaxLength(20);
            entity.HasIndex(p => new { p.Nome, p.Cidade }).IsUnique();

            entity.HasMany(p => p.Doacoes)
                  .WithOne(d => d.PontoColeta)
                  .HasForeignKey(d => d.PontoColetaId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasData(
                new PontoColeta
                {
                    Id = 1,
                    Nome = "ONG Mãos Solidárias",
                    Endereco = "Rua das Flores, 120 - Centro",
                    Cidade = "São Paulo",
                    Uf = "SP",
                    Telefone = "(11) 3333-1000",
                    Ativo = true,
                    CriadoEm = new DateTime(2026, 10, 1, 9, 0, 0, DateTimeKind.Utc)
                },
                new PontoColeta
                {
                    Id = 2,
                    Nome = "Paróquia São José",
                    Endereco = "Av. Brasil, 2500 - Jardim América",
                    Cidade = "Campinas",
                    Uf = "SP",
                    Telefone = "(19) 3222-4500",
                    Ativo = true,
                    CriadoEm = new DateTime(2026, 10, 2, 14, 30, 0, DateTimeKind.Utc)
                });
        });

        modelBuilder.Entity<Doacao>(entity =>
        {
            entity.ToTable("Doacoes");
            entity.HasKey(d => d.Id);
            entity.Property(d => d.NomeDoador).IsRequired().HasMaxLength(100);
            entity.Property(d => d.Descricao).IsRequired().HasMaxLength(300);
            entity.Property(d => d.Categoria).HasConversion<string>().HasMaxLength(20);
            entity.Property(d => d.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(d => d.Status);

            entity.HasData(
                new Doacao
                {
                    Id = 1,
                    NomeDoador = "Maria Oliveira",
                    Descricao = "Cestas básicas completas",
                    Categoria = CategoriaDoacao.Alimento,
                    Quantidade = 10,
                    Status = StatusDoacao.Recebida,
                    RecebidaEm = new DateTime(2026, 10, 5, 10, 0, 0, DateTimeKind.Utc),
                    PontoColetaId = 1
                },
                new Doacao
                {
                    Id = 2,
                    NomeDoador = "João Santos",
                    Descricao = "Agasalhos de inverno (adulto)",
                    Categoria = CategoriaDoacao.Roupa,
                    Quantidade = 25,
                    Status = StatusDoacao.EmTriagem,
                    RecebidaEm = new DateTime(2026, 10, 6, 16, 45, 0, DateTimeKind.Utc),
                    PontoColetaId = 2
                });
        });
    }
}
