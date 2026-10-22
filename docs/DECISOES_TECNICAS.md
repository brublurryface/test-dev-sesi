# Decisões técnicas e oportunidades de evolução

Registro da versão de frontend de 22/10/2026. O estado da integração e as instruções de execução estão no README.

## Arquitetura

| Decisão | Motivo | Consequência |
| --- | --- | --- |
| Componentes standalone e páginas carregadas sob demanda | Organizar cada área da aplicação e distribuir o carregamento por rota | Páginas e componentes têm responsabilidades delimitadas |
| Serviço compartilhado para os cursos | Concentrar o acesso à origem de dados e os estados de carregamento e erro | As páginas observam a mesma coleção; a futura integração fica concentrada nessa camada |
| Estado por signals e listas derivadas por computed | Manter contagens e resultados coerentes com a consulta | A lista filtrada é recalculada quando a seleção ou os dados mudam |
| Consulta em função pura | Isolar a regra de busca e combinação de critérios | A regra pode ser testada sem renderizar componentes ou iniciar o Angular |
| Filtros na URL | Permitir endereços que representem uma seleção do catálogo | A restauração em acesso direto exige o redirecionamento de SPA descrito no README |
| Controles nativos de HTML | Aproveitar os comportamentos de teclado e foco oferecidos pelo navegador | Painéis independentes usam details/summary; avisos usam dialog |
| Progresso local por identificador de aula | Permitir a demonstração de navegação entre aulas | O progresso pertence ao navegador e ao curso; autenticação e sincronização não fazem parte desta implementação |

## Melhorias incorporadas

- **Busca tolerante a acentos e caixa:** facilita encontrar um curso quando o texto digitado difere da grafia do título.
- **Filtros combinados e limpeza:** a consulta aplica todos os critérios ativos e mantém a contagem derivada do mesmo resultado.
- **Histórico de navegação controlado:** replaceUrl evita criar uma entrada por caractere digitado.
- **Parâmetros reconhecidos:** categoria, modalidade e status recebidos pela URL são comparados com os valores válidos do catálogo.
- **Progresso consistente:** identificadores repetidos ou de aulas removidas são ignorados, mantendo o percentual entre zero e cem.
- **Recuperação de registro local inválido:** dados antigos ou JSON inválido iniciam um progresso vazio; falhas de acesso ao armazenamento mantêm a navegação disponível na sessão.
- **Legibilidade do código:** comentários registram motivos, contratos e limites de integração; a formatação padroniza a leitura dos arquivos.

## Prioridades para a entrega completa

O enunciado exige backend C#/.NET, banco open source e integração efetiva. Essas atividades permanecem previstas para a próxima etapa, junto com a publicação e sua documentação de execução.

Na integração, é necessário definir o contrato dos endpoints, validar os dados no servidor, persistir cursos/módulos/aulas, configurar o endereço da API e verificar os estados de erro com requisições reais. O dataset JSON desta versão deve ser substituído pela origem integrada.

## Melhorias recomendadas além do mínimo

| Oportunidade | Benefício | Quando aplicar |
| --- | --- | --- |
| Imagens dimensionadas por contexto e formatos mais compactos | Reduzir o volume transferido em cartões e telas estreitas, preservando a referência visual | Após obter as imagens definitivas e comparar qualidade e tamanho |
| Paginação e consultas no servidor | Manter o custo da listagem previsível quando o catálogo crescer | Ao definir os endpoints e o volume esperado de cursos |
| Pequeno intervalo antes de enviar uma busca à API | Evitar uma requisição por caractere durante a digitação | Quando a consulta passar a depender do servidor |
| Testes de integração dos endpoints e do fluxo de consulta | Conferir a combinação de filtros com os dados persistidos | Após implementar a API e o banco |
| Verificação de acesso direto e recarga das rotas publicadas | Garantir que links compartilhados funcionem no ambiente de entrega | Durante a configuração da hospedagem e sua validação |
| Revisão de acessibilidade com ferramenta automatizada e leitor de tela | Ampliar a verificação de semântica, contraste, foco e mensagens de estado | Antes da entrega final, além das verificações de teclado já registradas |

As recomendações desta seção são propostas de evolução. Os resultados efetivamente verificados constam em `VALIDACAO.md`.
