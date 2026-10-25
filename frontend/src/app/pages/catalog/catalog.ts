import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CATEGORIES, MODALITIES, STATUSES } from '../../core/course.model';
import { EMPTY_QUERY, type CourseQuery } from '../../core/course-query';
import { CourseListStore } from '../../core/course-list.store';
import { SITE_CONFIG } from '../../core/site.config';
import { Header } from '../../shared/header/header';
import { Footer } from '../../shared/footer/footer';
import { CourseCard } from '../../shared/course-card/course-card';

/**
 * Página de consulta: sincroniza controles e URL e solicita os resultados ao CourseListStore.
 * A URL pública usa busca/categoria/modalidade; o contrato HTTP usa search/category/modality.
 */
@Component({
  selector: 'app-catalog',
  imports: [FormsModule, RouterLink, Header, Footer, CourseCard],
  providers: [CourseListStore],
  templateUrl: './catalog.html',
  styleUrl: './catalog.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Catalog {
  readonly site = SITE_CONFIG;
  readonly store = inject(CourseListStore);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  readonly categories = CATEGORIES;
  readonly modalities = MODALITIES;
  readonly statuses = STATUSES;
  readonly query = signal<CourseQuery>(this.readQuery());
  // Mantém o nome usado no template; a coleção já chega filtrada e contada pelo servidor.
  readonly filtered = this.store.courses;
  readonly hasFilters = computed(() => Object.values(this.query()).some((value) => value.trim()));

  constructor() {
    // A URL permite recuperar a seleção e responder à navegação do histórico do navegador.
    this.route.queryParamMap.pipe(takeUntilDestroyed()).subscribe(() => {
      const query = this.readQuery();
      if (
        Object.keys(query).some(
          (key) => query[key as keyof CourseQuery] !== this.query()[key as keyof CourseQuery]
        )
      ) {
        this.query.set(query);
      }
    });
    let previousSearch = this.query().search;
    effect(() => {
      const query = this.query();
      // A digitação aguarda uma pausa curta; os seletores respondem imediatamente.
      this.store.load(query, query.search !== previousSearch ? 250 : 0);
      previousSearch = query.search;
    });
  }
  /** Atualiza uma seleção e sua URL; o effect do construtor dispara a consulta correspondente. */
  update(key: keyof CourseQuery, value: string): void {
    this.query.update((query) => ({ ...query, [key]: value }));
    // replaceUrl evita adicionar uma entrada no histórico a cada caractere digitado.
    // Os filtros atualizam a mesma tela; a rolagem deve permanecer no ponto atual.
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: this.toParams(this.query()),
      replaceUrl: true,
      scroll: 'manual',
    });
  }
  /** Remove todos os critérios, preservando a posição de leitura no catálogo. */
  clear(): void {
    this.query.set({ ...EMPTY_QUERY });
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {},
      replaceUrl: true,
      scroll: 'manual',
    });
  }
  /** Reconstrói os controles ao abrir um link ou navegar pelo histórico. */
  private readQuery(): CourseQuery {
    // Parâmetros externos só ativam opções presentes nos conjuntos de valores do catálogo.
    const params = this.route.snapshot.queryParamMap;
    return {
      search: (params.get('busca') ?? '').slice(0, this.site.courseSearchMaxLength),
      category: this.categories.some((value) => value === params.get('categoria'))
        ? params.get('categoria')!
        : '',
      modality: this.modalities.some((value) => value === params.get('modalidade'))
        ? params.get('modalidade')!
        : '',
      status: this.statuses.some((value) => value === params.get('status'))
        ? params.get('status')!
        : '',
    };
  }
  private toParams(query: CourseQuery): Record<string, string | null> {
    // Valores nulos removem o parâmetro e mantêm os endereços de consulta enxutos.
    return {
      busca: query.search || null,
      categoria: query.category || null,
      modalidade: query.modality || null,
      status: query.status || null,
    };
  }
}
