import { test } from 'node:test';
import assert from 'node:assert/strict';
import { filterOptionsForSelection, readCourseQuery } from './course-query.ts';
import { categoryIcon, enrollmentLabel } from './course-presentation.ts';

test('links aceitam categorias, modalidades e status novos sem uma lista fixa no frontend', () => {
  const params = new URLSearchParams({
    busca: 'Programação',
    categoria: 'Tecnologia & Inovação',
    modalidade: 'Híbrido',
    status: 'Lista de espera',
  });
  assert.deepEqual(readCourseQuery(params, 120), {
    search: 'Programação',
    category: 'Tecnologia & Inovação',
    modality: 'Híbrido',
    status: 'Lista de espera',
  });
});

test('a seleção do link não desaparece antes das opções nem quando foi removida do catálogo', () => {
  assert.deepEqual(filterOptionsForSelection([], 'Tecnologia'), ['Tecnologia']);
  const options = ['Matemática', 'Tecnologia'];
  assert.deepEqual(filterOptionsForSelection(options, 'Tecnologia'), options);
  assert.deepEqual(filterOptionsForSelection(options, 'Removida'), [...options, 'Removida']);
  assert.deepEqual(options, ['Matemática', 'Tecnologia']);
});

test('novos valores não herdam ícone de Ciências nem rótulo de inscrições encerradas', () => {
  assert.equal(categoryIcon('Tecnologia'), 'assets/icon-curso-20px.svg');
  assert.equal(categoryIcon('Ciências da Natureza'), 'assets/icon-ciencias-natureza-20px.svg');
  assert.equal(enrollmentLabel('Lista de espera'), 'Lista de espera');
  assert.equal(enrollmentLabel('Disponível'), 'Matricule-se');
  assert.equal(enrollmentLabel('Encerrado'), 'Inscrições encerradas');
});
