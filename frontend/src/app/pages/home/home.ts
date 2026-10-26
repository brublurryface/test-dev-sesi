import { ChangeDetectionStrategy, Component, ElementRef, inject, viewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CourseListStore } from '../../core/course-list.store';
import { SITE_CONFIG } from '../../core/site.config';
import { Header } from '../../shared/header/header';
import { Footer } from '../../shared/footer/footer';
import { Benefits } from '../../shared/benefits/benefits';
import { Testimonials } from '../../shared/testimonials/testimonials';
import { CourseCard } from '../../shared/course-card/course-card';
import { scrollCarouselBy } from '../../shared/carousel-scroll';

/** Página inicial: consulta destaques na API e coordena carrossel e aviso do vídeo ilustrativo. */
@Component({
  selector: 'app-home',
  imports: [RouterLink, Header, Footer, Benefits, Testimonials, CourseCard],
  providers: [CourseListStore],
  templateUrl: './home.html',
  styleUrl: './home.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Home {
  readonly site = SITE_CONFIG;
  readonly store = inject(CourseListStore);
  readonly featured = this.store.courses;
  readonly courseTrack = viewChild<ElementRef<HTMLElement>>('courseTrack');
  readonly videoInfo = viewChild.required<ElementRef<HTMLDialogElement>>('videoInfo');
  constructor() {
    // A seleção de destaques é feita pelo servidor, sem manter uma segunda lista de cursos aqui.
    this.store.load({ featured: true });
  }
  /** Avança ou retorna a largura de um cartão mais seu espaçamento no carrossel. */
  move(direction: number): void {
    const track = this.courseTrack()?.nativeElement;
    if (!track) return;
    const card = track.querySelector('app-course-card');
    scrollCarouselBy(track, direction * ((card?.getBoundingClientRect().width ?? 320) + 24));
  }
  showVideoInfo(): void {
    this.videoInfo().nativeElement.showModal();
  }
  closeVideoInfo(): void {
    this.videoInfo().nativeElement.close();
  }
}
