import { test } from 'node:test';
import assert from 'node:assert/strict';
import { calculateLearningProgress, restoreLearningProgress } from './learning-progress.ts';

test('inicia sem progresso quando o navegador ainda não possui registro', () => {
  assert.deepEqual(restoreLearningProgress(null, ['a1']), []);
});

test('tolera JSON inválido e formatos locais inesperados', () => {
  for (const raw of ['registro inválido', '{}', 'true', '[1, null, false]']) {
    assert.deepEqual(restoreLearningProgress(raw, ['a1']), []);
  }
});

test('restaura aulas válidas uma única vez e ignora aulas removidas do curso', () => {
  assert.deepEqual(restoreLearningProgress('["a1","a1","removida","a2"]', ['a1', 'a2']), [
    'a1',
    'a2',
  ]);
});

test('um curso sem aulas tem progresso zero', () => {
  assert.equal(calculateLearningProgress([], ['a1']), 0);
});

test('calcula o percentual com base nas aulas distintas do curso', () => {
  assert.equal(calculateLearningProgress(['a1', 'a2', 'a3'], ['a1']), 33);
});

test('identificadores repetidos e desconhecidos não aumentam o progresso', () => {
  assert.equal(calculateLearningProgress(['a1', 'a2'], ['a1', 'a1', 'removida']), 50);
});

test('concluir todas as aulas mantém o limite de cem por cento', () => {
  assert.equal(calculateLearningProgress(['a1', 'a2'], ['a1', 'a2', 'a2', 'removida']), 100);
});
