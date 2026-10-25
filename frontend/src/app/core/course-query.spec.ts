import { test } from 'node:test';
import assert from 'node:assert/strict';
import { normalizeText } from './course-query.ts';

test('a busca de aulas ignora acentos, espaços externos e diferença de caixa', () => {
  assert.equal(normalizeText('  INTRODUÇÃO  '), 'introducao');
  assert.equal(normalizeText('Módulo 1'), 'modulo 1');
});
