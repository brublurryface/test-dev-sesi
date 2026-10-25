using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SesiIndustria.Api.Data.Migrations
{
    /// <summary>
    /// Preenche somente os itens introdutórios vazios da versão inicial do catálogo.
    /// Não substitui os textos de referência nem conteúdo já preenchido no banco.
    /// </summary>
    public partial class DescribeIntroductoryLessons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Bancos novos recebem as descrições pelo seeder; bancos existentes usam esta atualização.
            // As condições preservam tanto descrições próprias quanto blocos de conteúdo já presentes.
            migrationBuilder.Sql("""
                UPDATE "Lessons"
                SET "Description" = 'Exemplo fictício: apresentação do curso e de sua organização em módulos e aulas.'
                WHERE "ModuleId" IS NULL AND "Key" = 'introducao'
                    AND "Description" = '' AND "ContentJson" = '[]';

                UPDATE "Lessons"
                SET "Description" = 'Exemplo fictício: orientações gerais para organizar os estudos e acompanhar as aulas.'
                WHERE "ModuleId" IS NULL AND "Key" = 'boas-praticas'
                    AND "Description" = '' AND "ContentJson" = '[]';

                UPDATE "Lessons"
                SET "Description" = 'Exemplo fictício: espaço de materiais complementares para consulta ao longo do curso.'
                WHERE "ModuleId" IS NULL AND "Key" = 'extras'
                    AND "Description" = '' AND "ContentJson" = '[]';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // A reversão remove apenas os textos desta atualização que ainda permaneçam intactos.
            migrationBuilder.Sql("""
                UPDATE "Lessons" SET "Description" = ''
                WHERE "ModuleId" IS NULL AND "Key" = 'introducao' AND "ContentJson" = '[]'
                    AND "Description" = 'Exemplo fictício: apresentação do curso e de sua organização em módulos e aulas.';

                UPDATE "Lessons" SET "Description" = ''
                WHERE "ModuleId" IS NULL AND "Key" = 'boas-praticas' AND "ContentJson" = '[]'
                    AND "Description" = 'Exemplo fictício: orientações gerais para organizar os estudos e acompanhar as aulas.';

                UPDATE "Lessons" SET "Description" = ''
                WHERE "ModuleId" IS NULL AND "Key" = 'extras' AND "ContentJson" = '[]'
                    AND "Description" = 'Exemplo fictício: espaço de materiais complementares para consulta ao longo do curso.';
                """);
        }
    }
}
