import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import type { CourseSummary } from '../../core/course.model';

@Component({
  selector: 'app-course-card',
  imports: [RouterLink],
  template: `
    <a
      class="course-card"
      [routerLink]="['/cursos', course().slug]"
      [style.background-image]="'url(' + course().image + ')'"
    >
      <span
        class="category-tag"
        [class.nature]="course().category === 'Ciências da Natureza'"
        [class.sustainable]="course().category === 'Sustentabilidade'"
      >
        <img
          [src]="
            course().category === 'Matemática'
              ? 'assets/icon-matematica-20px.svg'
              : course().category === 'Sustentabilidade'
                ? 'assets/icon-sustentabilidade-20px.svg'
                : 'assets/icon-ciencias-natureza-20px.svg'
          "
          width="20"
          height="20"
          alt=""
        />{{ course().category }}
      </span>
      <div class="card-copy">
        @if (showMeta()) {
          <span class="card-status"
            >{{ course().modality }} · {{ course().hours }}h · {{ course().status }}</span
          >
        }
        <h3>{{ course().name }}</h3>
        <p>{{ course().description }}</p>
      </div>
    </a>
  `,
  styleUrl: './course-card.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CourseCard {
  readonly course = input.required<CourseSummary>();
  readonly showMeta = input(false);
}
