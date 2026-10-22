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
import { CourseService } from '../../core/course.service';
import { REFERENCE_COURSE_CONTENT } from '../../core/reference-course-content';
import { Header } from '../../shared/header/header';
import { Footer } from '../../shared/footer/footer';
import { Benefits } from '../../shared/benefits/benefits';
import { Testimonials } from '../../shared/testimonials/testimonials';

@Component({
  selector: 'app-course-detail',
  imports: [DatePipe, RouterLink, Header, Footer, Benefits, Testimonials],
  templateUrl: './course-detail.html',
  styleUrl: './course-detail.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CourseDetail {
  readonly store = inject(CourseService);
  // A mesma rota pode exibir outro curso; o slug deve acompanhar essa troca de forma reativa.
  private readonly params = toSignal(inject(ActivatedRoute).paramMap);
  private readonly title = inject(Title);
  readonly course = computed(() =>
    this.store.courses().find((course) => course.slug === this.params()?.get('slug'))
  );
  readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');
  readonly dialogMode = signal<'video' | 'enroll'>('enroll');
  readonly isReferenceCourse = computed(
    () => this.course()?.slug === REFERENCE_COURSE_CONTENT.slug
  );
  // Apenas o curso de referência recebeu conteúdo de FAQ nos materiais fornecidos.
  readonly faqs = computed(() => (this.isReferenceCourse() ? REFERENCE_COURSE_CONTENT.faqs : []));

  constructor() {
    effect(() =>
      this.title.setTitle(
        this.course()
          ? `${this.course()!.shortName} | TEST DEV SESI Indústria`
          : 'Curso | TEST DEV SESI Indústria'
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
