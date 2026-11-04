import { DestroyRef, Injectable, inject, signal } from '@angular/core';
import { CourseService } from './course.service';
import { LessonContentLoader, type LessonContentState } from './lesson-content.loader';

/** Estado por AVA: conteúdo, erros e cache não integram o índice nem o progresso. */
@Injectable()
export class LessonContentStore {
  private readonly api = inject(CourseService);
  private readonly loader = new LessonContentLoader((slug, id) => this.api.getLesson(slug, id));
  private readonly current = signal<LessonContentState>({
    selection: null,
    lesson: null,
    loading: false,
    error: null,
  });
  readonly state = this.current.asReadonly();

  constructor() {
    const subscription = this.loader.state$.subscribe((state) => this.current.set(state));
    inject(DestroyRef).onDestroy(() => {
      subscription.unsubscribe();
      this.loader.destroy();
    });
  }
  load(courseSlug: string, lessonId: string): void {
    this.loader.select({ courseSlug, lessonId });
  }
  retry(): void {
    this.loader.retry();
  }
  reset(): void {
    this.loader.reset();
  }
}
