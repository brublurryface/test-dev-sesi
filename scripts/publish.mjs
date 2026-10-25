import { spawnSync } from 'node:child_process';
import { cpSync, mkdirSync, rmSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

// Executado por npm run build na raiz: compila Angular, publica a API e reúne ambos em uma origem.
const root = fileURLToPath(new URL('../', import.meta.url));
const output = path.join(root, 'artifacts', 'publish');
const npmCli = process.env.npm_execpath;
if (!npmCli) throw new Error('Execute este script com npm run build na raiz do projeto.');

function run(command, args) {
  const result = spawnSync(command, args, { cwd: root, stdio: 'inherit' });
  if (result.error) throw result.error;
  if (result.status !== 0) process.exit(result.status ?? 1);
}

// Lockfiles fixam a resolução das dependências usada na publicação.
run(process.execPath, [npmCli, '--prefix', 'frontend', 'run', 'build']);
run('dotnet', [
  'restore',
  'TestDevSesiIndustria.slnx',
  '--locked-mode',
  '--disable-parallel',
  '-m:1'
]);
// artifacts/publish é uma saída descartável: o build remove todo o seu conteúdo anterior.
// Um banco usado para conservar dados entre builds deve ficar fora daqui, configurado por Database__Path.
rmSync(output, { recursive: true, force: true });
run('dotnet', [
  'publish',
  'backend/SesiIndustria.Api',
  '-c',
  'Release',
  '--no-restore',
  '-m:1',
  '-o',
  output
]);
// Program.cs detecta index.html em wwwroot para servir o Angular e suas rotas no mesmo processo.
mkdirSync(path.join(output, 'wwwroot'), { recursive: true });
cpSync(
  path.join(root, 'frontend', 'dist', 'test-dev-sesi-industria', 'browser'),
  path.join(output, 'wwwroot'),
  { recursive: true }
);
console.log(
  'Publicação gerada em artifacts/publish. Execute dotnet SesiIndustria.Api.dll nessa pasta.'
);
