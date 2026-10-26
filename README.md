![SESI Indústria](frontend/public/assets/logo-sesi-industria-primary.svg)

# TEST DEV SESI Indústria

Catálogo de cursos fictícios desenvolvido para a atividade prática de Desenvolvedor de Sistemas I — 519/2026. Angular apresenta os cursos fornecidos por uma API C#/.NET com persistência em SQLite. A interface segue as referências da landing page, dos detalhes de curso e da área de aprendizagem (AVA).

**Estado desta versão:** frontend, API e banco integrados, com demonstração publicada no plano gratuito do Render e validada por HTTP e no navegador.

**Repositório:** [brublurryface/test-dev-sesi](https://github.com/brublurryface/test-dev-sesi).

**Demonstração:** [test-dev-sesi.onrender.com](https://test-dev-sesi.onrender.com/). Após inatividade, o primeiro acesso pode levar aproximadamente um minuto para reativar a instância gratuita.

## Executar localmente

Requisitos: Node.js 24.19.0, npm 11 e SDK .NET 10.0.402, ou versões compatíveis com as configurações do projeto. O arquivo `global.json` fixa a versão de referência do SDK. O SQLite funciona como arquivo local; não exige instalação de um servidor, conta ou chave.

Na raiz do projeto:

```bash
npm --prefix frontend ci
dotnet restore TestDevSesiIndustria.slnx --locked-mode
```

Em um terminal, inicie a API:

```bash
dotnet run --project backend/SesiIndustria.Api
```

Em outro terminal, inicie o frontend:

```bash
npm --prefix frontend start
```

Abra `http://localhost:4200`. A API usa `http://localhost:5080`; o proxy do Angular encaminha `/api` a esse endereço. A aplicação aplica as migrations pendentes e importa o catálogo inicial quando ainda não há cursos. Nas seguintes execuções, preserva os dados existentes.

## Executar o pacote integrado

Após instalar as dependências, execute na raiz:

```bash
npm run build
cd artifacts/publish
dotnet SesiIndustria.Api.dll --urls http://localhost:5080
```

Abra `http://localhost:5080`. O ASP.NET Core entrega o Angular e a API na mesma origem. As rotas do frontend podem ser abertas ou recarregadas diretamente; recursos inexistentes da API continuam respondendo 404.

O diretório `artifacts/publish` contém a publicação executável e o catálogo inicial. O **SDK .NET 10** inclui os runtimes necessários para executá-lo. Em uma instalação somente de runtimes, são necessários **.NET Runtime 10 e ASP.NET Core Runtime 10**. O build não publica o aplicativo em um serviço de hospedagem. Orientações adicionais estão em [docs/PUBLICACAO.md](docs/PUBLICACAO.md).

## Funcionalidades

- Landing page com seis cursos em destaque, carrosséis e seções do protótipo.
- Catálogo de sete cursos, três categorias, duas modalidades e três status.
- Busca no servidor por nome completo ou curto, sem distinção entre acentos ou caixa.
- Filtros combináveis por categoria, modalidade e status; contagem de resultados e limpeza.
- Seleção preservada na URL, com atualização da listagem sem retorno ao topo.
- Detalhes, módulos, aulas, descrições e textos do curso consultados na API.
- AVA com busca de aulas, avanço e conclusão demonstrativa; progresso salvo neste navegador.
- Abertura na primeira aula pendente, respeitando a ordem do curso e o progresso salvo.
- Área do aluno vinculada ao curso exibido nos detalhes e links de cursos do rodapé fornecidos pela API.
- Estados de carregamento, erro com nova tentativa, resultado vazio e curso não encontrado.

## Organização e fluxo de dados

O [mapa da arquitetura](docs/ARQUITETURA.md) indica os arquivos por responsabilidade, detalha a inicialização do backend e acompanha uma consulta do endpoint até a atualização da tela. Para começar a leitura pelo backend, abra [Program.cs](backend/SesiIndustria.Api/Program.cs) e siga os blocos numerados.

```text
backend/
  SesiIndustria.Api/
    Contracts/           contratos das respostas HTTP
    Courses/             validação da consulta e acesso ao catálogo
    Data/                entidades, contexto, migration e carga inicial
    Program.cs           configuração e endpoints
  SesiIndustria.Api.Tests/ testes de integração HTTP com SQLite real
frontend/
  public/assets/         imagens, ícones e fonte fornecidos no teste
  src/app/core/          contratos HTTP, stores e progresso local
  src/app/pages/         início, catálogo, detalhes, AVA e página 404
  src/app/shared/        componentes e comportamentos reutilizáveis
scripts/publish.mjs      montagem do pacote integrado
docs/                   decisões, API, publicação e validação
Dockerfile              compilação e execução integradas em contêiner
USO_DE_IA.md             registro de uso de inteligência artificial
```

O fluxo é **Angular → HTTP `/api` → ASP.NET Core → Entity Framework Core → SQLite**. O JSON em `Data/Seed/courses.json` serve apenas à carga inicial do banco. O frontend não lê esse arquivo nem utiliza uma coleção local como alternativa à API.

As páginas Angular são standalone e carregadas sob demanda. Signals representam os estados da tela; stores por página cancelam requisições substituídas e distinguem carregamento, falha e ausência de resultado. A busca aguarda 250 ms após a digitação; os seletores enviam a consulta imediatamente. Os comentários documentam responsabilidades, contratos e decisões junto à implementação, incluindo escopos de dependências, execução das consultas, carga inicial e limites do progresso local.

## API e banco

| Método e caminho          | Resposta                                                                           |
| ------------------------- | ---------------------------------------------------------------------------------- |
| `GET /api/courses`        | `{ items, total }`; aceita `search`, `category`, `modality`, `status` e `featured` |
| `GET /api/courses/{slug}` | Curso com módulos, aulas, introdução, FAQ e blocos de texto                        |
| `GET /api/health`         | Estado da conexão com o banco                                                      |
| `GET /openapi/v1.json`    | Descrição OpenAPI dos endpoints                                                    |

Exemplo de consulta com os critérios combinados:

```text
/api/courses?search=matematica&category=Matem%C3%A1tica&modality=Online&status=Dispon%C3%ADvel
```

O arquivo padrão é `backend/SesiIndustria.Api/App_Data/catalog.db` durante o desenvolvimento e `App_Data/catalog.db` dentro da publicação. A configuração `Database:Path`, ou a variável de ambiente `Database__Path`, permite escolher outro caminho. O diretório precisa permitir escrita.

| Entidade     | Relações e dados principais                                                                                          |
| ------------ | -------------------------------------------------------------------------------------------------------------------- |
| Curso        | Nome completo/curto, descrição, apresentação, categoria, modalidade, horas, público, status, imagem, data e destaque |
| Módulo       | Pertence a um curso; posição, título, descrição e tópicos                                                            |
| Aula         | Pertence a um curso e, quando aplicável, a um módulo; identificador estável, título, descrição e conteúdo            |
| Tópico e FAQ | Conteúdos ordenados associados ao módulo ou curso                                                                    |

O banco impõe slugs únicos, horas positivas e integridade das relações. Uma aula não pode apontar para um módulo de outro curso. A carga contém 28 módulos e 49 registros de aulas: uma Aula 1 por módulo e três itens de introdução por curso. As descrições das 28 aulas dos módulos e dos 20 itens introdutórios sem texto de referência são exemplos fictícios identificados na interface. O texto de Boas práticas do curso de Matemática para a Precisão preserva a referência fornecida. Datas são valores de calendário `YYYY-MM-DD`, apresentados com UTC explícito para preservar o dia.

A migration está em `Data/Migrations`; `Data/initial-schema.sql` contém o esquema inicial para inspeção. A aplicação usa a migration e a carga JSON automaticamente, sem exigir execução manual do SQL. Consulte [docs/API.md](docs/API.md) e [docs/DECISOES_TECNICAS.md](docs/DECISOES_TECNICAS.md).

## Verificações

```bash
npm --prefix frontend test
npm --prefix frontend run format:check
dotnet test TestDevSesiIndustria.slnx --no-restore
npm run build
```

Os testes do frontend verificam busca de aulas e consistência do progresso. Os testes de integração exercitam requisições HTTP, migrations e consultas em arquivos SQLite temporários: combinação de filtros, respostas vazias e inválidas, conteúdos, integridade e persistência após reinício. O alcance da revisão pelo navegador está em [docs/VALIDACAO.md](docs/VALIDACAO.md).

## Responsividade e acessibilidade

- Grades fluidas, menu para telas estreitas e painel móvel de conteúdo da AVA.
- Labels, foco visível, links para pular ao conteúdo e mensagens de estado com regiões vivas.
- Controles nativos `details`/`summary` e `dialog`, com navegação por teclado.
- Imagens informativas com texto alternativo e ícones decorativos ignorados por leitores de tela.
- Animação compartilhada das setas e carrosséis com respeito à preferência de redução de movimento.

Essas medidas não equivalem a uma certificação de conformidade. Os limites da verificação estão documentados.

## Diferenças e limitações conhecidas

- O acesso automatizado ao Figma não ficou disponível; as referências foram as capturas fornecidas pela Bruna e os anexos do teste. Medidas e comportamento responsivo foram reconstruídos a partir delas.
- O pacote contém três fotos. A imagem de medição foi reaproveitada no primeiro cartão; os demais cartões usam provisoriamente as fotos disponíveis, pois as imagens individuais do carrossel não vieram nos anexos.
- Não foi fornecido um arquivo ou endereço de vídeo. Os controles de reprodução abrem um aviso de indisponibilidade.
- Bruna confirmou que os botões “Todos os cursos” não possuem destino clicável no protótipo. A tela de catálogo foi construída com a mesma linguagem visual para atender aos requisitos de busca e filtros do enunciado; nesta implementação, os botões “Todos os cursos” e os acessos gerais ao catálogo abrem essa tela.
- Carga horária, modalidade, público, status e datas não fornecidas nas referências usam valores demonstrativos para os campos exigidos pelo enunciado. Esses valores são dados de teste e não descrevem ofertas reais.
- Os títulos e as descrições dos seis cursos em destaque reproduzem as capturas fornecidas. O catálogo inclui também o curso complementar de Gestão de Resíduos para contemplar a terceira categoria exigida no enunciado.
- Os dez módulos do primeiro curso reproduzem os títulos e tópicos das capturas. Nos demais cursos, a estrutura de navegação usa identificadores neutros: “Módulo 1”, “Módulo 2” etc., com uma “Aula 1” sem conteúdo didático por módulo. A quantidade de módulos dessas ofertas é demonstrativa. As apresentações dos outros cinco cursos em destaque reutilizam suas descrições curtas fornecidas nas capturas.
- A FAQ e o painel de aulas/avaliações são exibidos apenas no curso de referência. Os textos de “Boas práticas” e “Recomendações” da AVA desse curso foram transcritos das capturas. Na AVA, os módulos usam os rótulos “Módulo 1”, “Módulo 2” etc.; cada módulo contém somente “Aula 1”, com uma breve descrição identificada como “Exemplo fictício”. Os rótulos de introdução fornecidos na referência foram preservados; os itens sem texto também recebem uma descrição fictícia breve para demonstrar a navegação.
- Conforme a navegação do protótipo informada pela Bruna, “Matricule-se” no bloco vermelho da landing page abre os detalhes de Matemática para a Precisão; o botão homônimo da faixa preta fixa nos detalhes abre diretamente a AVA do curso selecionado. Cursos em breve ou encerrados mantêm o controle da faixa desabilitado. O acesso é demonstrativo e não efetiva matrícula. O botão do início dos detalhes também oferece acesso à demonstração por um aviso.
- As cópias que pressupunham provas, biometria ou certificação oficial não foram tratadas como funcionalidades disponíveis.
- Itens institucionais fora do escopo abrem um aviso. Os links de cursos do rodapé foram adaptados aos sete cursos existentes.
- Indicadores, depoimentos e dados de contato reproduzem conteúdo ilustrativo da referência. O projeto não é um serviço oficial de cursos.
- As descrições das aulas são exemplos fictícios para demonstrar o campo previsto no item 4(b) do enunciado. Reutilizam o primeiro tópico fornecido de cada módulo de Matemática e as descrições curtas dos cartões nas demais ofertas. A associação desses textos à “Aula 1” é demonstrativa e não representa uma grade de aulas fornecida pelo teste.
- O botão “Marcar como concluída” foi acrescentado à AVA e não faz parte do Figma. Ele demonstra a conclusão de uma aula e a atualização do progresso, seguindo uma interação comum em cursos online. A marcação pode ser desfeita e o progresso pode ser reiniciado. Os dados ficam apenas neste navegador, sem sincronização com o servidor ou efeito de matrícula, avaliação ou certificação.
- A demonstração está publicada no plano gratuito do Render, documentado em [docs/PUBLICACAO.md](docs/PUBLICACAO.md), que suspende a instância após inatividade e usa armazenamento temporário. O catálogo SQLite é reconstruído com as migrations e a carga inicial quando o arquivo é perdido; alterações manuais no banco não são conservadas nessas condições. O progresso demonstrativo continua no navegador. O pacote integrado também pode ser executado localmente com as instruções deste README.

## Uso de IA

O desenvolvimento contou com a assistência do **agente Nemo, do ChatGPT/Codex**, na análise de requisitos, geração e adaptação de código, testes e documentação. Bruna forneceu o enunciado, os arquivos de apoio, as referências visuais e as orientações de interação. O registro das contribuições, adaptações e verificações está em [USO_DE_IA.md](USO_DE_IA.md).
