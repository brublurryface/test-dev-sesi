/**
 * Contratos das respostas JSON de Contracts/CourseContracts.cs no backend.
 * Estes tipos descrevem os dados recebidos; não carregam nem persistem o catálogo.
 */
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
  category: string;
  modality: string;
  hours: number;
  audience: string;
  status: string;
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
/** Uma página de resumos; total conta todos os resultados compatíveis com os filtros. */
export interface CoursePage {
  items: CourseSummary[];
  total: number;
  filters: CourseFilterOptions;
  page: number;
  pageSize: number;
  totalPages: number;
}

/** Valores distintos do catálogo inteiro, fornecidos pela API junto aos resultados. */
export interface CourseFilterOptions {
  categories: string[];
  modalities: string[];
  statuses: string[];
}
