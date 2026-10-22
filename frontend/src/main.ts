import { bootstrapApplication } from '@angular/platform-browser';
import { ViewportScroller } from '@angular/common';
import { provideHttpClient } from '@angular/common/http';
import { inject, provideAppInitializer } from '@angular/core';
import { provideRouter, withInMemoryScrolling } from '@angular/router';
import { App } from './app/app';
import { routes } from './app/app.routes';

bootstrapApplication(App, {
  providers: [
    provideHttpClient(),
    provideAppInitializer(() => {
      // Desconta a altura atual do header fixo para manter os títulos das âncoras visíveis.
      inject(ViewportScroller).setOffset(() => [
        0,
        (document.querySelector('app-header header')?.getBoundingClientRect().height ?? 0) + 24,
      ]);
    }),
    // Preserva a posição ao voltar e permite que links como /#sobre alcancem a seção de destino.
    provideRouter(
      routes,
      withInMemoryScrolling({ scrollPositionRestoration: 'enabled', anchorScrolling: 'enabled' })
    ),
  ],
}).catch((error) => console.error('Não foi possível iniciar a aplicação.', error));
