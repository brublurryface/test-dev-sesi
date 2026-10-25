/**
 * Configurações públicas compartilhadas pela interface.
 * O curso de referência mantém os destinos da demonstração definidos no protótipo;
 * os dados e os links das ofertas individuais continuam vindo da API.
 */
export const SITE_CONFIG = {
  projectName: 'TEST DEV SESI Indústria',
  referenceCourseSlug: 'matematica-para-a-precisao',
  contactEmail: 'contato@sesiindustria.com.br',
  // Deve acompanhar CourseQuery.MaxSearchLength, validado novamente no backend.
  courseSearchMaxLength: 120,
} as const;
