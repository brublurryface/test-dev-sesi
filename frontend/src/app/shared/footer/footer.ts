import { ChangeDetectionStrategy, Component, ElementRef, signal, viewChild } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-footer',
  imports: [RouterLink],
  templateUrl: './footer.html',
  styleUrl: './footer.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Footer {
  readonly infoTitle = signal('');
  readonly info = viewChild.required<ElementRef<HTMLDialogElement>>('info');
  openInfo(title: string): void {
    this.infoTitle.set(title);
    this.info().nativeElement.showModal();
  }
  closeInfo(): void {
    this.info().nativeElement.close();
  }
}
