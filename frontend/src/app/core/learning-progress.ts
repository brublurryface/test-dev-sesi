/**
 * Restaura apenas aulas ainda presentes no curso e remove identificadores repetidos.
 * Dados locais antigos ou inválidos são descartados para permitir retomar a navegação.
 */
export function restoreLearningProgress(
  raw: string | null,
  lessonIds: readonly string[]
): string[] {
  if (!raw) return [];
  try {
    const value: unknown = JSON.parse(raw);
    if (!Array.isArray(value)) return [];
    const validIds = new Set(lessonIds);
    return [
      ...new Set(value.filter((id): id is string => typeof id === 'string' && validIds.has(id))),
    ];
  } catch {
    return [];
  }
}

/** Conta a interseção dos conjuntos para manter o percentual entre 0 e 100. */
export function calculateLearningProgress(
  lessonIds: readonly string[],
  completedIds: readonly string[]
): number {
  const validIds = new Set(lessonIds);
  if (validIds.size === 0) return 0;
  const completed = new Set(completedIds);
  const count = [...validIds].filter((id) => completed.has(id)).length;
  return Math.round((count / validIds.size) * 100);
}
