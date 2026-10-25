/**
 * Contratos das respostas JSON de Contracts/CourseContracts.cs no backend.
 * Estes tipos descrevem os dados recebidos; não carregam nem persistem o catálogo.
 */
export type Category = 'Matemática' | 'Ciências da Natureza' | 'Sustentabilidade';
export type Modality = 'Online' | 'Presencial';
export type CourseStatus = 'Disponível' | 'Em breve' | 'Encerrado';

// O identificador da aula deve permanecer estável: o progresso local usa esse valor.
export interface Lesson {
  id: string;
  title: string;
  description: string;
  content: ContentBlock[];
}
/** Blocos de texto estruturado evitam inserir HTML recebido da API. */
export interface ContentBlock {
  kind: 'paragraph' | 'heading' | 'list' | 'note';
  text: string;
  items: string[];
}
export interface CourseModule {
  title: string;
  description: string;
  topics: string[];
  lessons: Lesson[];
}
/** A listagem não precisa transferir módulos, aulas e textos de cada curso. */
export interface CourseSummary {
  id: number;
  slug: string;
  name: string;
  shortName: string;
  description: string;
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
}
/** Resposta de detalhes, compartilhada pela página do curso e pela AVA. */
export interface Course extends CourseSummary {
  about: string;
  certification: string;
  modules: CourseModule[];
  introduction: Lesson[];
  faqs: { question: string; answer: string }[];
  onlineInformation: ContentBlock[];
}
/** Resultado sem paginação: a contagem é fornecida pela API para a consulta atual. */
export interface CoursePage {
  items: CourseSummary[];
  total: number;
}

// Opções dos controles da interface; CourseQuery.cs também valida esses valores no servidor.
export const CATEGORIES: Category[] = ['Matemática', 'Ciências da Natureza', 'Sustentabilidade'];
export const MODALITIES: Modality[] = ['Online', 'Presencial'];
export const STATUSES: CourseStatus[] = ['Disponível', 'Em breve', 'Encerrado'];
