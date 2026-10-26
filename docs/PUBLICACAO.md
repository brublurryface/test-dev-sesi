# Pacote integrado e publicação

O pacote de publicação reúne API, dependências, carga inicial e frontend compilado. A demonstração está publicada em [test-dev-sesi.onrender.com](https://test-dev-sesi.onrender.com/) no plano gratuito do Render. Este documento descreve a execução local, a configuração efetiva da hospedagem e seus limites.

## Gerar o pacote

Instale Node.js 24, npm 11 e SDK .NET 10 compatível com `global.json`. Na raiz do código-fonte:

```bash
npm --prefix frontend ci
npm run build
```

O comando compila o Angular, restaura os pacotes .NET conforme os lockfiles, publica a API em Release e copia o frontend para `artifacts/publish/wwwroot`. O destino é recriado a cada build; não use essa pasta como armazenamento permanente de um banco próprio.

## Executar a publicação

No computador de destino, instale o SDK .NET 10, que inclui os runtimes necessários. Em uma instalação somente de runtimes, são necessários .NET Runtime 10 e ASP.NET Core Runtime 10. Extraia o ZIP de publicação e, na pasta que contém a DLL, execute:

```bash
dotnet SesiIndustria.Api.dll --urls http://localhost:5080
```

Abra `http://localhost:5080` e verifique `/api/health`. O catálogo é criado automaticamente no primeiro início. O diretório `App_Data` precisa permitir escrita. A publicação é dependente do runtime, sem executável específico de um sistema operacional.

### Atualizar uma instalação local

Encerre o processo anterior com Ctrl+C e extraia o novo pacote em uma pasta separada. Se quiser manter o banco da instalação anterior, copie a pasta `App_Data` para a nova pasta de publicação antes de iniciar. As migrations pendentes são aplicadas automaticamente; a atualização dos itens introdutórios preenche apenas campos vazios. O progresso das aulas permanece associado ao curso no armazenamento do navegador.

## Configuração de hospedagem

| Configuração                        | Uso                                                                  |
| ----------------------------------- | -------------------------------------------------------------------- |
| `ASPNETCORE_URLS` ou `--urls`       | Endereço de escuta da API, com a porta exigida pelo provedor         |
| `Database__Path`                    | Caminho do arquivo SQLite; relativo à pasta da aplicação ou absoluto |
| `ASPNETCORE_ENVIRONMENT=Production` | Ambiente de publicação                                               |

Frontend e API usam a mesma origem. O ASP.NET Core entrega arquivos estáticos e utiliza `index.html` para as rotas da SPA. URLs desconhecidas sob `/api` retornam 404 em JSON, sem receber a página Angular.

Não são necessários serviços pagos, chaves, contas de alunos ou servidor de banco separado. Em um provedor com disco temporário, a perda do arquivo SQLite recria a carga demonstrativa no início seguinte. Para manter alterações administrativas fora do escopo desta demonstração, configure armazenamento persistente. O progresso das aulas permanece no navegador.

## Conferência do ambiente público

A publicação passou por 15 verificações HTTP, incluindo `/api/health`, consultas com filtros, detalhes, OpenAPI, acesso direto às rotas e respostas 400/404 em JSON. No navegador público, foram conferidos busca com filtros combinados, recarga da seleção, estado vazio, limpeza, acesso à AVA do curso selecionado e retomada da primeira aula pendente. Os resultados e o alcance estão em [VALIDACAO.md](VALIDACAO.md). A recuperação após indisponibilidade da API não foi provocada nesse ambiente.

## Execução em Docker

O `Dockerfile` na raiz compila o Angular com Node.js 24.19.0 e a API com SDK .NET 10.0.402. A imagem final usa ASP.NET Core Runtime 10.0.13, serve ambos no mesmo processo e executa com usuário sem privilégios. A pasta `App_Data` recebe permissão para criar o SQLite; migrations e carga inicial continuam sendo aplicadas no início da aplicação. O `.dockerignore` limita os arquivos enviados ao build e exclui bancos, dependências instaladas e resultados de compilação locais.

Com Docker disponível, execute na raiz do código-fonte:

```bash
docker build --tag test-dev-sesi .
docker run --rm -p 5080:10000 test-dev-sesi
```

Abra `http://localhost:5080`. A variável `PORT` define a porta interna, com padrão `10000`; o servidor escuta em `0.0.0.0`. O comando de entrada usa `exec` para o processo .NET receber o sinal de encerramento. Esse comando local não conserva o banco depois que o contêiner é removido.

O Docker não é necessário para executar o ZIP integrado localmente com .NET.

## Publicação no Render gratuito

O **Web Service** `test-dev-sesi` foi criado no workspace `TEST-DEV-SESI`, a partir do repositório [brublurryface/test-dev-sesi](https://github.com/brublurryface/test-dev-sesi). A primeira publicação validada utiliza o commit `ee40f19`, de `feat/backend`; seu conteúdo foi integrado à `main` na [PR #1](https://github.com/brublurryface/test-dev-sesi/pull/1). Configurações efetivas:

| Campo                        | Valor configurado                                |
| ---------------------------- | ------------------------------------------------ |
| Nome                         | `test-dev-sesi`                                  |
| Branch                       | `feat/backend` no deploy validado                |
| Linguagem/runtime            | Docker                                           |
| Diretório raiz               | Raiz do repositório                              |
| Dockerfile                   | `./Dockerfile`                                   |
| Contexto de build            | Raiz do repositório                              |
| Plano da instância           | **Free**                                         |
| Health check                 | TCP padrão; `/api/health` verificado por HTTP    |
| Deploy automático            | Desativado; novas publicações passam por revisão |
| Comando Docker personalizado | Vazio; usar o comando definido no Dockerfile     |

O provedor fornece `PORT`; não é necessário cadastrar chaves ou serviços de banco adicionais. O caminho padrão `App_Data/catalog.db` é suficiente para esta demonstração. O endereço atribuído ao serviço é `https://test-dev-sesi.onrender.com/`. O deploy automático está desativado; novas versões exigem uma publicação manual. A troca da branch de origem e a configuração opcional de health check HTTP podem ser feitas em Settings; o deploy validado usa as configurações da tabela.

### Limites do plano gratuito

Segundo a documentação do Render consultada em 25/10/2026, a instância gratuita é suspensa após 15 minutos sem tráfego; o primeiro acesso seguinte pode levar aproximadamente um minuto para reativá-la. O sistema de arquivos é temporário: reinícios, novas publicações e suspensão por inatividade podem descartar o arquivo SQLite. Não há disco persistente nesse plano.

O catálogo é somente de consulta e possui carga fictícia versionada. Quando o banco não existe, a aplicação aplica as migrations e importa novamente os sete cursos. Isso atende à demonstração; alterações manuais no arquivo não permanecem após sua perda. O progresso de aulas fica no navegador e não depende da conservação do SQLite.

Usar somente a instância Free, sem serviços adicionais pagos. Os limites de horas, tráfego e minutos de build são compartilhados com o workspace. Antes da criação, conferir a cota disponível e a configuração de gastos; não autorizar complementos pagos. A cópia local e o ZIP integrado permitem avaliar a aplicação mesmo durante indisponibilidade temporária do provedor.

### Referências da plataforma

- [Docker no Render](https://render.com/docs/docker)
- [Plano gratuito e suas limitações](https://render.com/docs/free)
- [Health checks](https://render.com/docs/health-checks)

A imagem Docker foi construída e executada no Render em 25/10/2026. Os logs confirmaram a aplicação das duas migrations, a inicialização em `0.0.0.0:10000` e o deploy no estado `live`. A consulta pública `/api/health` respondeu 200 com `{"status":"ok"}` e o catálogo retornou os sete cursos.
