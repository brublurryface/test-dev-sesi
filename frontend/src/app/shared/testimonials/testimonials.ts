import { ChangeDetectionStrategy, Component, ElementRef, input, viewChild } from '@angular/core';
import { scrollCarouselBy } from '../carousel-scroll';

@Component({
  selector: 'app-testimonials',
  templateUrl: './testimonials.html',
  styleUrl: './testimonials.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Testimonials {
  readonly light = input(false);
  readonly track = viewChild.required<ElementRef<HTMLElement>>('track');
  readonly testimonials = [
    {
      name: 'Lucas Pereira',
      role: 'Analista na Empresa X',
      quote:
        'A experiência do curso foi incrível! As aulas foram muito bem estruturadas e o cronograma foi cuidadosamente planejado, facilitando muito o aprendizado.',
    },
    {
      name: 'Mariana Costa',
      role: 'Gestora na Empresa X',
      quote:
        'Concluir o curso foi uma jornada enriquecedora. As aulas eram dinâmicas e o horário foi organizado para maximizar nosso tempo de estudo.',
    },
    {
      name: 'Thiago Almeida',
      role: 'Técnico na Empresa X',
      quote:
        'O curso superou minhas expectativas! As aulas eram bem elaboradas e o planejamento do horário ajudou a manter o foco e a produtividade.',
    },
    {
      name: 'Fernanda Silva',
      role: 'Analista na Empresa X',
      quote:
        'Finalizar o curso foi uma experiência motivadora. As aulas foram muito bem preparadas e o cronograma foi ideal para o aprendizado.',
    },
    {
      name: 'Gabriel Santos',
      role: 'Analista na Empresa X',
      quote:
        'A conclusão do curso foi uma etapa muito gratificante. As aulas eram bem organizadas e o horário foi pensado para facilitar nosso aprendizado.',
    },
  ];
  move(direction: number): void {
    const track = this.track().nativeElement;
    const card = track.querySelector('article');
    scrollCarouselBy(track, direction * ((card?.getBoundingClientRect().width ?? 320) + 24));
  }
}
