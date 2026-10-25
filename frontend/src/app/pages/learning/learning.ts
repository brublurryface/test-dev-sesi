import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, RouterLink } from '@angular/router';
import type { Lesson } from '../../core/course.model';
import { normalizeText } from '../../core/course-query';
import {
  calculateLearningProgress,
  findInitialLesson,
  restoreLearningProgress,
} from '../../core/learning-progress';
import { CourseDetailStore } from '../../core/course-detail.store';
import { SITE_CONFIG } from '../../core/site.config';

/**
 * AVA demonstrativa: recebe o conteúdo da API e controla seleção e busca de aulas na tela.
 * A conclusão é reversível e salva somente no navegador, sem matrícula ou progresso no servidor.
 */
@Component({
  selector: 'app-learning',
  imports: [RouterLink],
  providers: [CourseDetailStore],
  templateUrl: './learning.html',
  styleUrl: './learning.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Learning {
  readonly store = inject(CourseDetailStore);
  // Os parâmetros reativos permitem trocar de curso sem depender de uma recarga da página.
  private readonly params = toSignal(inject(ActivatedRoute).paramMap);
  private readonly title = inject(Title);
  readonly course = this.store.course;
  readonly search = signal('');
  readonly sidebarOpen = signal(false);
  readonly selectedId = signal('');
  readonly completed = signal<string[]>([]);
  readonly storageAvailable = signal(true);
  readonly introLessons = computed(() => this.course()?.introduction ?? []);
  // Agrupa o conteúdo recebido usando os rótulos neutros Módulo 1, 2... definidos para esta AVA.
  readonly groups = computed(() => [
    { id: 'introducao', title: 'Introdução', lessons: this.introLessons() },
    ...(this.course()?.modules ?? []).map((module, index) => ({
      id: `modulo-${index + 1}`,
      title: `Módulo ${index + 1}`,
      lessons: module.lessons,
    })),
  ]);
  readonly allLessons = computed(() => this.groups().flatMap((group) => group.lessons));
  // A aula retomada também mantém seu grupo visível quando o progresso já chegou aos módulos.
  readonly selectedGroupId = computed(
    () =>
      this.groups().find((group) => group.lessons.some((lesson) => lesson.id === this.selectedId()))
        ?.id
  );
  readonly visibleGroups = computed(() => {
    const term = normalizeText(this.search());
    // Encontrar o título de um módulo mantém suas aulas; buscar uma aula restringe a lista interna.
    return this.groups()
      .map((group, index) => ({
        ...group,
        number: index + 1,
        lessons: normalizeText(group.title).includes(term)
          ? group.lessons
          : group.lessons.filter((lesson) => normalizeText(lesson.title).includes(term)),
      }))
      .filter((group) => group.lessons.length);
  });
  // Enquanto não há curso/aulas, o objeto vazio mantém o template seguro; não cria uma aula no banco.
  readonly lesson = computed(
    () =>
      this.allLessons().find((lesson) => lesson.id === this.selectedId()) ??
      this.allLessons()[0] ??
      ({ id: '', title: 'Aula', description: '', content: [] } satisfies Lesson)
  );
  readonly progress = computed(() =>
    calculateLearningProgress(
      this.allLessons().map((lesson) => lesson.id),
      this.completed()
    )
  );
  readonly isComplete = computed(() => this.completed().includes(this.lesson().id));

  constructor() {
    // Mudança no slug carrega o curso; os demais estados dependem da resposta desse pedido.
    effect(() => {
      const slug = this.params()?.get('slug');
      if (slug) this.store.load(slug);
    });
    // Depois da resposta, restaura apenas os IDs de aulas ainda presentes neste curso.
    effect(() => {
      const slug = this.course()?.slug;
      if (!slug) return;
      const lessonIds = this.allLessons().map((lesson) => lesson.id);
      let completed: string[] = [];
      this.search.set('');
      try {
        // A chave por curso evita misturar o progresso de ofertas diferentes.
        const raw = localStorage.getItem(`sesi-progress:${slug}`);
        completed = restoreLearningProgress(raw, lessonIds);
        this.storageAvailable.set(true);
      } catch {
        this.storageAvailable.set(false);
      }
      // Usa os dados restaurados, sem depender do signal completed: marcar uma aula não troca a seleção.
      this.completed.set(completed);
      this.selectedId.set(findInitialLesson(lessonIds, completed));
    });
    effect(() => this.title.setTitle(`${this.lesson().title} | AVA ${SITE_CONFIG.projectName}`));
  }
  setSearch(event: Event): void {
    this.search.set((event.target as HTMLInputElement).value);
  }
  /** Seleciona a aula, fecha o painel móvel e leva a leitura ao início do conteúdo escolhido. */
  selectLesson(id: string): void {
    this.selectedId.set(id);
    this.sidebarOpen.set(false);
    window.scrollTo({ top: 0, behavior: 'instant' });
  }
  /** Interação adicional ao protótipo para demonstrar marcação e atualização do progresso local. */
  toggleComplete(): void {
    const id = this.lesson().id;
    this.completed.update((ids) =>
      ids.includes(id) ? ids.filter((item) => item !== id) : [...ids, id]
    );
    this.persist();
  }
  /** Segue a ordem completa do curso; a busca da barra lateral não altera a sequência das aulas. */
  next(): void {
    const current = this.allLessons().findIndex((lesson) => lesson.id === this.lesson().id);
    const next = this.allLessons()[current + 1];
    if (next) this.selectLesson(next.id);
  }
  readonly hasNext = computed(
    () =>
      this.allLessons().findIndex((lesson) => lesson.id === this.lesson().id) <
      this.allLessons().length - 1
  );
  resetProgress(): void {
    this.completed.set([]);
    this.persist();
  }
  private persist(): void {
    // A indisponibilidade do armazenamento local não impede a navegação durante a sessão.
    try {
      localStorage.setItem(
        `sesi-progress:${this.course()!.slug}`,
        JSON.stringify(this.completed())
      );
    } catch {
      this.storageAvailable.set(false);
    }
  }
}
