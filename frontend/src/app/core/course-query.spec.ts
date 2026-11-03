import { test } from 'node:test';
import assert from 'node:assert/strict';
import {
  courseQueryParams,
  filterOptionsForSelection,
  readCoursePage,
  readCourseQuery,
} from './course-query.ts';
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

test('restaura a página de um link e usa a primeira para valores ausentes ou malformados', () => {
  assert.equal(readCoursePage(new URLSearchParams('pagina=3')), 3);
  for (const value of ['', '0', '-1', '2.5', 'abc', 'Infinity', '2147483648']) {
    assert.equal(readCoursePage(new URLSearchParams({ pagina: value })), 1);
  }
  assert.equal(readCoursePage(new URLSearchParams()), 1);
});

test('um link paginado preserva busca e filtros; voltar à primeira página remove apenas pagina', () => {
  const query = {
    search: 'Matemática',
    category: 'Tecnologia',
    modality: 'Híbrido',
    status: 'Lista de espera',
  };
  const params = courseQueryParams(query, 3);
  assert.deepEqual(params, {
    busca: 'Matemática',
    categoria: 'Tecnologia',
    modalidade: 'Híbrido',
    status: 'Lista de espera',
    pagina: '3',
  });
  assert.deepEqual(courseQueryParams(query), { ...params, pagina: null });
  const link = new URLSearchParams(
    Object.entries(params).filter((entry): entry is [string, string] => entry[1] !== null)
  );
  assert.deepEqual(readCourseQuery(link, 120), query);
  assert.equal(readCoursePage(link), 3);
});
