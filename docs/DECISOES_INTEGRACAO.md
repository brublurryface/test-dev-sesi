# Decisões da integração

Este registro complementa as decisões de interface e documenta a versão integrada de 23/10/2026.

| Decisão                                 | Motivo e consequência                                                                                                                                                                                      |
| --------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| ASP.NET Core Minimal API e EF Core      | O catálogo de leitura tem poucos endpoints; contratos, validação, persistência e configuração ficam em arquivos separados, com dependências injetadas e testes HTTP                                        |
| SQLite                                  | Banco open source adequado ao volume demonstrativo; dispensa serviço externo e pode ser recriado com migration e carga inicial                                                                             |
| Migration e carga em transação          | O esquema é versionado e a carga inicial é aplicada apenas quando ainda não há cursos; reinícios preservam dados existentes                                                                                |
| Entidades e DTOs distintos              | A resposta não expõe navegações do EF, chaves internas ou ciclos; a listagem transfere somente os dados utilizados nos cartões                                                                             |
| Relacionamentos no banco                | Slugs únicos, carga horária positiva e chaves estrangeiras protegem a consistência; uma aula não pode pertencer ao módulo de outro curso                                                                   |
| Busca normalizada e filtros no SQL      | Acentos e caixa são tratados no servidor; todos os critérios são combinados e parametrizados pelo EF, sem concatenar a busca em SQL                                                                        |
| Sem paginação na demonstração           | Sete cursos permitem entregar a coleção inteira e uma contagem simples; paginação passa a ser necessária com o crescimento do catálogo                                                                     |
| Textos estruturados fornecidos pela API | Introdução, aulas, FAQ e orientações ficam associados ao curso no banco; o Angular interpola texto em elementos HTML, sem inserir HTML remoto                                                              |
| Stores locais às páginas                | Home, catálogo, detalhes e AVA mantêm estados de carregamento e erro próprios; uma consulta não sobrescreve os resultados de outra página                                                                  |
| Cancelamento e pausa de 250 ms na busca | Uma resposta antiga não substitui filtros novos; a pausa reduz requisições durante a digitação e os seletores continuam imediatos                                                                          |
| Mesma origem na publicação              | ASP.NET Core entrega Angular e API; o proxy mantém esse mesmo caminho `/api` durante o desenvolvimento                                                                                                     |
| Progresso demonstrativo no navegador    | O botão de concluir aula, acrescentado à implementação, demonstra a interação comum de conclusão e atualização do progresso. Pode ser desfeito; conta, matrícula e sincronização não fazem parte do escopo |
| Administração e integrações externas    | Autenticação, CRUD, provas e certificados não foram implementados. O diferencial opcional de leitura do Moodle, limitado ao conteúdo de um curso, está documentado em [MOODLE.md](MOODLE.md).              |

O JSON é uma fonte de carga inicial, não uma alternativa de consulta do frontend. Não há troca silenciosa para dados locais quando a API falha. As limitações de imagens, vídeos e conteúdos demonstrativos permanecem registradas no README.

A entrada na AVA considera o progresso salvo por curso e seleciona a primeira aula pendente; um curso concluído volta à primeira aula para revisão. Itens introdutórios sem texto de referência recebem descrições breves identificadas como “Exemplo fictício”. Uma migration preenche apenas campos ainda vazios em bancos existentes, sem recarregar o catálogo sobre os dados persistidos.
