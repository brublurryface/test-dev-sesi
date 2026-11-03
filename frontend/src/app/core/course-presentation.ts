/** Ícones da referência são preservados; novas categorias recebem um ícone neutro de curso. */
export function categoryIcon(category: string): string {
  switch (category) {
    case 'Matemática':
      return 'assets/icon-matematica-20px.svg';
    case 'Ciências da Natureza':
      return 'assets/icon-ciencias-natureza-20px.svg';
    case 'Sustentabilidade':
      return 'assets/icon-sustentabilidade-20px.svg';
    default:
      return 'assets/icon-curso-20px.svg';
  }
}

/** Um novo status mantém seu rótulo, sem ser confundido com inscrições encerradas. */
export function enrollmentLabel(status: string): string {
  switch (status) {
    case 'Disponível':
      return 'Matricule-se';
    case 'Encerrado':
      return 'Inscrições encerradas';
    default:
      return status;
  }
}
