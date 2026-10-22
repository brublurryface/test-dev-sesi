import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import type { Course } from './course.model';

@Injectable({ providedIn: 'root' })
/** Centraliza o carregamento para que as páginas compartilhem a mesma coleção de cursos. */
export class CourseService {
  private readonly http = inject(HttpClient);
  private readonly items = signal<Course[]>([]);
  private readonly pending = signal(true);
  private readonly failure = signal(false);
  // As páginas observam o estado; a atualização dos signals fica concentrada no serviço.
  readonly courses = this.items.asReadonly();
  readonly loading = this.pending.asReadonly();
  readonly error = this.failure.asReadonly();

  constructor() {
    void this.load();
  }

  async load(): Promise<void> {
    this.pending.set(true);
    this.failure.set(false);
    try {
      // A origem desta etapa é o dataset local. A integração .NET deve preservar o contrato Course.
      // HttpClient emite uma resposta por requisição; firstValueFrom permite aguardar esse resultado.
      const courses = await firstValueFrom(this.http.get<Course[]>('/data/courses.json'));
      this.items.set(courses);
    } catch {
      this.failure.set(true);
    } finally {
      // O estado de carregamento deve terminar tanto no sucesso quanto na falha.
      this.pending.set(false);
    }
  }
}
