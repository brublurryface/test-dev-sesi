import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-benefits',
  template: `
    <div class="benefits" [class.horizontal]="horizontal()">
      @for (item of items; track item.title; let index = $index) {
        @if (horizontal() || index < 3) {
          <article class="benefit">
            <img
              [src]="
                'assets/' +
                (index + 1).toString().padStart(2, '0') +
                '-' +
                item.asset +
                '-' +
                (horizontal() ? 'bege' : 'red') +
                '.svg'
              "
              width="56"
              height="56"
              alt=""
              loading="lazy"
            />
            <div>
              <h3>{{ item.title }}</h3>
              <p>{{ item.copy }}</p>
            </div>
          </article>
        }
      }
    </div>
  `,
  styles: `
    .benefits {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 35px;
    }
    .benefit {
      display: flex;
      flex-direction: column;
      gap: 24px;
      align-items: flex-start;
    }
    .benefit img {
      width: 68px;
      height: 68px;
      flex: none;
    }
    .benefit h3 {
      font-size: clamp(20px, 1.25vw, 24px);
      line-height: 1.25;
      margin: 0 0 10px;
    }
    .benefit p {
      font-size: clamp(17px, 1.1vw, 21px);
      line-height: 1.5;
      color: var(--muted);
      margin: 0;
    }
    .horizontal {
      grid-template-columns: repeat(4, 1fr);
      gap: 35px;
    }
    .horizontal .benefit {
      flex-direction: column;
      gap: 26px;
    }
    .horizontal .benefit img {
      width: 68px;
      height: 68px;
    }
    .horizontal .benefit p {
      font-size: clamp(16px, 1.05vw, 20px);
    }
    @media (max-width: 1000px) {
      .benefits:not(.horizontal) {
        grid-template-columns: 1fr;
      }
      .benefits:not(.horizontal) .benefit {
        flex-direction: row;
      }
      .horizontal {
        grid-template-columns: repeat(2, 1fr);
        gap: 34px;
      }
    }
    @media (max-width: 520px) {
      .benefit {
        gap: 18px;
      }
      .benefit img {
        width: 50px;
        height: 50px;
      }
      .horizontal {
        gap: 28px 22px;
      }
      .horizontal .benefit {
        gap: 16px;
      }
      .horizontal .benefit h3 {
        font-size: 18px;
      }
      .horizontal .benefit p {
        font-size: 15px;
      }
      .horizontal .benefit img {
        width: 50px;
        height: 50px;
      }
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Benefits {
  readonly horizontal = input(false);
  readonly items = [
    {
      title: 'Certificação SESI',
      asset: 'certificacao',
      copy: 'Cursos com a melhor certificação do mercado industrial reconhecidos pelo SESI.',
    },
    {
      title: 'Professores especialistas',
      asset: 'professores',
      copy: 'Contamos com professores reconhecidos por sua excelência no mercado.',
    },
    {
      title: 'Suporte contínuo',
      asset: 'suporte',
      copy: 'Você não está sozinho! Acompanhamento online durante todo o curso.',
    },
    {
      title: 'Flexibilidade',
      asset: 'flexibilidade',
      copy: 'Turmas presenciais e online com horários que se adaptam à sua rotina.',
    },
  ];
}
