import { test } from 'node:test';
import assert from 'node:assert/strict';
import { filterLearningGroups, normalizeText } from './learning-navigation.ts';
import type { LearningGroup } from './learning-navigation.ts';
import type { LessonSummary } from './course.model.ts';

const lesson = (id: string, title: string): LessonSummary => ({
  id,
  title,
  description: '',
});
const groups: LearningGroup[] = [
  {
    id: 'introducao',
    title: 'Introdução',
    lessons: [
      lesson('introducao', 'Introdução'),
      lesson('boas-praticas', 'Boas práticas'),
      lesson('extras', 'Extras'),
    ],
  },
  { id: 'modulo-1', title: 'Módulo 1', lessons: [lesson('m1-a1', 'Aula 1')] },
  { id: 'modulo-2', title: 'Módulo 2', lessons: [lesson('m2-a1', 'Aula 1')] },
];

test('a busca de aulas ignora acentos, espaços externos e diferença de caixa', () => {
  assert.equal(normalizeText('  INTRODUÇÃO  '), 'introducao');
  assert.equal(normalizeText('Módulo 1'), 'modulo 1');
});

test('Boas práticas permanece 1.2 e Extras permanece 1.3 durante a busca', () => {
  const [goodPractices] = filterLearningGroups(groups, '  BÓAS PRATICAS  ');
  assert.equal(goodPractices.number, 1);
  assert.equal(goodPractices.lessons[0].number, 2);
  assert.equal(goodPractices.lessons[0].id, 'boas-praticas');
  const [extras] = filterLearningGroups(groups, 'extras');
  assert.equal(extras.number, 1);
  assert.equal(extras.lessons[0].number, 3);
});

test('buscar pelo módulo mantém suas aulas e a posição original do grupo', () => {
  const [module] = filterLearningGroups(groups, 'modulo 2');
  assert.equal(module.id, 'modulo-2');
  assert.equal(module.number, 3);
  assert.deepEqual(
    module.lessons.map(({ id, number }) => ({ id, number })),
    [{ id: 'm2-a1', number: 1 }]
  );
});

test('limpar a busca recupera a estrutura inteira sem alterar IDs nem os dados de origem', () => {
  const original = structuredClone(groups);
  filterLearningGroups(groups, 'boas');
  const restored = filterLearningGroups(groups, '');
  assert.deepEqual(
    restored.flatMap((group) => group.lessons.map((lesson) => lesson.id)),
    ['introducao', 'boas-praticas', 'extras', 'm1-a1', 'm2-a1']
  );
  assert.deepEqual(
    restored[0].lessons.map((lesson) => lesson.number),
    [1, 2, 3]
  );
  assert.deepEqual(groups, original);
});

test('busca sem correspondência e curso sem grupos retornam uma coleção vazia', () => {
  assert.deepEqual(filterLearningGroups(groups, 'não existe'), []);
  assert.deepEqual(filterLearningGroups([], ''), []);
});
