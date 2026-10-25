import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import type { Course, CoursePage } from './course.model';
import type { CourseQuery } from './course-query';

/**
 * Cliente HTTP do catálogo. Envia os filtros à API e tipa as respostas conforme course.model.ts.
 * O estado de carregamento, os erros e os dados exibidos pertencem aos stores de cada página.
 */
@Injectable({ providedIn: 'root' })
export class CourseService {
  private readonly http = inject(HttpClient);

  /** Consulta o resumo dos cursos; a busca e os filtros são aplicados pelo backend. */
  list(query: Partial<CourseQuery> & { featured?: boolean } = {}): Observable<CoursePage> {
    let params = new HttpParams();
    // HttpParams codifica os valores da URL; featured=false também é um filtro válido.
    for (const [name, value] of Object.entries(query)) {
      if (value !== undefined && value !== '') params = params.set(name, value);
    }
    // O caminho relativo funciona com o proxy local e com a publicação de origem única.
    return this.http.get<CoursePage>('/api/courses', { params });
  }

  /** Obtém módulos, aulas e demais textos usados pelos detalhes e pela AVA. */
  getCourse(slug: string): Observable<Course> {
    return this.http.get<Course>(`/api/courses/${encodeURIComponent(slug)}`);
  }
}
