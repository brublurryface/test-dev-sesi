export type Category = 'Matemática' | 'Ciências da Natureza' | 'Sustentabilidade';
export type Modality = 'Online' | 'Presencial';
export type CourseStatus = 'Disponível' | 'Em breve' | 'Encerrado';

// O identificador da aula deve permanecer estável: o progresso local usa esse valor.
export interface Lesson {
  id: string;
  title: string;
  description: string;
}
export interface CourseModule {
  title: string;
  description: string;
  topics: string[];
  lessons: Lesson[];
}
/** Contrato compartilhado pelas páginas do catálogo e pela futura integração com a API. */
export interface Course {
  id: number;
  slug: string;
  name: string;
  shortName: string;
  description: string;
  about: string;
  category: Category;
  modality: Modality;
  hours: number;
  audience: string;
  status: CourseStatus;
  image: string;
  imageAlt: string;
  /** Data de calendário (YYYY-MM-DD), exibida com UTC para preservar o dia informado. */
  availableAt: string | null;
  featured: boolean;
  modules: CourseModule[];
}

export const CATEGORIES: Category[] = ['Matemática', 'Ciências da Natureza', 'Sustentabilidade'];
export const MODALITIES: Modality[] = ['Online', 'Presencial'];
export const STATUSES: CourseStatus[] = ['Disponível', 'Em breve', 'Encerrado'];
