# Decisões técnicas e oportunidades de evolução

Registro de 23/10/2026. O frontend consulta uma API ASP.NET Core com SQLite. As decisões de persistência e contratos estão em [DECISOES_INTEGRACAO.md](DECISOES_INTEGRACAO.md); a execução está no README.

## Arquitetura

| Decisão | Motivo | Consequência |
| --- | --- | --- |
| Componentes standalone e páginas carregadas sob demanda | Organizar cada área da aplicação e distribuir o carregamento por rota | Páginas e componentes têm responsabilidades delimitadas |
| Serviço HTTP e stores por página | Centralizar o contrato e isolar os estados de cada consulta | As páginas consultam a API; filtros e resultados de uma tela não interferem em outra |
| Estado por signals e listas derivadas por computed | Refletir a resposta HTTP e manter progresso e seleção coerentes | A contagem vem da consulta do servidor; grupos de aulas e progresso são derivados dos dados recebidos |
| Consulta no servidor e testes de integração | Exercitar filtros combinados com o banco utilizado pela aplicação | A listagem é filtrada no SQL e validada por HTTP com SQLite real |
| Filtros na URL | Permitir endereços que representem uma seleção do catálogo | A publicação integrada resolve as rotas da SPA e preserva endpoints de API |
| Controles nativos de HTML | Aproveitar os comportamentos de teclado e foco oferecidos pelo navegador | Painéis independentes usam details/summary; avisos usam dialog |
| Progresso local por identificador de aula | Demonstrar navegação e conclusão de uma aula por um controle acrescentado à implementação | O progresso pertence ao navegador e ao curso; autenticação e sincronização não fazem parte desta implementação |

## Melhorias incorporadas

- **Busca tolerante a acentos e caixa:** facilita encontrar um curso quando o texto digitado difere da grafia do título.
- **Filtros combinados e limpeza:** a consulta aplica todos os critérios ativos e mantém a contagem derivada do mesmo resultado.
- **Histórico de navegação controlado:** replaceUrl evita criar uma entrada por caractere digitado.
- **Parâmetros reconhecidos:** categoria, modalidade e status recebidos pela URL são comparados com os valores válidos do catálogo.
- **Progresso consistente:** identificadores repetidos ou de aulas removidas são ignorados, mantendo o percentual entre zero e cem.
- **Recuperação de registro local inválido:** dados antigos ou JSON inválido iniciam um progresso vazio; falhas de acesso ao armazenamento mantêm a navegação disponível na sessão.
- **Legibilidade do código:** comentários registram motivos, contratos e limites de integração; a formatação padroniza a leitura dos arquivos.

## Integração e entrega

Backend C#/.NET, banco SQLite, migration, carga inicial e integração HTTP estão implementados. Detalhes, módulos, aulas e textos são fornecidos pelo servidor. A geração do pacote integrado foi verificada; publicação e conferência do endereço público permanecem pendentes.

O botão “Marcar como concluída” não consta no Figma e foi acrescentado para demonstrar a conclusão de uma aula e a atualização do progresso, como em cursos online. A marcação é reversível e fica apenas no navegador.

## Melhorias recomendadas além do mínimo

| Oportunidade | Benefício | Quando aplicar |
| --- | --- | --- |
| Imagens dimensionadas por contexto e formatos mais compactos | Reduzir o volume transferido em cartões e telas estreitas, preservando a referência visual | Após obter as imagens definitivas e comparar qualidade e tamanho |
| Paginação | Manter o custo da listagem previsível quando o catálogo crescer | Quando o volume ultrapassar a coleção demonstrativa |
| Verificação de acesso direto e recarga das rotas publicadas | Garantir que links compartilhados funcionem no ambiente de entrega | Durante a configuração da hospedagem; o fallback já foi conferido por HTTP no pacote integrado |
| Revisão de acessibilidade com ferramenta automatizada e leitor de tela | Ampliar a verificação de semântica, contraste, foco e mensagens de estado | Antes da entrega final, além das verificações de teclado já registradas |

As recomendações desta seção são propostas de evolução. Os resultados efetivamente verificados constam em `VALIDACAO.md`.
