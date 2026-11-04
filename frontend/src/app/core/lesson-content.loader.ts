import {
  BehaviorSubject,
  Subject,
  Observable,
  catchError,
  defer,
  map,
  of,
  shareReplay,
  startWith,
  switchMap,
  takeUntil,
  tap,
} from 'rxjs';
import type { Lesson } from './course.model';

export interface LessonSelection {
  courseSlug: string;
  lessonId: string;
}
export interface LessonContentState {
  selection: LessonSelection | null;
  lesson: Lesson | null;
  loading: boolean;
  error: 'not-found' | 'unavailable' | null;
}
export interface LessonCacheLimits {
  maxEntries: number;
  maxBytes: number;
}

/** Limites técnicos do cache em memória, relativos ao JSON armazenado. */
const DEFAULT_LIMITS: LessonCacheLimits = { maxEntries: 10, maxBytes: 2 * 1024 * 1024 };

/**
 * Seleção concorrente e cache independentes do Angular, exercitados com RxJS real nos testes.
 * O índice e o progresso permanecem fora deste carregador.
 */
export class LessonContentLoader {
  private readonly selections = new BehaviorSubject<LessonSelection | null>(null);
  private readonly destroyed = new Subject<void>();
  private readonly cache = new Map<string, { lesson: Lesson; bytes: number }>();
  private bytes = 0;
  private readonly fetchLesson: (courseSlug: string, lessonId: string) => Observable<Lesson>;
  private readonly limits: LessonCacheLimits;
  readonly state$: Observable<LessonContentState>;

  constructor(
    fetchLesson: (courseSlug: string, lessonId: string) => Observable<Lesson>,
    limits: LessonCacheLimits = DEFAULT_LIMITS
  ) {
    if (
      !Number.isInteger(limits.maxEntries) ||
      limits.maxEntries < 1 ||
      !Number.isInteger(limits.maxBytes) ||
      limits.maxBytes < 1
    ) {
      throw new Error('Os limites do cache devem ser inteiros positivos.');
    }
    this.fetchLesson = fetchLesson;
    this.limits = { ...limits };
    this.state$ = this.selections.pipe(
      // A troca cancela a inscrição anterior, inclusive ao voltar para uma aula em cache.
      switchMap((selection): Observable<LessonContentState> => {
        if (!selection) return of({ selection: null, lesson: null, loading: false, error: null });
        const key = JSON.stringify([selection.courseSlug, selection.lessonId]);
        const hit = this.cache.get(key);
        if (hit) {
          this.cache.delete(key);
          this.cache.set(key, hit);
          return of({ selection, lesson: hit.lesson, loading: false, error: null });
        }
        return defer(() => this.fetchLesson(selection.courseSlug, selection.lessonId)).pipe(
          map((lesson) => {
            if (lesson.id !== selection.lessonId)
              throw new Error('A resposta não corresponde à aula.');
            return lesson;
          }),
          tap((lesson) => this.remember(key, lesson)),
          map((lesson): LessonContentState => ({ selection, lesson, loading: false, error: null })),
          startWith({ selection, lesson: null, loading: true, error: null } as LessonContentState),
          // O erro pertence à seleção, preservando o fluxo para a próxima aula ou tentativa.
          catchError((error: unknown) =>
            of<LessonContentState>({
              selection,
              lesson: null,
              loading: false,
              error:
                typeof error === 'object' &&
                error !== null &&
                'status' in error &&
                error.status === 404
                  ? 'not-found'
                  : 'unavailable',
            })
          )
        );
      }),
      takeUntil(this.destroyed),
      shareReplay({ bufferSize: 1, refCount: true })
    );
  }
  select(selection: LessonSelection): void {
    this.selections.next(selection);
  }
  retry(): void {
    if (this.selections.value) this.selections.next(this.selections.value);
  }
  reset(): void {
    this.selections.next(null);
    this.cache.clear();
    this.bytes = 0;
  }
  destroy(): void {
    this.destroyed.next();
    this.destroyed.complete();
    this.selections.complete();
    this.cache.clear();
    this.bytes = 0;
  }
  private remember(key: string, lesson: Lesson): void {
    const bytes = new TextEncoder().encode(JSON.stringify(lesson)).byteLength;
    // Uma aula maior pode ser exibida, mas não ocupa nem expulsa entradas do cache.
    if (bytes > this.limits.maxBytes) return;
    const previous = this.cache.get(key);
    if (previous) this.bytes -= previous.bytes;
    this.cache.delete(key);
    this.cache.set(key, { lesson, bytes });
    this.bytes += bytes;
    while (this.cache.size > this.limits.maxEntries || this.bytes > this.limits.maxBytes) {
      const oldest = this.cache.keys().next().value!;
      this.bytes -= this.cache.get(oldest)!.bytes;
      this.cache.delete(oldest);
    }
  }
}
