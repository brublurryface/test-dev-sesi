import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  computed,
  inject,
  viewChild,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { CourseService } from '../../core/course.service';
import { Header } from '../../shared/header/header';
import { Footer } from '../../shared/footer/footer';
import { Benefits } from '../../shared/benefits/benefits';
import { Testimonials } from '../../shared/testimonials/testimonials';
import { CourseCard } from '../../shared/course-card/course-card';
import { scrollCarouselBy } from '../../shared/carousel-scroll';

@Component({
  selector: 'app-home',
  imports: [RouterLink, Header, Footer, Benefits, Testimonials, CourseCard],
  templateUrl: './home.html',
  styleUrl: './home.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Home {
  readonly store = inject(CourseService);
  readonly featured = computed(() => this.store.courses().filter((course) => course.featured));
  readonly courseTrack = viewChild<ElementRef<HTMLElement>>('courseTrack');
  readonly videoInfo = viewChild.required<ElementRef<HTMLDialogElement>>('videoInfo');
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
