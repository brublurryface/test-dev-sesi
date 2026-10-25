import { ChangeDetectionStrategy, Component, HostListener, input, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { SITE_CONFIG } from '../../core/site.config';

const SCROLL_THRESHOLD_PX = 24;

/** Controla o menu móvel e a mudança visual do header durante a rolagem. */
@Component({
  selector: 'app-header',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './header.html',
  styleUrl: './header.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Header {
  readonly transparent = input(false);
  // Detalhes informam o curso atual; início e catálogo usam a referência da demonstração.
  readonly courseSlug = input<string>(SITE_CONFIG.referenceCourseSlug);
  // Na página inicial, o modo transparente dá lugar ao header claro após sair do topo.
  readonly scrolled = signal(window.scrollY > SCROLL_THRESHOLD_PX);
  readonly menuOpen = signal(false);
  @HostListener('window:scroll') onScroll(): void {
    this.scrolled.set(window.scrollY > SCROLL_THRESHOLD_PX);
  }
  closeMenu(): void {
    this.menuOpen.set(false);
  }
}
