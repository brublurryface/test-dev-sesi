# Uso de inteligência artificial

Projeto **TEST DEV SESI Indústria**. Registro referente à versão de frontend de 23/10/2026.

## Ferramenta e contribuição

Foi utilizado o **agente Nemo, do ChatGPT/Codex**, como apoio à análise dos requisitos, à implementação e à verificação do projeto. A contribuição incluiu geração e adaptação de componentes Angular, estilos, dados demonstrativos, funções de busca e filtros, testes e documentação técnica.

## Referências fornecidas pela Bruna

Bruna forneceu o enunciado, os arquivos de apoio e as capturas do protótipo, incluindo a landing page, os detalhes do curso, os painéis expandidos e a AVA. Esses materiais orientaram a estrutura das páginas, os conteúdos e os comportamentos implementados nesta versão.

## Prompts e instruções relevantes

As solicitações técnicas estão resumidas a partir do enunciado e das referências fornecidas pela Bruna.

| Solicitação | Contribuição do agente Nemo | Resultado nesta versão |
| --- | --- | --- |
| Implementar as telas com base no protótipo e nos arquivos fornecidos | Geração e adaptação de componentes e estilos | Landing page, detalhes de curso e AVA demonstrativa |
| Construir o catálogo com busca e filtros combináveis | Estruturação do modelo de dados e da lógica de consulta | Sete cursos, três categorias, duas modalidades e três status; busca, filtros, contagem e limpeza |
| Adaptar a navegação a diferentes larguras de tela | Implementação de grades responsivas, menus e controles nativos | Layouts para computador e telas estreitas, com labels e foco visível |
| Verificar a implementação e documentar seu funcionamento | Execução de compilação, testes e revisão no navegador; elaboração da documentação | Instruções de execução, resultados de validação e limitações registradas |
| Conferir as divergências em todas as capturas fornecidas | Comparação visual das capturas fornecidas e ajustes nos componentes compartilhados e nas três telas de referência | Botões, cores, tipografia, grades, ícones e proporções revistos; limites e adaptações registrados |

## Avaliação e aplicação das sugestões

Bruna realiza a revisão integral dos commits antes de autorizar seu registro e das pull requests (PRs) antes de aprovar o merge na branch `main`.

- **Aplicadas:** componentes Angular standalone, estado por signals, rotas sob demanda, comentários de decisões técnicas e testes das regras de consulta e progresso local.
- **Adaptadas:** uso das imagens disponíveis nos anexos, inclusão dos campos exigidos pelo teste e criação da tela de catálogo com a mesma linguagem visual das referências.
- **Conteúdo de referência:** títulos e descrições dos seis cursos em destaque, cartão “Todos os cursos” e cinco depoimentos reproduzidos das capturas. FAQ completa e textos de “Boas práticas” e “Recomendações” da AVA transcritos das capturas do curso de Matemática para a Precisão. A estrutura sem conteúdo didático fornecido usa rótulos neutros de módulos e aulas. As descrições de Aula 1 são identificadas na interface como “Exemplo fictício”, usando os tópicos fornecidos de Matemática e as descrições curtas dos cursos como base. Carga horária, modalidade, público, status e datas ausentes usam dados de teste; a oferta complementar de Gestão de Resíduos contempla a terceira categoria do catálogo.
- **Demonstração de conclusão de aula:** o botão “Marcar como concluída” foi acrescentado à AVA, com aprovação da Bruna, para demonstrar a conclusão de uma aula e a atualização do progresso, como é comum em cursos online. O controle não consta no Figma; permite desfazer a marcação e utiliza somente o armazenamento deste navegador, sem matrícula, avaliação ou certificação.
- **Análise de escopo:** as funcionalidades do catálogo foram definidas a partir dos requisitos do enunciado. As menções a provas, reconhecimento facial e certificados no protótipo foram consideradas conteúdo descritivo dos cursos.
- **Navegação:** Bruna confirmou o fluxo de “Matricule-se”: bloco vermelho da landing page → detalhes do curso de referência → faixa preta fixa → AVA. Esse fluxo foi aplicado. Os botões “Todos os cursos” não têm destino clicável no protótipo; nesta implementação, abrem a tela de busca e filtros exigida no enunciado.

## Validação dos resultados

As verificações desta versão foram executadas pelo agente Nemo:

- `npm run build`: compilação de produção concluída, com verificação estrita dos templates Angular.
- `npm test`: testes das regras de normalização da busca, combinação de filtros, limpeza da consulta e consistência do progresso local. A quantidade e os resultados estão registrados em `docs/VALIDACAO.md`.
- Revisão no navegador: navegação entre páginas, filtros, resultado vazio, painéis independentes, acesso demonstrativo, busca e avanço de aulas, recuperação do progresso local, layout estreito e navegação por teclado.

O alcance das verificações e os pontos ainda não verificados estão registrados em `docs/VALIDACAO.md`. O estado da integração com backend e banco está descrito no README.

## Exemplo de inadequação e correção

A configuração inicial sugerida para a prévia utilizou opções de servidor incompatíveis com o Angular CLI: `--strictPort` e o formato textual de `--allowed-hosts`. O comando foi recusado.

Após consultar a ajuda e o schema da versão instalada, as opções incompatíveis foram removidas e a lista de hosts foi configurada em `angular.json`. Uma limitação adicional da infraestrutura de prévia exigiu utilizar o build estático, compilado com sucesso, para a inspeção no navegador. O desenvolvimento local permanece disponível por `npm start`.
