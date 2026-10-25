type CarouselAnimation = { target: number; cancel: () => void };

const animations = new WeakMap<HTMLElement, CarouselAnimation>();
const DURATION_MS = 700;

/**
 * Compartilha o movimento horizontal entre cursos e depoimentos.
 * Limita o destino à faixa disponível, respeita redução de movimento e libera o controle ao usuário.
 */
export function scrollCarouselBy(track: HTMLElement, distance: number): void {
  const previous = animations.get(track);
  // Cliques rápidos somam cartões ao destino, sem interromper o movimento com um salto.
  const destination = (previous?.target ?? track.scrollLeft) + distance;
  previous?.cancel();

  const start = track.scrollLeft;
  const target = Math.max(0, Math.min(destination, track.scrollWidth - track.clientWidth));
  if (Math.abs(target - start) < 1) return;
  if (matchMedia('(prefers-reduced-motion: reduce)').matches) {
    track.scrollTo({ left: target, behavior: 'instant' });
    return;
  }

  const snap = track.style.scrollSnapType;
  const behavior = track.style.scrollBehavior;
  // O encaixe nativo volta ao final; durante a animação, ele não deve disputar cada quadro.
  track.style.scrollSnapType = 'none';
  track.style.scrollBehavior = 'auto';
  let frame = 0;
  let startedAt: number | undefined;
  let finished = false;

  // Encerramento único para fim da animação, novo comando ou interação manual.
  const finish = (): void => {
    if (finished) return;
    finished = true;
    cancelAnimationFrame(frame);
    track.style.scrollSnapType = snap;
    track.style.scrollBehavior = behavior;
    track.removeEventListener('wheel', finish);
    track.removeEventListener('pointerdown', finish);
    track.removeEventListener('keydown', finish);
    animations.delete(track);
  };

  const animate = (now: number): void => {
    if (!track.isConnected) {
      finish();
      return;
    }
    startedAt ??= now;
    const progress = Math.min((now - startedAt) / DURATION_MS, 1);
    const eased = progress < 0.5 ? 4 * progress ** 3 : 1 - (-2 * progress + 2) ** 3 / 2;
    track.scrollLeft = start + (target - start) * eased;
    if (progress < 1) frame = requestAnimationFrame(animate);
    else finish();
  };

  animations.set(track, { target, cancel: finish });
  track.addEventListener('wheel', finish, { once: true, passive: true });
  track.addEventListener('pointerdown', finish, { once: true, passive: true });
  track.addEventListener('keydown', finish, { once: true });
  frame = requestAnimationFrame(animate);
}
