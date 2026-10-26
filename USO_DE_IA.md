# Uso de inteligência artificial

Projeto **TEST DEV SESI Indústria**. Registro referente à versão integrada, atualizado em 25/10/2026.

## Ferramenta e contribuição

Foi utilizado o **agente Nemo, do ChatGPT/Codex**, como apoio à análise dos requisitos, à implementação e à verificação do projeto. A contribuição incluiu geração e adaptação de componentes Angular, estilos, API C#/.NET, modelo SQLite, migrations, carga demonstrativa, integração HTTP, testes e documentação técnica.

O diferencial Moodle foi acrescentado com apoio do agente Nemo na configuração do curso e do serviço de leitura, implementação do cliente REST, importação para SQLite, diagnóstico, testes e documentação. Bruna criou e confirmou a conta de hospedagem, aceitou os termos, autorizou o acesso e executou as consultas autenticadas de verificação no PowerShell. A chave foi mantida fora da conversa e do repositório. O alcance e a situação da validação estão registrados em [docs/MOODLE.md](docs/MOODLE.md).

## Referências fornecidas pela Bruna

Bruna forneceu o enunciado, os arquivos de apoio e as capturas do protótipo, incluindo a landing page, os detalhes do curso, os painéis expandidos e a AVA. Esses materiais orientaram a estrutura das páginas, os conteúdos e os comportamentos implementados nesta versão.

## Prompts e instruções relevantes

As solicitações técnicas estão resumidas a partir do enunciado e das referências fornecidas pela Bruna.

| Solicitação                                                          | Contribuição do agente Nemo                                                                                          | Resultado nesta versão                                                                                                                                 |
| -------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Implementar as telas com base no protótipo e nos arquivos fornecidos | Geração e adaptação de componentes e estilos                                                                         | Landing page, detalhes de curso e AVA demonstrativa                                                                                                    |
| Construir o catálogo com busca e filtros combináveis                 | Estruturação do modelo de dados e da lógica de consulta                                                              | Sete cursos, três categorias, duas modalidades e três status; busca, filtros, contagem e limpeza                                                       |
| Adaptar a navegação a diferentes larguras de tela                    | Implementação de grades responsivas, menus e controles nativos                                                       | Layouts para computador e telas estreitas, com labels e foco visível                                                                                   |
| Verificar a implementação e documentar seu funcionamento             | Execução de compilação, testes e revisão no navegador; elaboração da documentação                                    | Instruções de execução, resultados de validação e limitações registradas                                                                               |
| Conferir as divergências nas capturas fornecidas                     | Comparação visual e ajustes nos componentes compartilhados e nas três telas de referência                            | Botões, cores, tipografia, grades, ícones e proporções revistos; limites e adaptações registrados                                                      |
| Documentar as responsabilidades e decisões próximas ao código        | Revisão de comentários C# e TypeScript e elaboração de um mapa da arquitetura                                        | Inicialização, contratos, consultas, integridade, carga inicial e estados de integração localizados por arquivo                                        |
| Construir o backend e integrar cursos, módulos e aulas               | Implementação de ASP.NET Core, EF Core, SQLite, migration e carga inicial; adaptação do Angular para consultar a API | Listagem e detalhes persistidos, textos e aulas fornecidos pelo servidor, cancelamento de requisições substituídas e pacote integrado                  |
| Reproduzir os efeitos dos botões informados pela Bruna               | Animação CSS compartilhada das setas; revisão das cores e retirada da elevação dos botões                            | Saída pela direita e retorno pela esquerda enquanto há hover, respeitando redução de movimento; duração aproximada a partir da descrição da referência |
| Publicar a aplicação integrada em hospedagem gratuita                | Elaboração do Dockerfile e do .dockerignore, configuração do serviço Free, validação HTTP e revisão no navegador     | Angular e API publicados na mesma origem; 15 verificações HTTP e navegação integrada conferidas no endereço público                                    |

## Avaliação e aplicação das sugestões

Bruna realiza a revisão integral dos commits antes de autorizar seu registro e das pull requests (PRs) antes de aprovar o merge na branch `main`.

- **Aplicadas:** componentes Angular standalone, estado por signals, rotas sob demanda, comentários de decisões técnicas e testes da consulta HTTP com SQLite e das regras de progresso local.
- **Adaptadas:** uso das imagens disponíveis nos anexos, inclusão dos campos exigidos pelo teste e criação da tela de catálogo com a mesma linguagem visual das referências.
- **Conteúdo de referência:** títulos e descrições dos seis cursos em destaque, cartão “Todos os cursos” e cinco depoimentos reproduzidos das capturas. FAQ completa e textos de “Boas práticas” e “Recomendações” da AVA transcritos das capturas do curso de Matemática para a Precisão. A estrutura sem conteúdo didático fornecido usa rótulos neutros de módulos e aulas. As descrições de Aula 1 são identificadas na interface como “Exemplo fictício”, usando os tópicos fornecidos de Matemática e as descrições curtas dos cursos como base. Carga horária, modalidade, público, status e datas ausentes usam dados de teste; a oferta complementar de Gestão de Resíduos contempla a terceira categoria do catálogo.
- **Demonstração de conclusão de aula:** o botão “Marcar como concluída” foi acrescentado à AVA, com aprovação da Bruna, para demonstrar a conclusão de uma aula e a atualização do progresso, como é comum em cursos online. O controle não consta no Figma; permite desfazer a marcação e utiliza somente o armazenamento deste navegador, sem matrícula, avaliação ou certificação.
- **Entrada e conteúdo introdutório da AVA:** a abertura considera a primeira aula ainda não concluída na ordem do curso. Introdução, Extras e Boas práticas sem conteúdo de referência usam descrições breves identificadas como “Exemplo fictício”. Uma migration preenche os campos ainda vazios em bancos existentes, preservando os textos de referência e as descrições já preenchidas.
- **Valores variáveis e configurações:** o header recebe o curso exibido nos detalhes para montar o acesso à AVA; o rodapé obtém nomes e slugs pela API. Nome do projeto nos títulos das rotas, contato, curso de referência e limite de busca da interface foram centralizados em configuração pública. Conteúdos visuais e institucionais da referência permanecem identificados como demonstrativos.
- **Análise de escopo:** as funcionalidades do catálogo foram definidas a partir dos requisitos do enunciado. As menções a provas, reconhecimento facial e certificados no protótipo foram consideradas conteúdo descritivo dos cursos.
- **Navegação:** Bruna confirmou o fluxo de “Matricule-se”: bloco vermelho da landing page → detalhes do curso de referência → faixa preta fixa → AVA. Esse fluxo foi aplicado. Os botões “Todos os cursos” não têm destino clicável no protótipo; nesta implementação, abrem a tela de busca e filtros exigida no enunciado.

## Validação dos resultados

As verificações desta versão foram executadas pelo agente Nemo:

- `npm run build` na raiz: compilação do Angular, restauração com lockfiles e publicação Release da API concluídas; frontend copiado para o pacote integrado.
- `npm --prefix frontend test`: 12 testes de busca de aulas, progresso local e seleção da primeira aula pendente aprovados.
- `dotnet test`: 17 testes de integração aprovados, com requisições HTTP, migrations e arquivos SQLite temporários; busca, filtros combinados, validação, detalhes, integridade, persistência após reinício e atualização dos itens introdutórios de um banco anterior.
- Pacote Release: requisições HTTP verificaram arquivos do frontend, acesso direto às rotas, consulta, detalhes, health check, OpenAPI e 404 de API.
- Revisão no navegador: navegação entre páginas, filtros, resultado vazio, painéis independentes, acesso demonstrativo, busca e avanço de aulas, recuperação do progresso local, layout estreito e navegação por teclado.
- Publicação no Render Free: imagem Docker construída, migrations aplicadas e 15 verificações HTTP aprovadas. No endereço público, foram conferidos filtros combinados, recarga da seleção, limpeza, acesso à AVA do curso selecionado e retomada da primeira aula pendente.

A revisão visual inicial usou o build do frontend. O pacote integrado foi conferido por HTTP no executor, além dos testes de integração. No Windows, Bruna confirmou a retomada da primeira aula pendente, a permanência no curso selecionado ao acessar a AVA e recarregar a página e o funcionamento do layout móvel. A aplicação integrada também foi publicada e revisada em [test-dev-sesi.onrender.com](https://test-dev-sesi.onrender.com/), incluindo a seleção preservada após recarregar os filtros. A recuperação após indisponibilidade da API não foi provocada no navegador público. O alcance das verificações está em `docs/VALIDACAO.md`.

## Exemplo de inadequação e correção

A configuração inicial sugerida para a prévia utilizou opções de servidor incompatíveis com o Angular CLI: `--strictPort` e o formato textual de `--allowed-hosts`. O comando foi recusado.

Após consultar a ajuda e o schema da versão instalada, as opções incompatíveis foram removidas e a lista de hosts foi configurada em `angular.json`. Uma limitação adicional da infraestrutura de prévia exigiu utilizar o build estático, compilado com sucesso, para a inspeção no navegador. O desenvolvimento local permanece disponível por `npm start`.
