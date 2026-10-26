import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { CourseListStore } from '../../core/course-list.store';
import { SITE_CONFIG } from '../../core/site.config';

@Component({
  selector: 'app-footer',
  imports: [RouterLink],
  providers: [CourseListStore],
  templateUrl: './footer.html',
  styleUrl: './footer.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Footer {
  readonly site = SITE_CONFIG;
  // O rodapé consulta seu catálogo sem herdar os filtros aplicados na página principal.
  readonly store = inject(CourseListStore);
  readonly infoTitle = signal('');
  readonly info = viewChild.required<ElementRef<HTMLDialogElement>>('info');
  constructor() {
    this.store.load();
  }
  openInfo(title: string): void {
    this.infoTitle.set(title);
    this.info().nativeElement.showModal();
  }
  closeInfo(): void {
    this.info().nativeElement.close();
  }
}
