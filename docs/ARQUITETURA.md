# Arquitetura e mapa do código

Este documento localiza os pontos de entrada, as responsabilidades dos arquivos e o percurso dos dados entre Angular, API e SQLite. Os comentários no código detalham as decisões próximas de sua implementação.

## Onde começar

| Responsabilidade                    | Arquivo                                                                         | O que localizar                                                  |
| ----------------------------------- | ------------------------------------------------------------------------------- | ---------------------------------------------------------------- |
| Iniciar o backend                   | [Program.cs](../backend/SesiIndustria.Api/Program.cs)                           | Configuração, registro de serviços, banco, endpoints e servidor  |
| Validar os filtros                  | [CourseQuery.cs](../backend/SesiIndustria.Api/Courses/CourseQuery.cs)           | Opções aceitas, limite da busca e normalização de texto          |
| Consultar o catálogo                | [CourseRepository.cs](../backend/SesiIndustria.Api/Courses/CourseRepository.cs) | `ListAsync` para resumos e `FindAsync` para detalhes             |
| Definir as respostas HTTP           | [CourseContracts.cs](../backend/SesiIndustria.Api/Contracts/CourseContracts.cs) | DTOs de cursos, módulos, aulas e blocos de texto                 |
| Representar registros persistidos   | [Entities.cs](../backend/SesiIndustria.Api/Data/Entities.cs)                    | Entidades, chaves estrangeiras e relações de navegação           |
| Configurar tabelas e restrições     | [CatalogDbContext.cs](../backend/SesiIndustria.Api/Data/CatalogDbContext.cs)    | Mapeamento do EF Core, índices e integridade dos relacionamentos |
| Importar o catálogo inicial         | [CatalogSeeder.cs](../backend/SesiIndustria.Api/Data/CatalogSeeder.cs)          | Validação da fonte, transação e preservação dos dados existentes |
| Verificar o comportamento integrado | [CatalogApiTests.cs](../backend/SesiIndustria.Api.Tests/CatalogApiTests.cs)     | Requisições HTTP com bancos SQLite temporários                   |

`TestDevSesiIndustria.slnx` reúne a API e seus testes. O arquivo `SesiIndustria.Api.csproj` define o alvo .NET e as dependências do backend; `global.json` fixa o SDK de referência.

## Inicialização do backend

O `Program.cs` usa instruções de nível superior como ponto de entrada. Os blocos comentados seguem esta ordem:

1. **Configuração e serviços:** `CreateBuilder(args)` lê a configuração; `builder.Services` registra os recursos que a aplicação poderá receber por injeção de dependência.
2. **Persistência:** o caminho do arquivo SQLite compõe a conexão. `CatalogDbContext` e `CourseRepository` são registrados com escopo de requisição.
3. **Aplicação:** `builder.Build()` cria a aplicação; os tratamentos comuns de erro são configurados.
4. **Banco inicial:** um escopo próprio aplica as migrations e chama o seeder antes de atender requisições.
5. **Endpoints:** cada `MapGet` associa um caminho e o método HTTP GET à função que processa a consulta.
6. **Frontend publicado:** quando existe `wwwroot/index.html`, a API também serve os arquivos Angular e o fallback das rotas de tela.
7. **Servidor:** `app.Run()` inicia o atendimento e mantém o processo em execução.

O escopo de requisição mantém contexto e repositório disponíveis durante um pedido e permite seu descarte ao final. A inicialização do banco ocorre antes dos pedidos e, por isso, abre um escopo separado com `CreateAsyncScope()`.

## Percurso de uma consulta

Para uma chamada `GET /api/courses?search=matematica`:

1. O endpoint em `Program.cs` recebe `search` da URL e recebe `CourseRepository` pela injeção de dependência.
2. `CourseQuery.Validate()` verifica a entrada. Erros produzem HTTP 400, com mensagens por parâmetro.
3. `CourseRepository.ListAsync()` normaliza o termo e acrescenta os critérios à consulta. Os filtros preenchidos são combinados por AND.
4. O EF Core traduz a consulta e seus valores em SQL parametrizado. `ToArrayAsync()` executa a leitura no SQLite.
5. O repositório retorna `CoursePageDto`, com resumos e contagem; `Results.Ok()` produz HTTP 200 e a resposta JSON.
6. `CourseService` recebe o JSON no Angular; `CourseListStore` atualiza dados, contagem e carregamento; o componente apresenta os cartões.

Uma consulta válida sem correspondência retorna `items: []` e `total: 0`. Um slug inexistente em `/api/courses/{slug}` retorna 404. Os estados de carregamento, ausência de resultado e falha são tratados separadamente na interface.

### Listagem e detalhes

| Contrato C#        | Tipo Angular    | Uso                                                             |
| ------------------ | --------------- | --------------------------------------------------------------- |
| `CoursePageDto`    | `CoursePage`    | Resumos e contagem da consulta atual, sem paginação             |
| `CourseSummaryDto` | `CourseSummary` | Cartões do catálogo e destaques                                 |
| `CourseDto`        | `Course`        | Detalhes completos do curso e conteúdo da AVA                   |
| `LessonDto`        | `Lesson`        | Identificador estável, título, descrição e conteúdo de uma aula |
| `ContentBlockDto`  | `ContentBlock`  | Parágrafos, títulos, listas e notas renderizados como texto     |

DTOs definem o contrato de transporte; entidades definem a persistência. A listagem projeta apenas o resumo, enquanto os detalhes carregam os relacionamentos necessários. `AsNoTracking()` evita o acompanhamento de alterações em consultas de leitura. `AsSplitQuery()` evita multiplicar as linhas ao carregar várias coleções relacionadas nos detalhes.

Os métodos assíncronos recebem o `CancellationToken` do pedido HTTP e o encaminham ao EF Core. Isso permite sinalizar interrupção do trabalho quando a requisição é cancelada.

## Banco, esquema e carga inicial

| Tabela         | Entidade             | Relação principal                                         |
| -------------- | -------------------- | --------------------------------------------------------- |
| `Courses`      | `CourseRecord`       | Registro principal do catálogo                            |
| `Modules`      | `CourseModuleRecord` | Módulo pertencente a um curso                             |
| `Lessons`      | `LessonRecord`       | Aula pertencente a um curso e, opcionalmente, a um módulo |
| `ModuleTopics` | `TopicRecord`        | Tópico pertencente a um módulo                            |
| `CourseFaqs`   | `FaqRecord`          | Pergunta e resposta pertencentes a um curso               |

`CatalogDbContext.OnModelCreating()` define o modelo relacional. Os arquivos em `Data/Migrations/` registram sua evolução e são mantidos pelo EF Core. O arquivo `Data/initial-schema.sql` permite inspecionar o esquema inicial; a execução normal usa as migrations automaticamente.

O banco impõe slugs únicos, horas positivas e integridade das relações. A chave estrangeira de aula para módulo inclui o curso: uma aula não pode apontar para um módulo de outro curso. A posição ordena módulos, tópicos, aulas e perguntas; a chave pública da aula permanece estável para permitir recuperar o progresso local.

`Data/Seed/courses.json` é a fonte da primeira carga. O seeder verifica a estrutura e importa os registros em uma transação. Se já existe algum curso no banco, a importação é ignorada. Alterar esse JSON depois da carga não atualiza os registros existentes; as consultas HTTP leem o SQLite.

Atualizações específicas de dados existentes também podem ser versionadas por migration. `DescribeIntroductoryLessons` preenche as descrições demonstrativas dos itens introdutórios ainda vazios, preservando conteúdo de referência e textos já preenchidos. A carga inicial contém esses mesmos textos para bancos novos.

Os blocos de texto de aulas e orientações são persistidos em colunas JSON, mantendo curso, módulo, aula, tópico e FAQ como entidades relacionais. O repositório converte esses blocos em DTOs; o Angular renderiza seu texto por interpolação.

## Onde acompanhar no frontend

| Arquivo                                                                   | Responsabilidade                                              |
| ------------------------------------------------------------------------- | ------------------------------------------------------------- |
| [main.ts](../frontend/src/main.ts)                                        | Inicialização Angular, cliente HTTP e navegação               |
| [app.routes.ts](../frontend/src/app/app.routes.ts)                        | Rotas de tela e carregamento sob demanda                      |
| [course.model.ts](../frontend/src/app/core/course.model.ts)               | Tipos correspondentes às respostas JSON da API                |
| [course.service.ts](../frontend/src/app/core/course.service.ts)           | Chamadas HTTP de listagem e detalhes                          |
| [course-list.store.ts](../frontend/src/app/core/course-list.store.ts)     | Estado da listagem, cancelamento e nova tentativa             |
| [course-detail.store.ts](../frontend/src/app/core/course-detail.store.ts) | Estado de um curso, distinguindo 404 e falha                  |
| [catalog.ts](../frontend/src/app/pages/catalog/catalog.ts)                | Controles, URL e solicitação dos filtros ao servidor          |
| [learning.ts](../frontend/src/app/pages/learning/learning.ts)             | Seleção, busca e conclusão demonstrativa de aulas             |
| [learning-progress.ts](../frontend/src/app/core/learning-progress.ts)     | Restauração dos identificadores e cálculo do percentual local |

O serviço HTTP é compartilhado e não guarda os resultados. Cada componente que consulta o catálogo fornece sua própria instância do store, para não misturar os filtros e resultados do início, do catálogo e do rodapé. Os signals públicos são de leitura; os métodos do store coordenam as atualizações.

No catálogo, os parâmetros públicos `busca`, `categoria` e `modalidade` são convertidos para `search`, `category` e `modality` no contrato HTTP. A digitação aguarda uma pausa de 250 ms; os seletores consultam imediatamente. Uma consulta nova cancela a inscrição anterior para evitar que uma resposta atrasada substitua a seleção atual.

A busca de cursos ocorre no servidor. A busca de aulas da AVA ocorre no navegador, sobre o conteúdo do curso já recebido. A conclusão de aulas também é local: usa uma chave de armazenamento por curso, pode ser desfeita e não representa matrícula, avaliação ou progresso sincronizado.

Ao abrir um curso, `findInitialLesson()` escolhe a primeira aula ainda não concluída na ordem recebida da API. O componente usa os identificadores restaurados sem acompanhar o signal de conclusão nesse efeito, para que marcar uma aula não altere a seleção durante a leitura. Um curso já concluído abre na primeira aula para revisão; o grupo da aula retomada fica expandido.

## Configurações públicas e dados variáveis

[site.config.ts](../frontend/src/app/core/site.config.ts) centraliza o nome do projeto usado nos títulos das rotas, o e-mail de contato, o curso de referência da demonstração e o limite de busca da interface. São configurações compiladas com o frontend; os dados das ofertas permanecem no banco e na API. `CourseQuery.MaxSearchLength` mantém a validação correspondente no servidor.

Nos detalhes, o header recebe o slug do curso atual por input para montar o link da área do aluno. Na página inicial e no catálogo, usa o curso de referência definido na configuração. O rodapé consulta os resumos da API e monta os links a partir de `slug` e `shortName`, com estado independente dos filtros da página.

## Publicação

[scripts/publish.mjs](../scripts/publish.mjs) compila o Angular, restaura as dependências .NET com lockfiles, publica a API e copia o frontend para `wwwroot`. O pacote usa uma origem para telas e API; no desenvolvimento, `frontend/proxy.conf.json` encaminha `/api` para o backend.

`artifacts/publish` é uma saída gerada e seu conteúdo anterior é removido pelo build. Um banco que precise sobreviver a novos builds deve ficar fora desse diretório, com caminho configurado por `Database__Path`. Os requisitos e os comandos estão em [PUBLICACAO.md](PUBLICACAO.md).
