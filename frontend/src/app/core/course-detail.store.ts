import { HttpErrorResponse } from '@angular/common/http';
import { DestroyRef, Injectable, inject, signal } from '@angular/core';
import type { Subscription } from 'rxjs';
import type { Course } from './course.model';
import { CourseService } from './course.service';

/**
 * Carrega um curso completo para detalhes ou AVA, com uma instância por página.
 * course=null representa ausência de dados; error distingue falha da API de um curso inexistente.
 */
@Injectable()
export class CourseDetailStore {
  private readonly api = inject(CourseService);
  private request?: Subscription;
  private lastSlug = '';
  private readonly item = signal<Course | null>(null);
  private readonly pending = signal(true);
  private readonly failure = signal(false);
  readonly course = this.item.asReadonly();
  readonly loading = this.pending.asReadonly();
  readonly error = this.failure.asReadonly();
  constructor() {
    // Evita manter uma consulta ativa depois que a página sai de uso.
    inject(DestroyRef).onDestroy(() => this.request?.unsubscribe());
  }
  /** Reutiliza o último slug na nova tentativa e cancela a consulta anterior ao trocar de curso. */
  load(slug = this.lastSlug): void {
    this.lastSlug = slug;
    this.request?.unsubscribe();
    // Não exibe o conteúdo de outro curso enquanto a resposta do novo slug está pendente.
    this.item.set(null);
    this.pending.set(true);
    this.failure.set(false);
    this.request = this.api.getCourse(slug).subscribe({
      next: (course) => {
        this.item.set(course);
        this.pending.set(false);
      },
      error: (error: HttpErrorResponse) => {
        // Um endereço inexistente tem mensagem própria, diferente de uma falha de conexão.
        this.failure.set(error.status !== 404);
        this.pending.set(false);
      },
    });
  }
}
