# Integração opcional com Moodle

O diferencial usa o Moodle hospedado no plano gratuito Essentials da MooDIY para fornecer o conteúdo de **Matemática para a Precisão: Medidas e Escalas na Metalurgia**. O catálogo mantém os outros seis cursos demonstrativos no SQLite. A interface Angular e a navegação da AVA permanecem as do projeto.

## Fluxo e alcance

Na inicialização, depois das migrations e da carga inicial, o backend consulta o Moodle por REST e importa nome completo, títulos dos módulos, títulos, descrições e conteúdo das páginas das aulas para o SQLite. As requisições do Angular continuam consultando a API e o banco local; não usam o token e não dependem de uma chamada ao Moodle por página.

O curso possui dez seções de módulos, com uma página “Aula 1” por módulo, além de Introdução, Boas práticas e Extras na seção geral. Boas práticas contém os textos de referência fornecidos nas capturas, incluindo Recomendações. Os demais conteúdos são identificados como exemplos fictícios. O fórum Announcements criado automaticamente pelo Moodle não é importado.

Os campos específicos da demonstração — nome curto, descrição comercial, categoria, modalidade, carga horária, público, status, imagem, data, tópicos e FAQ — permanecem no catálogo local. A integração não implementa matrícula, autenticação de alunos, notas, provas, certificados nem sincronização de progresso com Moodle. A conclusão de aulas continua sendo uma demonstração salva no navegador.

## Configuração privada

A integração fica desativada por padrão. Nenhuma credencial é necessária para executar o projeto e avaliar o catálogo completo localmente.

| Variável de ambiente | Valor                                                        |
| -------------------- | ------------------------------------------------------------ |
| `Moodle__Enabled`    | `true` para ativar                                           |
| `Moodle__BaseUrl`    | `https://test-dev-sesi.moodiy.tech/`                         |
| `Moodle__CourseId`   | `3`                                                          |
| `Moodle__CourseSlug` | `matematica-para-a-precisao`                                 |
| `Moodle__Token`      | Chave privada, fornecida diretamente ao ambiente do servidor |

No Render, preencher em **Environment**, salvar e implantar a versão que contém esta integração. Não incluir o token no frontend, README, arquivos versionados, capturas, comandos com valores literais ou parâmetros de URL. No desenvolvimento, usar configuração privada ou variáveis de ambiente da sessão. Após mudar uma página no Moodle, reiniciar/reimplantar o servidor para importar a alteração.

Na instância Moodle, o curso precisa ter o campo **Course ID number** igual ao slug configurado. O serviço personalizado “TEST DEV SESI - consulta de cursos” permite apenas:

- `core_course_get_courses_by_field`;
- `core_course_get_contents`;
- `mod_page_get_pages_by_courses`.

O serviço aceita somente usuários autorizados. A credencial de demonstração foi associada à conta administradora e tem vencimento configurado; por isso a restrição ao curso é feita na aplicação, não pela chave. Para ampliar o ambiente, adotar um usuário dedicado com permissões mínimas e limitar seu acesso ao curso. A configuração do serviço segue a [documentação oficial de web services do Moodle](https://docs.moodle.org/503/en/Using_web_services).

## Integridade e indisponibilidade

Antes de gravar, a importação confere ID/identificador do curso, dez módulos e treze páginas. Os itens introdutórios são associados pelos nomes, sem depender da ordem em que foram cadastrados. Os IDs locais das aulas permanecem estáveis para preservar o progresso já existente no navegador. A gravação usa uma única transação.

O token segue no corpo de requisições POST por HTTPS; redirecionamentos são recusados. O total das consultas tem limite de vinte segundos, e cada resposta tem limite de dois MiB. HTML das páginas é reduzido a blocos de texto, sem execução de scripts ou inserção de HTML no frontend. As mensagens externas de erro não são expostas nos logs ou no diagnóstico público.

Se uma consulta falhar, expirar ou retornar uma estrutura incompleta, o catálogo anterior é preservado. Em uma instalação nova, ele continua com a carga demonstrativa inicial. O estado informa a falha; a aplicação não apresenta essa situação como sincronização bem-sucedida.

`GET /api/integrations/moodle` informa o estado da importação **desta inicialização**:

| `state`                 | Significado                                               |
| ----------------------- | --------------------------------------------------------- |
| `disabled`              | Integração desativada; nenhuma consulta externa realizada |
| `invalid_configuration` | Configuração incompleta ou inválida; catálogo preservado  |
| `unavailable`           | Importação não concluída; catálogo preservado             |
| `synchronized`          | Conteúdo importado e gravado no SQLite                    |

`lastSuccessAt` registra a conclusão bem-sucedida nesta execução; não é um histórico persistente. A atualização é feita na inicialização, sem agendamento ou endpoint público para executar importações.

## Validação e limites

Bruna executou consultas autenticadas no PowerShell contra a instância real: curso ID 3, seção geral e dez módulos, além da página Aula 1 inicialmente cadastrada. Depois dessa consulta, o cadastro foi completado e conferido no navegador: dez páginas Aula 1, Introdução, Boas práticas e Extras.

Essa validação confirma o acesso real às três funções. Ela não equivale a uma validação do backend implantado com a integração: essa etapa depende da revisão, publicação da nova versão e configuração privada do token no servidor. O site público existente permanece na versão anterior até essa implantação.

O Moodle gratuito e a instância gratuita do Render dependem dos limites dos respectivos provedores. Expiração da chave, suspensão do ambiente ou indisponibilidade externa impedem novas importações, mas não exigem credenciais de terceiros para executar o catálogo localmente.
