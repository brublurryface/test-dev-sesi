import { Routes } from '@angular/router';
import { SITE_CONFIG } from './core/site.config';

// Rotas de tela do Angular, distintas dos endpoints /api definidos no backend.
// O carregamento sob demanda mantém as páginas fora do pacote inicial até serem acessadas.
export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    title: `${SITE_CONFIG.projectName} | Conhecimento que transforma`,
    loadComponent: () => import('./pages/home/home').then((m) => m.Home),
  },
  {
    path: 'cursos',
    title: `Cursos | ${SITE_CONFIG.projectName}`,
    loadComponent: () => import('./pages/catalog/catalog').then((m) => m.Catalog),
  },
  {
    path: 'cursos/:slug',
    loadComponent: () => import('./pages/course-detail/course-detail').then((m) => m.CourseDetail),
  },
  {
    path: 'ava/:slug',
    loadComponent: () => import('./pages/learning/learning').then((m) => m.Learning),
  },
  {
    path: '**',
    title: `Página não encontrada | ${SITE_CONFIG.projectName}`,
    loadComponent: () => import('./pages/not-found/not-found').then((m) => m.NotFound),
  },
];
