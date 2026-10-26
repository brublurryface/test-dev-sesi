import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Header } from '../../shared/header/header';
import { Footer } from '../../shared/footer/footer';

@Component({
  selector: 'app-not-found',
  imports: [RouterLink, Header, Footer],
  template:
    '<app-header /><main id="conteudo" class="container section"><div class="state-panel"><p class="eyebrow">404</p><h1>Página não encontrada</h1><p>Continue sua jornada explorando os cursos disponíveis.</p><a class="button button-dark" routerLink="/cursos">Conhecer cursos <span aria-hidden="true">→</span></a></div></main><app-footer />',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotFound {}
