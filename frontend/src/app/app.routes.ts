import { Routes } from '@angular/router';

// O carregamento sob demanda mantém as páginas fora do pacote inicial até serem acessadas.
export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    title: 'TEST DEV SESI Indústria | Conhecimento que transforma',
    loadComponent: () => import('./pages/home/home').then((m) => m.Home),
  },
  {
    path: 'cursos',
    title: 'Cursos | TEST DEV SESI Indústria',
    loadComponent: () => import('./pages/catalog/catalog').then((m) => m.Catalog),
  },
  {
    path: 'cursos/:slug',
    loadComponent: () => import('./pages/course-detail/course-detail').then((m) => m.CourseDetail),
  },
  {
    path: '**',
    title: 'Página não encontrada | TEST DEV SESI Indústria',
    loadComponent: () => import('./pages/not-found/not-found').then((m) => m.NotFound),
  },
];
