import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CATEGORIES, MODALITIES, STATUSES } from '../../core/course.model';
import { EMPTY_QUERY, filterCourses, type CourseQuery } from '../../core/course-query';
import { CourseService } from '../../core/course.service';
import { Header } from '../../shared/header/header';
import { Footer } from '../../shared/footer/footer';
import { CourseCard } from '../../shared/course-card/course-card';

@Component({
  selector: 'app-catalog',
  imports: [FormsModule, RouterLink, Header, Footer, CourseCard],
  templateUrl: './catalog.html',
  styleUrl: './catalog.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Catalog {
  readonly store = inject(CourseService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  readonly categories = CATEGORIES;
  readonly modalities = MODALITIES;
  readonly statuses = STATUSES;
  readonly query = signal<CourseQuery>(this.readQuery());
  // A lista é derivada do estado atual; não há uma segunda coleção de resultados para sincronizar.
  readonly filtered = computed(() => filterCourses(this.store.courses(), this.query()));
  readonly hasFilters = computed(() => Object.values(this.query()).some((value) => value.trim()));

  constructor() {
    // A URL permite recuperar a seleção e responder à navegação do histórico do navegador.
    this.route.queryParamMap.subscribe(() => this.query.set(this.readQuery()));
  }
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
  clear(): void {
    this.query.set({ ...EMPTY_QUERY });
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {},
      replaceUrl: true,
      scroll: 'manual',
    });
  }
  private readQuery(): CourseQuery {
    // Parâmetros externos só ativam opções presentes nos conjuntos de valores do catálogo.
    const params = this.route.snapshot.queryParamMap;
    return {
      search: params.get('busca') ?? '',
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
