/** Critérios enviados à API; Catalog converte os nomes portugueses da URL para este contrato. */
export interface CourseQuery {
  search: string;
  category: string;
  modality: string;
  status: string;
}
export const EMPTY_QUERY: CourseQuery = { search: '', category: '', modality: '', status: '' };

/** Lê links sem uma lista fixa de opções; a API valida os valores contra o banco. */
export function readCourseQuery(
  params: { get(name: string): string | null },
  maxSearchLength: number
): CourseQuery {
  return {
    search: (params.get('busca') ?? '').slice(0, maxSearchLength),
    category: params.get('categoria') ?? '',
    modality: params.get('modalidade') ?? '',
    status: params.get('status') ?? '',
  };
}

/**
 * Mantém visível a seleção do link enquanto as opções chegam ou se uma opção foi removida.
 * Um valor inexistente continua sujeito à validação da API; não se transforma em filtro vazio.
 */
export function filterOptionsForSelection(options: string[], selected: string): string[] {
  return selected && !options.includes(selected) ? [...options, selected] : options;
}
