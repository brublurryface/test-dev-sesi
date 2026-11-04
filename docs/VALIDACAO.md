# TEST DEV SESI Indústria — Validação

Verificações executadas pelo agente Nemo, do ChatGPT/Codex. Este registro distingue as revisões de 03/11/2026 dos testes e revisões anteriores da versão integrada.

## Conteúdo de aula sob demanda — 03/11/2026

Revisão preparada na branch `feat/aulas-sob-demanda`, a partir de `edf4e6b`, após a integração da PR #4. O índice mantém os IDs e a ordem utilizados no progresso e recebe apenas prévias de descrição. Os blocos completos são consultados por aula. Bruna autorizou o registro em dois commits, a abertura da PR e a publicação após a conferência da alteração. A verificação da versão publicada permanece pendente neste registro.

| Verificação | Resultado observado |
| --- | --- |
| Frontend | 33 testes aprovados; 12 novos casos usam RxJS real para seleção, resposta tardia, cache, limites, recuperação após erro e destruição |
| Backend e SQLite | 53 testes aprovados; oito casos novos verificam projeção, quantidade constante de consultas, escopo por curso, conteúdos preservados, prévias e 404 |
| Corrida 728 → 729 | A inscrição de 728 foi cancelada; sua resposta posterior não substituiu 729 nem foi armazenada no cache |
| Retorno a aula em cache | Não repetiu o pedido e cancelou uma requisição pendente de outra aula |
| Chaves compartilhadas entre cursos | Mesmo ID não reutilizou conteúdo de outro curso; a API consulta slug e chave juntos |
| Cache | Evicção LRU por quantidade e bytes verificada; conteúdo acima do limite foi exibido sem cache; falhas e respostas com ID divergente não foram reaproveitadas |
| Índice maior | Mais 500 aulas inseridas somente no banco temporário: índice com 513 aulas conservou a quantidade de consultas da versão com 13, sem consultar `ContentJson` |
| Consulta selecionada | Uma única consulta para a aula solicitada; Boas práticas manteve seus oito blocos de referência |
| Descrições longas | Prévia de 240 caracteres com reticências no índice; descrição de 50.000 caracteres preservada integralmente na rota da aula, tanto para módulo quanto para introdução |
| Build e formatação | Angular de produção com templates estritos, publicação Release da API, Prettier e `git diff --check` passaram |

O pacote integrado também foi iniciado em produção no executor e consultado por HTTP. O índice de Matemática manteve exatamente 10.384 bytes antes e depois de acrescentar um corpo de 1.000.000 de caracteres a uma aula de teste. Somente a rota dessa aula retornou o corpo maior, em uma resposta de 1.000.189 bytes. Catálogo, escopo por curso, 404, OpenAPI, rota direta da AVA e JavaScript publicado foram conferidos. Os dados maiores existiram apenas em bancos temporários.

O índice completo continua disponível para `allLessons()`, mas seus itens são `LessonSummary`, sem os blocos de conteúdo. Os testes anteriores de seleção inicial, retomada, numeração e progresso permanecem aprovados. As chaves locais de progresso e o esquema SQLite foram preservados; `CatalogSeedCourse` mantém o formato completo usado pela carga inicial e pelos testes Moodle.

A tela confere curso e ID antes de exibir a resposta e só permite concluir uma aula cujo conteúdo corresponde à seleção atual. O cache é técnico, em memória por AVA, limitado a dez entradas e 2 MiB de JSON serializado. Recarregar o índice e destruir o componente encerram a seleção e limpam o cache. O cancelamento de inscrição evita aplicar a resposta antiga no cliente; não desfaz trabalho que já tenha terminado no servidor.

Os testes do carregador usam respostas HTTP simuladas, com RxJS real. A conexão HTTP do pacote foi verificada separadamente; a interação completa da tela com cliques rápidos, foco, leitores de tela e rede lenta ainda requer conferência no navegador. O índice cresce com o número de aulas e ainda reúne os tópicos necessários aos detalhes do curso; esta etapa não implementa virtualização do índice, cache persistente, invalidação em tempo real nem pré-carregamento de aulas vizinhas.

## Paginação do catálogo — 03/11/2026

Após o apontamento arquitetural compartilhado pela Bruna, a listagem passou a usar `CountAsync()` para o total filtrado e `OrderBy(Id)`, `Skip()` e `Take()` para materializar somente uma página. Padrão de 20 e máximo de 100 cursos por pedido. As opções dos filtros continuam sendo obtidas do catálogo inteiro. O registro desta etapa em commits foi autorizado pela Bruna na branch `fix/filtros-dinamicos`; publicação e conferência visual permanecem pendentes.

| Verificação | Resultado observado |
| --- | --- |
| API e SQLite | 45 testes aprovados; 13 novos casos cobrem catálogo de 47 cursos, limites, sequência completa sem repetições/omissões, total filtrado, opções globais, página além da última e parâmetros inválidos |
| Frontend | 21 testes aprovados; incluem leitura da página e preservação de busca/filtros ao montar e reabrir links |
| Build integrado | Angular de produção com templates estritos e API em Release compilados com sucesso |
| Formatação | Prettier e `git diff --check` passaram |
| Volume por HTTP | Banco temporário com 100.000 cursos sintéticos; páginas 1, 2 e 5.000 retornaram 20 itens, total 100.000 e total de 5.000 páginas, com IDs na sequência esperada |
| Transferência observada | As três respostas do teste de volume tiveram 8.947, 7.521 e 7.646 bytes, incluindo metadados e opções; não transferiram os demais resumos |
| Outros pedidos HTTP | Limites de 6, 7 e 100 itens, página além da última, entrada inválida com 400 e rota direta Angular conferidos |

Os testes detectaram que a conversão de parâmetros malformados, como `page=abc`, produzia 500 em desenvolvimento. O tratamento agora preserva o status de `BadHttpRequestException`, retornando 400; a regressão também inclui `featured=abc`. Falhas inesperadas continuam respondendo 500.

O catálogo inicia na página 1 ao alterar ou limpar filtros e mantém a página no parâmetro público `pagina`. Anterior/Próxima usam a contagem de páginas da API e só aparecem quando há mais de uma página; páginas além da última recebem uma opção de retorno à primeira. Destaques e rodapé usam limites próprios de seis e sete cursos.

A revisão visual dos controles e a interação com eles no navegador ainda estão pendentes pela limitação da prévia local. A leitura/montagem da URL foi testada como lógica e o template foi compilado. O exercício de 100.000 registros é uma verificação funcional local; não representa um teste de carga ou garantia de desempenho em produção. Deslocamentos profundos, alterações concorrentes, custo da contagem e índices continuam sendo considerações de evolução, conforme [paginação no EF Core](https://learn.microsoft.com/en-us/ef/core/querying/pagination).

## Dependências, navegação da AVA e configurações locais — 03/11/2026

Revisão preparada após os três commits de filtros, a partir de `deb6f8b`, na branch `fix/filtros-dinamicos`. O registro das alterações desta etapa em commits foi autorizado pela Bruna; a publicação permanece pendente.

| Verificação | Resultado observado |
| --- | --- |
| Dependências de desenvolvimento | Angular CLI e build atualizados de 22.2.4 para 22.2.5; o lockfile passou a resolver Vite 8.3.5 |
| Auditoria npm | Zero alertas na consulta completa após a atualização, incluindo ferramentas de desenvolvimento |
| Testes do frontend | 19 testes aprovados; quatro novos casos cobrem números originais na busca, busca por módulo, limpeza sem alterar IDs/dados e ausência de resultados |
| Formatação e compilação | Prettier, `git diff --check`, Angular de produção com templates estritos e publicação Release da API passaram |
| Servidor de desenvolvimento | Angular CLI iniciou; página inicial e rota direta da AVA retornaram HTML; proxy `/api/courses` retornou sete cursos e opções dos filtros |
| Regras do Git | Sete caminhos `.env`/`.env.*`, incluindo subdiretórios, foram ignorados; três caminhos `.env.example` permaneceram permitidos |

Boas práticas permanece **1.2** e Extras permanece **1.3** quando cada uma é o único resultado da busca. Os números são atribuídos antes do filtro; a busca continua ignorando caixa, acentos e espaços externos. A extração da normalização para `learning-navigation.ts` mantém seu teste anterior. Os identificadores e os dados recebidos da API são preservados.

O alerta original envolvia Vite 8.3.0 na ferramenta de desenvolvimento, com atualização disponível. A referência de segurança inclui [GHSA-587h-7wmj-h5pm](https://github.com/advisories/GHSA-587h-7wmj-h5pm), sobre Windows. A auditoria após a atualização foi executada em 03/11/2026.

A numeração foi verificada por testes da lógica usada pelo componente e pela compilação estrita do template. A revisão visual permanece pendente pela limitação de acesso à prévia local descrita abaixo. Otimização das imagens, automação de testes nas PRs e cabeçalhos de proteção do navegador são melhorias identificadas para etapas seguintes.

## Filtros derivados do catálogo — 03/11/2026

Revisão preparada na branch `fix/filtros-dinamicos`, a partir da `main` em `a4ea107`. Os resultados desta seção pertencem ao código local da revisão; sua publicação depende da revisão da Bruna.

| Verificação | Resultado observado |
| --- | --- |
| Frontend | 15 testes aprovados; incluem leitura de links com valores novos, preservação da seleção e apresentação de categorias e status novos |
| API e SQLite | 32 testes aprovados; sete casos novos verificam opções do banco inteiro, combinação vazia, valores novos e removidos, catálogo vazio e validação da carga inicial |
| Build integrado | Angular compilado com templates estritos e API publicada em Release, sem avisos ou erros de compilação |
| Formatação | Prettier e `git diff --check` passaram |
| Pacote Release por HTTP | Oito grupos de verificações aprovados: saúde; quatro rotas Angular diretas; JavaScript e ícone genérico; catálogo e resultado vazio; novos valores combinados e consulta repetida; filtro inválido e recuperação; contrato OpenAPI; 404 de API |
| Estado da listagem | Exercício adicional com cliente HTTP simulado verificou primeira carga, opções no resultado vazio, falha de conexão, filtro inválido, recuperação e descarte de resposta antiga. Esta verificação complementar foi executada no executor e não integra o comando de testes do frontend |

Os testes inseriram `Tecnologia & Inovação`, `Híbrido` e `Lista de espera` apenas em bancos temporários. Uma consulta combinando esses valores e a busca `MATEMATICA` retornou o curso esperado sem alterar código ou a carga de demonstração do projeto. A remoção de uma opção foi refletida na consulta seguinte; um link com o valor removido retornou 400.

A lógica de leitura da URL foi verificada por teste, e as rotas diretas do pacote foram verificadas por HTTP. A conferência visual desta revisão, incluindo recarga no navegador e rótulos longos nos seletores, permanece pendente: o navegador do executor bloqueou o acesso à prévia local com `ERR_BLOCKED_BY_CLIENT`. As revisões visuais históricas abaixo não substituem essa conferência. Nesta etapa inicial de filtros a listagem ainda não tinha paginação; a evolução posterior está registrada na seção de paginação acima.

## Diferencial Moodle — 26/10/2026

- As três funções de leitura foram consultadas na instância real pelo PowerShell da Bruna: curso ID 3, seção geral e dez módulos, além da página Aula 1 inicialmente cadastrada. O cadastro foi depois completado e conferido no navegador, com dez páginas Aula 1 e três itens introdutórios; Boas práticas preserva o texto fornecido nas capturas.
- Os 25 testes do backend passaram: 17 testes anteriores da API/SQLite e oito novos casos para importação Moodle. Os novos casos usam HTTP simulado e SQLite real: importação bem-sucedida, preservação de IDs e de outros cursos, conteúdo de referência, página ausente, curso incorreto, erro Moodle com HTTP 200, falha HTTP, cancelamento, configuração desativada/inválida e conversão de HTML em texto.
- A API compilou e foi publicada em Release no executor, com verificação estrita de avisos. Não houve alteração no frontend nem inclusão de pacotes de terceiros.
- O processo Release foi iniciado com a integração desativada e passou em cinco verificações HTTP: diagnóstico `disabled`, sete cursos, dez módulos de Matemática, oito blocos de Boas práticas e saúde do banco. O projeto continua executável sem credenciais Moodle.
- A integração desta versão ainda depende de revisão, implantação e configuração privada do token no Render. Os testes automatizados não substituem a verificação dessa implantação. Detalhes, estados e limites estão em [MOODLE.md](MOODLE.md).

## Versão integrada

| Verificação                                     | Resultado observado                                                                                                                                   |
| ----------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------- |
| Angular: build de produção e templates estritos | Passou                                                                                                                                                |
| Frontend: testes de busca de aulas e progresso  | 12 testes aprovados                                                                                                                                   |
| API: compilação e publicação Release            | Passou, sem avisos ou erros                                                                                                                           |
| API + SQLite: testes de integração HTTP         | 17 testes aprovados; cada teste utiliza banco temporário isolado                                                                                      |
| Catálogo                                        | Sete cursos, seis em destaque, três categorias, duas modalidades e três status                                                                        |
| Busca                                           | Acentos, caixa, espaços externos e termos sem correspondência verificados por HTTP                                                                    |
| Filtros combinados                              | Nome, categoria, modalidade e status juntos retornaram um curso; combinação incompatível retornou zero; limpeza retornou sete                         |
| Entrada inválida                                | Valores de filtro desconhecidos e busca longa retornaram 400 com Problem Details                                                                      |
| Texto semelhante a SQL                          | Tratado apenas como termo de busca, sem ampliar a consulta                                                                                            |
| Detalhes                                        | Dez módulos do curso de referência, uma Aula 1 por módulo, descrições fictícias, introdução, dez perguntas e blocos de orientação fornecidos pela API |
| Integridade de banco                            | A tentativa de associar uma aula ao módulo de outro curso foi recusada pelo SQLite                                                                    |
| Persistência                                    | Descrições alteradas diretamente no banco de teste permaneceram após reiniciar a aplicação; carga inicial não sobrescreveu os dados                   |
| Esquema e carga                                 | Migration aplicada; contagem no banco do pacote: sete cursos, 28 módulos, 49 registros de aulas, 30 tópicos e dez perguntas                           |
| Pacote integrado por HTTP                       | Página inicial, JavaScript de entrada e acesso direto a catálogo, detalhes e AVA retornaram 200                                                       |
| API no pacote integrado                         | Consulta com quatro critérios, detalhes, health check e OpenAPI retornaram os dados esperados                                                         |
| Rota desconhecida de API                        | Retornou 404 com application/problem+json; o fallback da SPA não substituiu a resposta                                                                |
| Formatação                                      | Prettier no frontend e dotnet format whitespace em modo de pasta no backend                                                                           |

A listagem, as descrições e os textos do curso vêm do banco. O frontend não mantém o catálogo JSON público nem uma alternativa local à API.

## Refinamentos de interação em 23/10

- Botões com seta compartilham a animação de saída pela direita e retorno pela esquerda. O ciclo foi observado no hero; o mesmo efeito foi conferido em Matricule-se e Mandar uma mensagem. A animação para ao sair do hover.
- A posição do botão do hero permaneceu em 497,984 px durante a amostragem, com transform igual a none. O botão de mensagem também permaneceu parado. A elevação geral de botões foi removida; a elevação dos cartões de cursos foi preservada.
- O fundo de hover do hero foi conferido como #E6E6E6; o botão contornado Ver todos os cursos utiliza #F5F1EC. O tempo da animação é uma aproximação da interação descrita pela Bruna; a timeline do Figma não ficou disponível.
- A conclusão demonstrativa de Boas práticas alterou a marcação para Aula concluída e o progresso de zero para 8%. O README e o registro de IA explicam o acréscimo ao protótipo e seu armazenamento local.

- Em 24/10, o sublinhado no hover do card branco Todos os cursos foi retirado conforme a referência confirmada pela Bruna. Formatação e compilação de produção passaram; a conferência visual do pacote atualizado permanece em andamento.

## Revisão de comentários e arquitetura em 25/10

Foram documentadas as responsabilidades e decisões em oito arquivos C#, 13 arquivos TypeScript e no script de publicação. O mapa de arquivos e o percurso das requisições estão em [ARQUITETURA.md](ARQUITETURA.md), também referenciado no README.

- As linhas executáveis C# foram comparadas com a versão anterior, removendo apenas linhas de comentário e linhas vazias: permaneceram idênticas.
- Os tokens executáveis dos 14 arquivos TypeScript/JavaScript alterados foram comparados pelas árvores de tokens do compilador TypeScript, desconsiderando comentários: permaneceram idênticos.
- Os blocos XML de documentação C# e os destinos dos links locais do mapa de arquitetura e do README foram verificados.
- A compilação Angular de produção e a formatação dos arquivos revisados passaram.

Esta etapa altera comentários e documentação. Os resultados de compilação e testes da API registrados acima pertencem à validação anterior; a revisão atual verifica a preservação das instruções executáveis C#.

## Entrada da AVA e itens introdutórios em 25/10

- 12 testes frontend aprovados, incluindo primeiro acesso em Introdução, retomada na primeira pendente, conclusão posterior com Introdução ainda pendente, curso concluído e curso sem aulas.
- 17 testes HTTP com SQLite aprovados. O teste adicional inicia no esquema anterior, aplica a atualização de descrições e verifica a preservação de textos de referência e de conteúdo já preenchido.
- A fonte inicial contém 20 descrições introdutórias identificadas como “Exemplo fictício”, além das 28 descrições demonstrativas dos módulos. Os oito blocos de Boas práticas do curso de Matemática para a Precisão foram preservados.
- O grupo da aula selecionada é expandido na AVA para manter visível uma aula retomada nos módulos.

A regra de seleção foi verificada por testes de suas entradas e resultados. A conferência posterior da retomada no pacote integrado foi confirmada pela Bruna, conforme o registro abaixo.

A revisão adicional de valores fixos vinculou o link da área do aluno ao curso exibido nos detalhes, substituiu os nomes e slugs manuais do rodapé por dados da API e centralizou configurações públicas repetidas. A compilação estrita do Angular passou. O pacote Release atualizado passou por 15 verificações HTTP e de banco, incluindo os textos de Introdução e Extras, os oito blocos de referência preservados, a aplicação da migration e 48 descrições fictícias identificadas.

## Conferência do pacote integrado no Windows pela Bruna — 25/10

Após atualizar o pacote de publicação e executar a aplicação em `http://localhost:5080`, Bruna confirmou:

- Retomada em Boas práticas após a conclusão de Introdução, conforme a primeira aula pendente.
- Acesso à AVA de Geometria e Modelagem pelos detalhes do próprio curso, mantendo o curso correto após recarregar a página.
- Funcionamento satisfatório do layout móvel, sem registro de uma dimensão ou aparelho específico nessa confirmação.

Essas confirmações complementam os testes automatizados e a revisão do frontend. A recarga com busca e filtros foi posteriormente conferida no endereço público, conforme a seção seguinte. A recuperação após indisponibilidade da API não foi provocada no navegador integrado.

## Preparação da hospedagem em Docker — 25/10

O Dockerfile compila frontend e API em etapas separadas, utiliza as versões de referência de Node.js e .NET, entrega ambos pelo ASP.NET Core e prepara escrita no SQLite para um usuário sem privilégios. O .dockerignore exclui bancos e resultados locais do contexto de build. As três tags de imagens utilizadas foram consultadas nos registros oficiais e oferecem Linux amd64.

A configuração do Render Free e seus limites de armazenamento temporário e suspensão por inatividade estão em [PUBLICACAO.md](PUBLICACAO.md). A construção da imagem e a execução do serviço foram concluídas no Render, conforme o registro da publicação abaixo.

O pacote integrado foi executado com ASP.NET Core Runtime 10.0.13 e o comando de entrada do Dockerfile, fora de um contêiner. Nove verificações passaram: health check nas portas padrão e configurada, carga inicial de sete cursos, rota direta da AVA, encerramento por SIGTERM em ambas as execuções, conservação do catálogo no reinício, aplicação das duas migrations e integridade das chaves estrangeiras. Essas verificações locais foram complementadas pela construção e execução da imagem no Render. O Dockerfile define `USER 1654:1654`; a inicialização do contêiner e a criação do SQLite ocorreram com sucesso. Não houve inspeção independente do UID do processo hospedado.

## Publicação integrada no Render — 25/10

Endereço: [test-dev-sesi.onrender.com](https://test-dev-sesi.onrender.com/). A imagem foi construída do commit `ee40f19`, cujo conteúdo foi integrado à `main` pela [PR #1](https://github.com/brublurryface/test-dev-sesi/pull/1). Serviço Free, runtime Docker, região Virginia, publicação automática desativada e monitoramento TCP padrão do Render.

| Verificação                     | Resultado observado                                                                                                                       |
| ------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| Construção e inicialização      | Imagem construída; duas migrations aplicadas; aplicação ouvindo em `0.0.0.0:10000`; deploy `live`                                         |
| Health check e catálogo         | `/api/health`: 200 e `status: ok`; listagem com sete cursos e seis destaques                                                              |
| Consulta pública                | Busca sem acento e três filtros retornaram um curso; combinação incompatível retornou zero                                                |
| Validação e rotas de API        | Busca longa: 400; curso e endpoint inexistentes: 404; respostas em JSON                                                                   |
| Detalhes e conteúdo             | Dez módulos no curso de referência; três em Geometria; descrições introdutórias e das aulas disponíveis                                   |
| Rotas e OpenAPI                 | Página inicial, catálogo, detalhes e AVA de outro curso responderam ao acesso direto; OpenAPI disponível                                  |
| Resultado das verificações HTTP | 15 de 15 verificações aprovadas                                                                                                           |
| Catálogo no navegador           | Filtros combinados, seleção preservada após recarregar, estado vazio e limpeza para sete cursos conferidos                                |
| Curso selecionado               | Detalhes de Geometria abriram a AVA de Geometria; controles Em breve desabilitados nos detalhes                                           |
| Progresso da AVA                | Introdução concluída: 17%; após recarregar, abertura em Boas práticas; Introdução e Extras com exemplos fictícios; reinício retornou a 0% |
| Alcance visual público          | Navegador desktop em 1363 × 936 px; os testes móveis anteriores permanecem registrados abaixo                                             |

A indisponibilidade da API não foi provocada no navegador público; a recuperação de falha continua sem confirmação nesse ambiente. A suspensão por inatividade e a perda do disco temporário são limites documentados do plano Free, sem simulação de todo o ciclo de suspensão nesta validação.

## Histórico da revisão visual do frontend

As observações desta seção foram feitas no navegador usando o build do frontend. Na etapa anterior à integração, os dez testes então existentes incluíam filtros locais; as regras de consulta passaram a ser verificadas nos 16 testes HTTP com SQLite, enquanto naquela revisão o frontend mantinha oito testes.

| Verificação                                          | Resultado                                                                                                                                                                                                                                                                                                                                                                                                                      |
| ---------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Instalação de dependências                           | Concluída com npm                                                                                                                                                                                                                                                                                                                                                                                                              |
| Compilação de produção e templates estritos          | Passou                                                                                                                                                                                                                                                                                                                                                                                                                         |
| Formatação padronizada com Prettier                  | Verificada por npm run format:check                                                                                                                                                                                                                                                                                                                                                                                            |
| Testes automatizados das regras de negócio           | Dez testes aprovados                                                                                                                                                                                                                                                                                                                                                                                                           |
| Consistência do catálogo                             | Arquivo conferido: sete cursos, seis em destaque, três categorias, duas modalidades e três status; IDs e slugs únicos                                                                                                                                                                                                                                                                                                          |
| Busca sem acentos, caixa e espaços externos          | Teste automatizado passou                                                                                                                                                                                                                                                                                                                                                                                                      |
| Combinação de nome, categoria, modalidade e status   | Teste automatizado passou                                                                                                                                                                                                                                                                                                                                                                                                      |
| Limpeza e preservação da coleção original            | Teste automatizado passou                                                                                                                                                                                                                                                                                                                                                                                                      |
| Rolagem ao buscar, selecionar e limpar filtros       | Verificada no navegador: lista e URL atualizadas sem retorno ao topo; abertura dos detalhes e retorno pelo histórico preservados                                                                                                                                                                                                                                                                                               |
| Restauração de progresso ausente ou inválido         | Testes automatizados passaram                                                                                                                                                                                                                                                                                                                                                                                                  |
| Identificadores duplicados ou de aulas removidas     | Testes automatizados passaram; não alteram a contagem de aulas concluídas                                                                                                                                                                                                                                                                                                                                                      |
| Progresso de curso vazio, parcial e completo         | Testes automatizados passaram; percentual preservado entre zero e cem                                                                                                                                                                                                                                                                                                                                                          |
| Navegação início → catálogo → detalhes → AVA         | Verificada no navegador                                                                                                                                                                                                                                                                                                                                                                                                        |
| Links “Cursos” do header e “Conhecer cursos” do hero | Verificados em 23/10: rolagem suave até “Principais cursos”; repetição e retorno a partir de outras páginas preservados. Título a 24 px do header fixo: 106 px no desktop e 100 px no frame de 390 × 844 px; menu móvel fechado após selecionar Cursos                                                                                                                                                                         |
| Item “Sobre” do header                               | Link sem destino definido: preventDefault preserva a posição e fecha o menu móvel, conforme a orientação atualizada da Bruna                                                                                                                                                                                                                                                                                                   |
| Tom do hero e link “Área do aluno”                   | Revistos com as capturas: foto ampliada e reposicionada, camada vermelha uniforme; link sem seta, com cantos arredondados. Sobre o hero, fundo transparente e contorno branco discreto visível em repouso; escurecimento no hover, conforme o recorte sem mouse fornecido em 23/10; no header branco, contorno cinza. Destino à AVA conferido em desktop e no menu de um frame de 390 × 844 px, sem transbordamento horizontal |
| Fluxo dos botões “Matricule-se”                      | Verificado no navegador: bloco vermelho da landing page → detalhes de Matemática para a Precisão → faixa preta fixa → AVA, sem aviso intermediário na faixa                                                                                                                                                                                                                                                                    |
| Combinação de busca e três filtros na interface      | Retornou o único curso esperado                                                                                                                                                                                                                                                                                                                                                                                                |
| Estado vazio e botão de limpeza                      | Verificados novamente após ampliar o catálogo; a limpeza devolveu os sete cursos                                                                                                                                                                                                                                                                                                                                               |
| Busca por MATEMATICA sem acento                      | Retornou o curso de Matemática para a Precisão                                                                                                                                                                                                                                                                                                                                                                                 |
| Painéis independentes nos detalhes                   | Três abertos simultaneamente; dez módulos e dez perguntas                                                                                                                                                                                                                                                                                                                                                                      |
| Curso em breve                                       | Botão de matrícula desabilitado                                                                                                                                                                                                                                                                                                                                                                                                |
| AVA                                                  | Busca e recuperação de progresso verificadas; em 23/10, rótulos “Módulo 1, 2…” e uma “Aula 1” por módulo conferidos; descrições identificadas como “Exemplo fictício” verificadas na AVA e no painel dos detalhes. Marcação, reinício e avanço ao próximo módulo também verificados                                                                                                                                            |
| Origem do conteúdo                                   | Conferidos os 28 módulos do JSON: uma Aula 1 por módulo, com descrição breve iniciada por “Exemplo fictício:”. Os textos reutilizam tópicos de Matemática ou a descrição curta do curso. Nos demais cursos, apresentações ampliadas, tópicos didáticos e FAQ genérica foram removidos; no curso de referência, dez módulos e dez perguntas preservados                                                                         |
| Layout estreito                                      | Revisado em um frame de 390 × 844 px; sem transbordamento horizontal em início, catálogo, detalhes e AVA                                                                                                                                                                                                                                                                                                                       |
| Menu em layout estreito                              | Abertura e navegação verificadas, incluindo o painel de conteúdo da AVA                                                                                                                                                                                                                                                                                                                                                        |
| Teclado                                              | Tab entre busca e selects; fechamento de diálogo com Escape verificado                                                                                                                                                                                                                                                                                                                                                         |
| Alinhamento do carrossel                             | Ajustado e conferido com a margem do conteúdo                                                                                                                                                                                                                                                                                                                                                                                  |
| Cartão Todos os cursos                               | Fundo branco explícito, texto/seta pretos na parte inferior, cantos arredondados e sombra conforme a captura adicional de 23/10; acesso ao catálogo preservado. No frame de 320 × 844 px, texto e seta cabem no cartão; a landing page ficou com conteúdo de 305 px, igual à área útil, após ajuste dos controles e da disposição dos indicadores                                                                              |
| Fim do carrossel de cursos                           | Seis ofertas e cartão “Todos os cursos” conferidos no navegador; o cartão abriu o catálogo                                                                                                                                                                                                                                                                                                                                     |
| Curso de Ciência dos Materiais                       | Encontrado por nome sem acentos e pelos três filtros combinados; retornou um único resultado                                                                                                                                                                                                                                                                                                                                   |
| Depoimentos                                          | Cinco nomes e textos conferidos com as capturas; Gabriel Santos incluído. Fundo #890000 e cartões #C3222C na landing page; fundo #FCD19C e cartões brancos nos detalhes. Estrelas abaixo do nome e cargo, conforme as referências                                                                                                                                                                                              |
| Revisão das 31 capturas fornecidas                   | Conferidos hero, header, cartões, indicadores, benefícios, depoimentos, chamadas, rodapé, detalhes, painéis e AVA. Ajustados botões retangulares com cantos arredondados, cores, tipografia, grades, proporções dos vídeos, ícones de data/certificação e banner da AVA. Reconstrução visual a partir de capturas; sem equivalência pixel a pixel certificada                                                                  |
| Revisão visual em 1920 × 1080 px                     | Navegador em frame nessa dimensão: título do curso em três linhas; vídeo em 16:9; faixa fixa de matrícula alinhada ao conteúdo; banner da AVA com proporção 6:1 e raio de 12 px. Pequenas diferenças de área útil decorrem das barras de rolagem do navegador                                                                                                                                                                  |
| Revisão após os ajustes em 390 × 844 px              | Início, catálogo, detalhes e AVA com largura de conteúdo igual à área útil de 375 px; sem transbordamento horizontal. Âncora de cursos a 100 px; painel móvel de conteúdo da AVA abriu e fechou                                                                                                                                                                                                                                |
| Movimento dos carrosséis                             | Avanço e retorno de cursos e depoimentos verificados. Cursos também conferidos com comandos sucessivos até o limite; encaixe e estilos restaurados após a animação. Duração de 700 ms definida no utilitário compartilhado; redução de movimento tratada no código, sem emulação dessa preferência nesta revisão                                                                                                               |
| Imagens e fonte fornecidas                           | Carregadas na revisão visual                                                                                                                                                                                                                                                                                                                                                                                                   |
| Texto do curso de referência                         | FAQ completa e seções “Boas práticas” e “Recomendações” conferidas com as capturas fornecidas                                                                                                                                                                                                                                                                                                                                  |

## Alcance e pendências

A integração foi verificada pelos testes HTTP com SQLite, pelo processo do pacote Release no executor e, após a publicação, por HTTP e no navegador público. A revisão visual inicial utilizou o build do frontend; Bruna também conferiu parte do fluxo integrado no Windows. A aplicação está publicada e a recarga dos filtros foi conferida. A recuperação após indisponibilidade da API permanece sem confirmação no navegador integrado.

A revisão não equivale a uma auditoria completa com leitores de tela nem a testes exaustivos em dispositivos físicos. A preferência de redução de movimento é tratada no código; não foi emulada nesta revisão.

A ferramenta de formatação C# baseada na solução não pôde abrir o pipe do build host neste ambiente. O modo de pasta foi utilizado para a formatação de espaços; compilação estrita e testes da solução passaram. As incompatibilidades iniciais de opções da prévia Angular foram resolvidas com a configuração do CLI, sem desativar a verificação de hosts.
