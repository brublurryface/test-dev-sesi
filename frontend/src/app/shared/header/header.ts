import { ChangeDetectionStrategy, Component, HostListener, input, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-header',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './header.html',
  styleUrl: './header.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Header {
  readonly transparent = input(false);
  readonly scrolled = signal(window.scrollY > 24);
  readonly menuOpen = signal(false);
  @HostListener('window:scroll') onScroll(): void {
    this.scrolled.set(window.scrollY > 24);
  }
  closeMenu(): void {
    this.menuOpen.set(false);
  }
}
