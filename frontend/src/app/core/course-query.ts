/** Critérios enviados à API; Catalog converte os nomes portugueses da URL para este contrato. */
export interface CourseQuery {
  search: string;
  category: string;
  modality: string;
  status: string;
}
export const EMPTY_QUERY: CourseQuery = { search: '', category: '', modality: '', status: '' };

/** Parâmetros HTTP da listagem, incluindo os limites dos destaques, rodapé e catálogo. */
export type CourseListQuery = Partial<CourseQuery> & {
  featured?: boolean;
  page?: number;
  pageSize?: number;
};

/** Links sem página ou com formato inválido iniciam na primeira página. */
export function readCoursePage(params: { get(name: string): string | null }): number {
  const raw = params.get('pagina') ?? '1';
  const page = Number(raw);
  return /^\d+$/.test(raw) && Number.isSafeInteger(page) && page >= 1 && page <= 2147483647
    ? page
    : 1;
}

/** Os filtros e a página compartilham a URL; a primeira página dispensa um parâmetro. */
export function courseQueryParams(query: CourseQuery, page = 1): Record<string, string | null> {
  return {
    busca: query.search || null,
    categoria: query.category || null,
    modalidade: query.modality || null,
    status: query.status || null,
    pagina: page > 1 ? String(page) : null,
  };
}

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
