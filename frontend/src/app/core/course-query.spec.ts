import { test } from 'node:test';
import assert from 'node:assert/strict';
import { filterCourses, EMPTY_QUERY, type SearchableCourse } from './course-query.ts';

const courses = [
  {
    id: 1,
    name: 'Matemática para a Precisão',
    category: 'Matemática',
    modality: 'Online',
    status: 'Disponível',
  },
  {
    id: 2,
    name: 'Geometria e Modelagem',
    category: 'Matemática',
    modality: 'Presencial',
    status: 'Em breve',
  },
  {
    id: 3,
    name: 'Energia e produção',
    category: 'Sustentabilidade',
    modality: 'Online',
    status: 'Encerrado',
  },
] satisfies (SearchableCourse & { id: number })[];

test('busca ignora acentos, espaços externos e diferença entre maiúsculas e minúsculas', () => {
  assert.deepEqual(
    filterCourses(courses, { ...EMPTY_QUERY, search: '  MATEMATICA  ' }).map((c) => c.id),
    [1]
  );
});
test('aplica simultaneamente nome, categoria, modalidade e status', () => {
  assert.deepEqual(
    filterCourses(courses, {
      search: 'geometria',
      category: 'Matemática',
      modality: 'Presencial',
      status: 'Em breve',
    }).map((c) => c.id),
    [2]
  );
  assert.equal(
    filterCourses(courses, {
      search: 'geometria',
      category: 'Matemática',
      modality: 'Online',
      status: 'Em breve',
    }).length,
    0
  );
});
test('limpar filtros devolve todos os cursos sem alterar a coleção original', () => {
  const result = filterCourses(courses, EMPTY_QUERY);
  assert.equal(result.length, 3);
  assert.notEqual(result, courses);
  assert.equal(courses.length, 3);
});
