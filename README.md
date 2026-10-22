![SESI Indústria](frontend/public/assets/logo-sesi-industria-primary.svg)

# TEST DEV SESI Indústria

Catálogo de cursos fictícios desenvolvido para a atividade prática de Desenvolvedor de Sistemas I — 519/2026. A interface segue as referências visuais fornecidas para a landing page, os detalhes de um curso e a área de aprendizagem (AVA).

**Estado desta versão:** primeira etapa de frontend, com dados demonstrativos em JSON. O backend C#/.NET, o banco de dados, a integração real e a publicação ainda não foram implementados. Esta versão não representa a entrega completa do teste.

## Executar localmente

Requisitos: Node.js 24.19.0 e npm 11 ou versões compatíveis com Angular 22. Para reproduzir o ambiente usado nesta etapa:

```bash
cd frontend
npm ci
npm start
```

Abra `http://localhost:4200`. Não é necessário criar contas, configurar chaves ou instalar banco de dados nesta etapa.

```bash
npm test
npm run build
npm run format:check
```

O build estático é gerado em `frontend/dist/test-dev-sesi-industria/browser`. O servidor de hospedagem deverá redirecionar as rotas da aplicação para `index.html`, preservando os arquivos existentes. Nenhuma hospedagem foi configurada nesta versão.

A formatação segue a configuração do Prettier incluída no projeto. Para aplicá-la após alterações, execute `npm run format`.

## Funcionalidades disponíveis

- Landing page com carrosséis, indicadores e seções do protótipo.
- Catálogo de sete cursos, três categorias, duas modalidades e três status; seis cursos em destaque na landing page.
- Busca por nome sem distinção entre acentos ou maiúsculas e minúsculas.
- Filtros combináveis por categoria, modalidade e status; contagem de resultados e limpeza dos filtros.
- Parâmetros de busca preservados na URL, permitindo recarregar ou compartilhar uma seleção.
- Detalhes com carga horária, público, status, modalidade, disponibilidade, módulos e aulas.
- Painéis independentes para módulos, aulas/avaliações e dúvidas frequentes no curso de referência.
- AVA demonstrativa com busca de aulas e progresso salvo apenas no navegador, sem conta ou matrícula.
- Estados de carregamento, falha de rede, resultado vazio e endereço não encontrado.

## Tecnologias e organização

Angular 22, TypeScript 6, CSS e recursos nativos de HTML. Os componentes são standalone; o estado usa signals, com listas derivadas por `computed` e carregamento via `HttpClient`. As páginas são carregadas sob demanda pelo Angular Router.

```text
frontend/
  public/assets/       imagens, ícones e fonte fornecidos no teste
  public/data/         catálogo demonstrativo
  src/app/core/        modelo, carregamento e filtros de cursos
  src/app/pages/       início, catálogo, detalhes, AVA e página 404
  src/app/shared/      cabeçalho, rodapé, cartões e seções reutilizáveis
  src/styles.css       tokens e estilos globais
docs/
  DECISOES_TECNICAS.md decisões de arquitetura e propostas de evolução
  VALIDACAO.md         verificações e limites desta versão
USO_DE_IA.md           registro de uso de inteligência artificial
```

O fluxo de dados atual é: arquivo `courses.json` → `CourseService` → signals → componentes. O frontend ainda não consulta uma API de negócio. Uma falha no carregamento é exibida ao usuário, sem substituir silenciosamente a origem por dados locais alternativos.

Os comentários do código registram os motivos das escolhas e os limites de integração. A relação entre requisitos, decisões e oportunidades de melhoria está em `docs/DECISOES_TECNICAS.md`.

## Modelo de dados atual

| Entidade | Campos principais |
| --- | --- |
| Curso | id, slug, nome completo, nome curto, descrição, apresentação, categoria, modalidade, horas, público, status, imagem, disponibilidade e destaque |
| Módulo | título, descrição, tópicos e aulas |
| Aula | id, título e descrição |

O JSON usa os nomes técnicos em inglês definidos em `course.model.ts`. Datas de disponibilidade são datas de calendário no formato `YYYY-MM-DD`; a interface as apresenta em `dd/MM/yyyy`, com UTC explícito para evitar alteração do dia por fuso horário.

## Backend e banco — próxima etapa

O enunciado exige C#/.NET e banco open source. A proposta para a próxima etapa é ASP.NET Core com SQLite e persistência de cursos, módulos e aulas. Ainda não existem endpoints, migrations ou scripts de banco nesta versão. A API deverá fornecer cursos, módulos e aulas, incluindo suas descrições, para o catálogo e a AVA. A carga inicial desses dados será definida na etapa de backend, reaproveitando o dataset provisório. O contrato e a configuração serão documentados quando a API for implementada e verificada.

Moodle é um diferencial opcional no enunciado. A página AVA desta versão é uma demonstração visual, sem integração com Moodle, autenticação, provas ou emissão de certificados.

## Responsividade e acessibilidade

- Layouts fluidos, grades que se reorganizam e menu de navegação para telas estreitas.
- Barra de inscrição com espaço reservado no fim da página.
- Links para pular diretamente ao conteúdo, labels de formulário, foco visível e mensagens de resultado com regiões vivas.
- Elementos `details`/`summary` para os painéis, `dialog` para avisos e botões nativos para ações.
- Texto alternativo em imagens informativas e ícones decorativos ignorados por leitores de tela.
- Respeito à preferência de redução de movimento.

Essas medidas não equivalem a uma certificação de conformidade. O alcance da verificação está em `docs/VALIDACAO.md`.

## Diferenças e limitações conhecidas

- O acesso automatizado ao Figma não ficou disponível; as referências foram as capturas fornecidas pela Bruna e os anexos do teste. Medidas e comportamento responsivo foram reconstruídos a partir delas.
- O pacote contém três fotos. A imagem de medição foi reaproveitada no primeiro cartão; os demais cartões usam provisoriamente as fotos disponíveis, pois as imagens individuais do carrossel não vieram nos anexos.
- Não foi fornecido um arquivo ou endereço de vídeo. Os controles de reprodução abrem um aviso de indisponibilidade.
- Bruna confirmou que os botões “Todos os cursos” não possuem destino clicável no protótipo. A tela de catálogo foi construída com a mesma linguagem visual para atender aos requisitos de busca e filtros do enunciado; nesta implementação, os botões “Todos os cursos” e os acessos gerais ao catálogo abrem essa tela.
- Carga horária, modalidade, público, status e datas não fornecidas nas referências usam valores demonstrativos para os campos exigidos pelo enunciado. Esses valores são dados de teste e não descrevem ofertas reais.
- Os títulos e as descrições dos seis cursos em destaque reproduzem as capturas fornecidas. O catálogo inclui também o curso complementar de Gestão de Resíduos para contemplar a terceira categoria exigida no enunciado.
- Os dez módulos do primeiro curso reproduzem os títulos e tópicos das capturas. Nos demais cursos, a estrutura de navegação usa identificadores neutros: “Módulo 1”, “Módulo 2” etc., com uma “Aula 1” sem conteúdo didático por módulo. A quantidade de módulos dessas ofertas é demonstrativa. As apresentações dos outros cinco cursos em destaque reutilizam suas descrições curtas fornecidas nas capturas.
- A FAQ e o painel de aulas/avaliações são exibidos apenas no curso de referência. Os textos de “Boas práticas” e “Recomendações” da AVA desse curso foram transcritos das capturas. Na AVA, os módulos usam os rótulos “Módulo 1”, “Módulo 2” etc.; cada módulo contém somente “Aula 1”, com uma breve descrição identificada como “Exemplo fictício”. Os rótulos de introdução fornecidos na referência foram preservados.
- Conforme a navegação do protótipo informada pela Bruna, “Matricule-se” no bloco vermelho da landing page abre os detalhes de Matemática para a Precisão; o botão homônimo da faixa preta fixa nos detalhes abre diretamente a AVA do curso selecionado. Cursos em breve ou encerrados mantêm o controle da faixa desabilitado. O acesso é demonstrativo e não efetiva matrícula. O botão do início dos detalhes também oferece acesso à demonstração por um aviso.
- As cópias que pressupunham provas, biometria ou certificação oficial não foram tratadas como funcionalidades disponíveis.
- Itens institucionais fora do escopo abrem um aviso. Os links de cursos do rodapé foram adaptados aos sete cursos existentes.
- Indicadores, depoimentos e dados de contato reproduzem conteúdo ilustrativo da referência. O projeto não é um serviço oficial de cursos.
- As descrições das aulas são exemplos fictícios para demonstrar o campo previsto no item 4(b) do enunciado. Reutilizam o primeiro tópico fornecido de cada módulo de Matemática e as descrições curtas dos cartões nas demais ofertas. A associação desses textos à “Aula 1” é demonstrativa e não representa uma grade de aulas fornecida pelo teste.
- O botão “Marcar como concluída” foi acrescentado à AVA e não faz parte do Figma. Ele demonstra a conclusão de uma aula e a atualização do progresso, seguindo uma interação comum em cursos online. A marcação pode ser desfeita e o progresso pode ser reiniciado. Os dados ficam apenas neste navegador, sem sincronização com o servidor ou efeito de matrícula, avaliação ou certificação.
- Ainda faltam backend, banco, integração, publicação e a documentação de execução correspondente.

## Uso de IA

O desenvolvimento desta etapa contou com a assistência do agente Nemo, do ChatGPT/Codex, na análise de requisitos, geração e adaptação de código, testes e documentação técnica. Bruna forneceu o enunciado, os arquivos de apoio e as referências visuais. O registro das contribuições e das verificações está em `USO_DE_IA.md`.
