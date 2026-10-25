import { DestroyRef, Injectable, inject, signal } from '@angular/core';
import { Subscription, switchMap, timer } from 'rxjs';
import type { CourseSummary } from './course.model';
import type { CourseQuery } from './course-query';
import { CourseService } from './course.service';

type ListQuery = Partial<CourseQuery> & { featured?: boolean };

/**
 * Coordena a listagem e seus estados de tela: dados, contagem, carregamento e falha.
 * É fornecido pelo componente que consulta os dados; início, catálogo e rodapé têm estados próprios.
 * CourseService realiza o HTTP; este store controla quando consultar e o que a tela deve mostrar.
 */
@Injectable()
export class CourseListStore {
  private readonly api = inject(CourseService);
  private request?: Subscription;
  private lastQuery: ListQuery = {};
  private readonly items = signal<CourseSummary[]>([]);
  private readonly count = signal(0);
  private readonly pending = signal(true);
  private readonly failure = signal(false);
  // A página lê os signals públicos, enquanto as mudanças de estado passam pelos métodos do store.
  readonly courses = this.items.asReadonly();
  readonly total = this.count.asReadonly();
  readonly loading = this.pending.asReadonly();
  readonly error = this.failure.asReadonly();
  constructor() {
    // Descartar a página também interrompe a inscrição e a requisição HTTP em andamento.
    inject(DestroyRef).onDestroy(() => this.request?.unsubscribe());
  }
  /** Reconsulta os últimos filtros quando chamado sem argumentos, inclusive na nova tentativa. */
  load(query = this.lastQuery, delay = 0): void {
    this.lastQuery = { ...query };
    // Uma resposta atrasada não pode substituir a seleção atual.
    this.request?.unsubscribe();
    this.pending.set(true);
    this.failure.set(false);
    // O catálogo pede 250 ms ao digitar; seletores e destaques usam consulta imediata (delay=0).
    this.request = timer(delay)
      .pipe(switchMap(() => this.api.list(query)))
      .subscribe({
        next: (page) => {
          this.items.set(page.items);
          this.count.set(page.total);
          this.pending.set(false);
        },
        error: () => {
          // Uma falha limpa os resultados e habilita a mensagem de erro; não usa dados fictícios locais.
          this.items.set([]);
          this.count.set(0);
          this.failure.set(true);
          this.pending.set(false);
        },
      });
  }
}
