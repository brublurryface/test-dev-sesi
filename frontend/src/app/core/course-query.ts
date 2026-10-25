/** Critérios enviados à API; Catalog converte os nomes portugueses da URL para este contrato. */
export interface CourseQuery {
  search: string;
  category: string;
  modality: string;
  status: string;
}
export const EMPTY_QUERY: CourseQuery = { search: '', category: '', modality: '', status: '' };

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
