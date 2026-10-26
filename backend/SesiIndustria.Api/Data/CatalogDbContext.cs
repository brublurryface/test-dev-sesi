using Microsoft.EntityFrameworkCore;

namespace SesiIndustria.Api.Data;

/// <summary>
/// Representa a sessão de acesso ao SQLite e o mapeamento entre entidades e tabelas.
/// Program.cs configura a conexão; as migrations registram a evolução deste modelo.
/// </summary>
public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    // DbSet é a porta de entrada para consultas. Tópicos e FAQ também integram o modelo abaixo.
    public DbSet<CourseRecord> Courses => Set<CourseRecord>();
    public DbSet<CourseModuleRecord> Modules => Set<CourseModuleRecord>();
    public DbSet<LessonRecord> Lessons => Set<LessonRecord>();

    /// <summary>Configura nomes de tabelas, chaves, relacionamentos e restrições do catálogo.</summary>
    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<CourseRecord>(course =>
        {
            course.ToTable("Courses", table => table.HasCheckConstraint("CK_Courses_Hours", "Hours > 0"));
            course.HasKey(c => c.Id);
            // IDs dos cursos são fornecidos pela carga inicial, preservando as referências existentes.
            course.Property(c => c.Id).ValueGeneratedNever();
            course.Property(c => c.Slug).HasMaxLength(150);
            course.Property(c => c.Name).HasMaxLength(250);
            course.Property(c => c.ShortName).HasMaxLength(120);
            course.HasIndex(c => c.Slug).IsUnique();
            // Registra o índice composto dos filtros; o plano efetivo depende do provedor e da consulta.
            course.HasIndex(c => new { c.Category, c.Modality, c.Status });
            // As relações são gravadas como chaves estrangeiras, não apenas verificadas na interface.
            course.HasMany(c => c.Modules).WithOne(m => m.Course).HasForeignKey(m => m.CourseId);
            course.HasMany(c => c.Lessons).WithOne(l => l.Course).HasForeignKey(l => l.CourseId);
            course.HasMany(c => c.Faqs).WithOne(f => f.Course).HasForeignKey(f => f.CourseId);
        });
        model.Entity<CourseModuleRecord>(module =>
        {
            module.ToTable("Modules");
            // Dois módulos do mesmo curso não podem ocupar a mesma posição.
            module.HasIndex(m => new { m.CourseId, m.Position }).IsUnique();
            // A FK da aula inclui CourseId: um módulo de outro curso não satisfaz essa relação.
            module.HasAlternateKey(m => new { m.Id, m.CourseId });
            module.HasMany(m => m.Lessons).WithOne(l => l.Module)
                .HasForeignKey(l => new { l.ModuleId, l.CourseId })
                .HasPrincipalKey(m => new { m.Id, m.CourseId });
            module.HasMany(m => m.Topics).WithOne(t => t.Module).HasForeignKey(t => t.ModuleId);
        });
        model.Entity<LessonRecord>(lesson =>
        {
            lesson.ToTable("Lessons");
            // A chave pública da aula pode se repetir em outro curso, mas não dentro do mesmo curso.
            lesson.HasIndex(l => new { l.CourseId, l.Key }).IsUnique();
        });
        model.Entity<TopicRecord>().ToTable("ModuleTopics");
        model.Entity<FaqRecord>().ToTable("CourseFaqs");
    }
}
