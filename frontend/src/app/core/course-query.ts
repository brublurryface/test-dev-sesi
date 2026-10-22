import type { Course } from './course.model.ts';

export interface CourseQuery {
  search: string;
  category: string;
  modality: string;
  status: string;
}
export const EMPTY_QUERY: CourseQuery = { search: '', category: '', modality: '', status: '' };

/** Projeção usada na consulta; mantém a função independente de detalhes de apresentação. */
export type SearchableCourse = Pick<Course, 'name' | 'category' | 'modality' | 'status'>;

/** NFD separa os sinais de acentuação e permite comparar, por exemplo, "matematica" e "Matemática". */
export function normalizeText(value: string): string {
  return value
    .trim()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLocaleLowerCase('pt-BR');
}

/**
 * Combina os critérios com AND e mantém a coleção original intacta.
 * O tipo genérico preserva todos os campos dos cursos retornados ao componente.
 */
export function filterCourses<T extends SearchableCourse>(
  courses: readonly T[],
  query: CourseQuery
): T[] {
  const term = normalizeText(query.search);
  // Um critério vazio não restringe o resultado; limpar a consulta devolve todos os cursos.
  return courses.filter(
    (course) =>
      (!term || normalizeText(course.name).includes(term)) &&
      (!query.category || course.category === query.category) &&
      (!query.modality || course.modality === query.modality) &&
      (!query.status || course.status === query.status)
  );
}
