import type { LessonSummary } from './course.model';

/** Grupos da navegação local; não altera os módulos ou as aulas recebidos da API. */
export interface LearningGroup {
  id: string;
  title: string;
  lessons: readonly LessonSummary[];
}

/**
 * Normaliza a busca local de aulas da AVA. A busca de cursos é feita pelo backend.
 * NFD separa os sinais de acentuação para comparar palavras com e sem acentos.
 */
export function normalizeText(value: string): string {
  return value
    .trim()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLocaleLowerCase('pt-BR');
}

/**
 * Numera antes de filtrar para preservar a posição de cada aula na estrutura completa.
 * Buscar pelo título do grupo mantém todas as suas aulas; limpar a busca restaura a lista.
 */
export function filterLearningGroups(groups: readonly LearningGroup[], search: string) {
  const term = normalizeText(search);
  return groups
    .map((group, index) => {
      const lessons = group.lessons.map((lesson, index) => ({ ...lesson, number: index + 1 }));
      return {
        ...group,
        number: index + 1,
        lessons: normalizeText(group.title).includes(term)
          ? lessons
          : lessons.filter((lesson) => normalizeText(lesson.title).includes(term)),
      };
    })
    .filter((group) => group.lessons.length);
}
