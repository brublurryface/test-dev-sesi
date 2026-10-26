import { DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  computed,
  effect,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CourseDetailStore } from '../../core/course-detail.store';
import { SITE_CONFIG } from '../../core/site.config';
import { Header } from '../../shared/header/header';
import { Footer } from '../../shared/footer/footer';
import { Benefits } from '../../shared/benefits/benefits';
import { Testimonials } from '../../shared/testimonials/testimonials';

/**
 * Exibe o curso escolhido pelo slug da rota, com conteúdo recebido do CourseDetailStore.
 * Os diálogos explicam vídeo e acesso demonstrativo; não realizam matrícula ou autenticação.
 */
@Component({
  selector: 'app-course-detail',
  imports: [DatePipe, RouterLink, Header, Footer, Benefits, Testimonials],
  providers: [CourseDetailStore],
  templateUrl: './course-detail.html',
  styleUrl: './course-detail.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CourseDetail {
  readonly site = SITE_CONFIG;
  readonly store = inject(CourseDetailStore);
  // A mesma rota pode exibir outro curso; o slug deve acompanhar essa troca de forma reativa.
  private readonly params = toSignal(inject(ActivatedRoute).paramMap);
  private readonly title = inject(Title);
  readonly course = this.store.course;
  readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');
  readonly dialogMode = signal<'video' | 'enroll'>('enroll');
  readonly faqs = computed(() => this.course()?.faqs ?? []);

  constructor() {
    effect(() => {
      const slug = this.params()?.get('slug');
      if (slug) this.store.load(slug);
    });
    effect(() =>
      this.title.setTitle(
        this.course()
          ? `${this.course()!.shortName} | ${this.site.projectName}`
          : `Curso | ${this.site.projectName}`
      )
    );
  }
  // O dialog nativo mantém o foco dentro do aviso e permite fechá-lo com Escape.
  showDialog(mode: 'video' | 'enroll'): void {
    this.dialogMode.set(mode);
    this.dialog().nativeElement.showModal();
  }
  closeDialog(): void {
    this.dialog().nativeElement.close();
  }
}
