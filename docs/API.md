# API do catálogo

Base local em desenvolvimento: `http://localhost:5080`. O Angular usa caminhos relativos `/api`; o proxy de desenvolvimento e a publicação na mesma origem dispensam uma URL de serviço no código das telas.

## Listagem

`GET /api/courses`

| Parâmetro opcional | Valores ou regra                                                                                        |
| ------------------ | ------------------------------------------------------------------------------------------------------- |
| `search`           | Até 120 caracteres; busca parcial em nome completo e curto, ignorando caixa, acentos e espaços externos |
| `category`         | `Matemática`, `Ciências da Natureza`, `Sustentabilidade`                                                |
| `modality`         | `Online`, `Presencial`                                                                                  |
| `status`           | `Disponível`, `Em breve`, `Encerrado`                                                                   |
| `featured`         | `true` ou `false`                                                                                       |

Os critérios ativos são combinados com AND. Ausência de critérios retorna o catálogo completo, ordenado por ID. Não há paginação nesta coleção de sete cursos. `total` corresponde à quantidade de elementos de `items`.

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
  "total": 1
}
```

O exemplo representa uma consulta filtrada. A resposta real contém os valores da carga inicial persistida; imagens, textos e datas são demonstrativos. Uma busca sem correspondências retorna **200**, com `items: []` e `total: 0`.

## Detalhes e aulas

`GET /api/courses/{slug}`

Além dos campos da listagem, retorna `about`, `certification`, `modules`, `introduction`, `faqs` e `onlineInformation`.

- Módulo: `title`, `description`, `topics` e `lessons`.
- Aula: `id`, `title`, `description` e `content`.
- FAQ: `question` e `answer`.
- Bloco de texto: `kind`, `text` e `items`. Os tipos utilizados são `paragraph`, `heading`, `list` e `note`.

Os identificadores de aula são estáveis dentro do curso e permitem restaurar o progresso local. Os três itens introdutórios de cada curso também vêm da API. Textos da FAQ, orientação de estudos e informações sobre aulas online são armazenados no banco; o Angular renderiza os blocos como texto, sem inserir HTML recebido da API.

## Erros e disponibilidade

| Situação                                       | Status e corpo                                                  |
| ---------------------------------------------- | --------------------------------------------------------------- |
| Consulta válida, inclusive sem resultados      | 200, JSON                                                       |
| Filtro fora dos valores aceitos ou busca longa | 400, Validation Problem Details                                 |
| Slug ou endpoint de API inexistente            | 404, Problem Details                                            |
| Falha inesperada durante uma requisição        | Tratamento padrão de exceções do ASP.NET Core e Problem Details |
| `GET /api/health` com conexão disponível       | 200, `{ "status": "ok" }`                                       |
| Health check sem conexão disponível            | 503, Problem Details                                            |

O documento OpenAPI está em `/openapi/v1.json`. Não há operações de escrita, autenticação, matrícula, provas ou emissão de certificados. A conclusão demonstrativa de aula é uma interação local do navegador; ela não envia alterações à API.

## Carga e persistência

A aplicação aplica as migrations pendentes ao iniciar. Se o banco ainda não contém cursos, importa `Data/Seed/courses.json` em uma transação. Reinícios preservam o catálogo; editar o JSON após a primeira execução não altera automaticamente um banco existente.

A migration `DescribeIntroductoryLessons` preenche somente as descrições vazias de Introdução, Boas práticas e Extras que também não tenham blocos de conteúdo. Os textos são identificados como “Exemplo fictício”. Conteúdo de referência e descrições já preenchidas são preservados; a atualização também atende bancos criados pela versão anterior.

Para um novo catálogo de demonstração, configure `Database__Path` com um caminho ainda não utilizado e reinicie. A API não oferece edição administrativa. Uma hospedagem com disco temporário recriará o catálogo inicial quando perder o arquivo; um caminho persistente mantém alterações entre reinícios.
