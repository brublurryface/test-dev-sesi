# API do catálogo

Base local em desenvolvimento: `http://localhost:5080`. O Angular usa caminhos relativos `/api`; o proxy de desenvolvimento e a publicação na mesma origem dispensam uma URL de serviço no código das telas.

## Listagem

`GET /api/courses`

| Parâmetro opcional | Valores ou regra                                                                                        |
| ------------------ | ------------------------------------------------------------------------------------------------------- |
| `search`           | Até 120 caracteres; busca parcial em nome completo e curto, ignorando caixa, acentos e espaços externos |
| `category`         | Um dos valores de `filters.categories` presentes no banco                                               |
| `modality`         | Um dos valores de `filters.modalities` presentes no banco                                               |
| `status`           | Um dos valores de `filters.statuses` presentes no banco                                                 |
| `featured`         | `true` ou `false`                                                                                       |
| `page`             | Inteiro maior ou igual a 1; padrão 1. O deslocamento calculado deve caber em um inteiro de 32 bits       |
| `pageSize`         | Inteiro de 1 a 100; padrão 20                                                                          |

Os critérios ativos são combinados com AND. `total` conta todos os cursos compatíveis com os filtros; `items` contém somente a página solicitada, ordenada por ID. A contagem usa `CountAsync()` antes de `Skip()`/`Take()`, sem materializar a coleção completa. `page`, `pageSize` e `totalPages` descrevem a navegação; um total zero corresponde a zero páginas. Sem filtros e sem parâmetros de paginação, o pedido retorna os primeiros 20 cursos.

As três listas de `filters` vêm dos valores distintos do catálogo inteiro, ordenados no banco. Não são reduzidas pela busca nem pelos demais filtros. Assim, novas categorias, modalidades e status persistidos aparecem sem alterar a API ou o Angular; uma combinação sem resultados não apaga as opções. Não há cache dessas listas. Em um catálogo vazio, as três listas também estão vazias.

```json
{
  "items": [
    {
      "id": 1,
      "slug": "matematica-para-a-precisao",
      "name": "Matemática para a Precisão: Medidas e Escalas na Metalurgia",
      "shortName": "Matemática para a Precisão",
      "description": "Aplicação de grandezas, proporções e escalas em processos de corte, furação e usinagem.",
      "category": "Matemática",
      "modality": "Online",
      "hours": 40,
      "audience": "Estudantes, operadores e profissionais da metalurgia que desejam desenvolver conhecimentos sobre medidas e escalas.",
      "status": "Disponível",
      "image": "assets/banner-ava.png",
      "imageAlt": "Paquímetro medindo uma peça metálica",
      "availableAt": "2026-01-05",
      "featured": true
    }
  ],
  "total": 1,
  "filters": {
    "categories": ["Ciências da Natureza", "Matemática", "Sustentabilidade"],
    "modalities": ["Online", "Presencial"],
    "statuses": ["Disponível", "Em breve", "Encerrado"]
  },
  "page": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

O exemplo representa uma consulta filtrada na carga inicial. A resposta real reflete o banco; imagens, textos e datas são demonstrativos. Uma busca sem correspondências retorna **200**, com `items: []`, `total: 0` e as opções do catálogo em `filters`. Um valor inexistente ou removido do banco continua retornando **400**; não é convertido silenciosamente em ausência de filtro.

Uma página válida além da última retorna **200** com `items: []`, preservando `total`, `totalPages`, `page` solicitado e opções dos filtros. O catálogo oferece voltar à primeira página sem perder os filtros. Parâmetros de paginação fora dos limites, textos em campos numéricos e deslocamentos com overflow retornam **400**.

Exemplo: `/api/courses?page=2&pageSize=20&category=Matem%C3%A1tica`. No Angular, a página aparece como `pagina` na URL pública e volta a 1 ao alterar ou limpar filtros. Destaques e rodapé solicitam suas próprias primeiras páginas, limitadas a seis e sete cursos; não percorrem o catálogo completo.

O contrato usa paginação por deslocamento. Ordenação única evita ambiguidades, mas alterações concorrentes podem deslocar resultados entre pedidos. Páginas profundas e contagem continuam tendo custo no banco; índices, consultas de busca e paginação por cursor devem ser avaliados para grandes volumes. Referência: [paginação no EF Core](https://learn.microsoft.com/en-us/ef/core/querying/pagination).

## Detalhes e aulas

`GET /api/courses/{slug}`

Além dos campos da listagem, retorna `about`, `certification`, `modules`, `introduction`, `faqs` e `onlineInformation`.

- Módulo: `title`, `description`, `topics` e `lessons`.
- Aula no índice: `id`, `title` e `description`; a descrição é uma prévia de até 240 caracteres, seguida de reticências quando cortada. Não inclui `content`.
- FAQ: `question` e `answer`.
- Bloco de texto: `kind`, `text` e `items`. Os tipos utilizados são `paragraph`, `heading`, `list` e `note`.

Os identificadores de aula são estáveis dentro do curso e permitem restaurar o progresso local. Os três itens introdutórios de cada curso também vêm da API. Textos da FAQ, orientação de estudos e informações sobre aulas online são armazenados no banco; o Angular renderiza os blocos como texto, sem inserir HTML recebido da API.

`GET /api/courses/{slug}/lessons/{lessonId}`

Retorna somente a aula solicitada, com `id`, `title`, `description` completa e `content`. A busca combina o slug do curso e a chave da aula: uma chave como `m1-a1` pode existir em outros cursos sem compartilhar seu conteúdo. A ordem dos módulos e das aulas permanece no índice; progresso, busca local, retomada e próxima aula usam esse índice.

```json
{
  "id": "m1-a1",
  "title": "Aula 1",
  "description": "Exemplo fictício: descrição demonstrativa da aula.",
  "content": []
}
```

O exemplo demonstra uma aula fictícia sem blocos adicionais; a resposta real vem do SQLite. Curso ou aula inexistente retorna **404**, com Problem Details. O endpoint do índice usa projeção sem ler a coluna `Lessons.ContentJson`; a rota de aula lê somente o registro correspondente.

**Mudança de contrato:** `modules[].lessons[]` e `introduction[]` deixaram de conter `content`. Clientes que utilizavam esse campo devem consultar a rota de aula. Frontend e API atualizados devem ser publicados juntos. O formato completo de `Data/Seed/courses.json` é preservado e lido pelo tipo separado `CatalogSeedCourse`; o banco não exige nova migration.

O cache é do cliente e dura somente enquanto a AVA está aberta: até dez entradas e 2 MiB de JSON serializado, por chave composta de curso e aula. A entrada menos recentemente acessada é removida ao atingir os limites; uma aula maior pode ser exibida, mas não fica em cache. Recarregar o índice ou sair da AVA limpa o cache. Não há política de cache HTTP adicional nem sincronização de conteúdo em tempo real nesta etapa.

## Erros e disponibilidade

| Situação                                       | Status e corpo                                                  |
| ---------------------------------------------- | --------------------------------------------------------------- |
| Consulta válida, inclusive sem resultados      | 200, JSON                                                       |
| Filtro fora dos valores aceitos ou busca longa | 400, Validation Problem Details                                 |
| Página/tamanho inválido ou parâmetro sem conversão | 400, Problem Details                                           |
| Slug ou endpoint de API inexistente            | 404, Problem Details                                            |
| Falha inesperada durante uma requisição        | Tratamento padrão de exceções do ASP.NET Core e Problem Details |
| `GET /api/health` com conexão disponível       | 200, `{ "status": "ok" }`                                       |
| Health check sem conexão disponível            | 503, Problem Details                                            |

O documento OpenAPI está em `/openapi/v1.json`. Não há operações de escrita, autenticação, matrícula, provas ou emissão de certificados. A conclusão demonstrativa de aula é uma interação local do navegador; ela não envia alterações à API.

## Carga e persistência

A aplicação aplica as migrations pendentes ao iniciar. Se o banco ainda não contém cursos, importa `Data/Seed/courses.json` em uma transação. Reinícios preservam o catálogo; editar o JSON após a primeira execução não altera automaticamente um banco existente.

A migration `DescribeIntroductoryLessons` preenche somente as descrições vazias de Introdução, Boas práticas e Extras que também não tenham blocos de conteúdo. Os textos são identificados como “Exemplo fictício”. Conteúdo de referência e descrições já preenchidas são preservados; a atualização também atende bancos criados pela versão anterior.

Para um novo catálogo de demonstração, configure `Database__Path` com um caminho ainda não utilizado e reinicie. A API não oferece edição administrativa. Uma hospedagem com disco temporário recriará o catálogo inicial quando perder o arquivo; um caminho persistente mantém alterações entre reinícios.
