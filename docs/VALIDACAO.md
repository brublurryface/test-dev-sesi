# TEST DEV SESI Indústria — Validação da primeira etapa

Verificações executadas pelo agente Nemo, do ChatGPT/Codex, em 22 e 23/10/2026. O registro apresenta os resultados observados e o alcance das verificações desta versão.

| Verificação | Resultado |
| --- | --- |
| Instalação de dependências | Concluída com npm |
| Compilação de produção e templates estritos | Passou |
| Formatação padronizada com Prettier | Verificada por npm run format:check |
| Testes automatizados das regras de negócio | Dez testes aprovados |
| Consistência do catálogo | Arquivo conferido: sete cursos, seis em destaque, três categorias, duas modalidades e três status; IDs e slugs únicos |
| Busca sem acentos, caixa e espaços externos | Teste automatizado passou |
| Combinação de nome, categoria, modalidade e status | Teste automatizado passou |
| Limpeza e preservação da coleção original | Teste automatizado passou |
| Rolagem ao buscar, selecionar e limpar filtros | Verificada no navegador: lista e URL atualizadas sem retorno ao topo; abertura dos detalhes e retorno pelo histórico preservados |
| Restauração de progresso ausente ou inválido | Testes automatizados passaram |
| Identificadores duplicados ou de aulas removidas | Testes automatizados passaram; não alteram a contagem de aulas concluídas |
| Progresso de curso vazio, parcial e completo | Testes automatizados passaram; percentual preservado entre zero e cem |
| Navegação início → catálogo → detalhes → AVA | Verificada no navegador |
| Links “Cursos” do header e “Conhecer cursos” do hero | Verificados em 23/10: rolagem suave até “Principais cursos”; repetição e retorno a partir de outras páginas preservados. Título a 24 px do header fixo: 106 px no desktop e 100 px no frame de 390 × 844 px; menu móvel fechado após selecionar Cursos |
| Item “Sobre” do header | Link sem destino definido: preventDefault preserva a posição e fecha o menu móvel, conforme a orientação atualizada da Bruna |
| Tom do hero e link “Área do aluno” | Revistos com as capturas: foto ampliada e reposicionada, camada vermelha uniforme; link sem seta, com cantos arredondados. Sobre o hero, fundo transparente e contorno branco discreto visível em repouso; escurecimento no hover, conforme o recorte sem mouse fornecido em 23/10; no header branco, contorno cinza. Destino à AVA conferido em desktop e no menu de um frame de 390 × 844 px, sem transbordamento horizontal |
| Fluxo dos botões “Matricule-se” | Verificado no navegador: bloco vermelho da landing page → detalhes de Matemática para a Precisão → faixa preta fixa → AVA, sem aviso intermediário na faixa |
| Combinação de busca e três filtros na interface | Retornou o único curso esperado |
| Estado vazio e botão de limpeza | Verificados novamente após ampliar o catálogo; a limpeza devolveu os sete cursos |
| Busca por MATEMATICA sem acento | Retornou o curso de Matemática para a Precisão |
| Painéis independentes nos detalhes | Três abertos simultaneamente; dez módulos e dez perguntas |
| Curso em breve | Botão de matrícula desabilitado |
| AVA | Busca e recuperação de progresso verificadas; em 23/10, rótulos “Módulo 1, 2…” e uma “Aula 1” por módulo conferidos; descrições identificadas como “Exemplo fictício” verificadas na AVA e no painel dos detalhes. Marcação, reinício e avanço ao próximo módulo também verificados |
| Origem do conteúdo | Conferidos os 28 módulos do JSON: uma Aula 1 por módulo, com descrição breve iniciada por “Exemplo fictício:”. Os textos reutilizam tópicos de Matemática ou a descrição curta do curso. Nos demais cursos, apresentações ampliadas, tópicos didáticos e FAQ genérica foram removidos; no curso de referência, dez módulos e dez perguntas preservados |
| Layout estreito | Revisado em um frame de 390 × 844 px; sem transbordamento horizontal em início, catálogo, detalhes e AVA |
| Menu em layout estreito | Abertura e navegação verificadas, incluindo o painel de conteúdo da AVA |
| Teclado | Tab entre busca e selects; fechamento de diálogo com Escape verificado |
| Alinhamento do carrossel | Ajustado e conferido com a margem do conteúdo |
| Cartão Todos os cursos | Fundo branco explícito, texto/seta pretos na parte inferior, cantos arredondados e sombra conforme a captura adicional de 23/10; acesso ao catálogo preservado. No frame de 320 × 844 px, texto e seta cabem no cartão; a landing page ficou com conteúdo de 305 px, igual à área útil, após ajuste dos controles e da disposição dos indicadores |
| Fim do carrossel de cursos | Seis ofertas e cartão “Todos os cursos” conferidos no navegador; o cartão abriu o catálogo |
| Curso de Ciência dos Materiais | Encontrado por nome sem acentos e pelos três filtros combinados; retornou um único resultado |
| Depoimentos | Cinco nomes e textos conferidos com as capturas; Gabriel Santos incluído. Fundo #890000 e cartões #C3222C na landing page; fundo #FCD19C e cartões brancos nos detalhes. Estrelas abaixo do nome e cargo, conforme as referências |
| Revisão das 31 capturas fornecidas | Conferidos hero, header, cartões, indicadores, benefícios, depoimentos, chamadas, rodapé, detalhes, painéis e AVA. Ajustados botões retangulares com cantos arredondados, cores, tipografia, grades, proporções dos vídeos, ícones de data/certificação e banner da AVA. Reconstrução visual a partir de capturas; sem equivalência pixel a pixel certificada |
| Revisão visual em 1920 × 1080 px | Navegador em frame nessa dimensão: título do curso em três linhas; vídeo em 16:9; faixa fixa de matrícula alinhada ao conteúdo; banner da AVA com proporção 6:1 e raio de 12 px. Pequenas diferenças de área útil decorrem das barras de rolagem do navegador |
| Revisão após os ajustes em 390 × 844 px | Início, catálogo, detalhes e AVA com largura de conteúdo igual à área útil de 375 px; sem transbordamento horizontal. Âncora de cursos a 100 px; painel móvel de conteúdo da AVA abriu e fechou |
| Movimento dos carrosséis | Avanço e retorno de cursos e depoimentos verificados. Cursos também conferidos com comandos sucessivos até o limite; encaixe e estilos restaurados após a animação. Duração de 700 ms definida no utilitário compartilhado; redução de movimento tratada no código, sem emulação dessa preferência nesta revisão |
| Imagens e fonte fornecidas | Carregadas na revisão visual |
| Texto do curso de referência | FAQ completa e seções “Boas práticas” e “Recomendações” conferidas com as capturas fornecidas |

O primeiro comando de prévia falhou por incompatibilidade de opções do servidor. A configuração foi ajustada a partir do schema do Angular CLI instalado. Uma segunda falha, na leitura de memória dentro da infraestrutura de prévia, impediu a compilação em modo de desenvolvimento nesse ambiente. A revisão passou a usar o build estático compilado com sucesso no executor. Nenhuma verificação de hosts foi desativada.

As 28 descrições de aulas do dataset demonstrativo estão preenchidas e identificadas como exemplos fictícios. A integração desses dados com API e banco permanece para a próxima etapa.

Ainda não foram realizados testes de integração com API ou banco, pois eles não existem nesta etapa. A aplicação não está publicada. Não foi feita auditoria completa com leitores de tela ou validação exaustiva em dispositivos físicos.

O estado de falha de rede está implementado, mas não foi provocado na revisão pelo navegador. A recarga direta de rotas não pôde ser validada na prévia estática, que não configura o redirecionamento de SPA. A hospedagem final precisa dessa regra; o modo de desenvolvimento deve ser executado com `npm start` conforme o README. Os filtros na URL foram observados durante a navegação; a restauração após recarga ainda precisa dessa verificação em um servidor com a regra adequada.

Em 23/10, os botões com seta receberam animação compartilhada de saída pela direita e retorno pela esquerda. A elevação geral dos botões no hover foi removida. Hero com fundo de hover #E6E6E6; botão contornado Ver todos os cursos com #F5F1EC. Conferidos no navegador o ciclo da seta no hero, a aplicação do mesmo efeito em Matricule-se e Mandar uma mensagem e a interrupção ao sair do hover. A conclusão demonstrativa alterou o progresso de zero para 8%; seu acréscimo à implementação está justificado no README e no USO_DE_IA.
