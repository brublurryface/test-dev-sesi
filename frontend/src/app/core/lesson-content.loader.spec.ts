import { test } from 'node:test';
import assert from 'node:assert/strict';
import { Subject, finalize } from 'rxjs';
import { LessonContentLoader } from './lesson-content.loader.ts';
import type { LessonCacheLimits, LessonContentState } from './lesson-content.loader.ts';
import type { Lesson } from './course.model.ts';

const lesson = (id: string, text = id): Lesson => ({
  id,
  title: `Aula ${id}`,
  description: '',
  content: [{ kind: 'paragraph', text, items: [] }],
});
function harness(limits?: LessonCacheLimits) {
  const requests: {
    courseSlug: string;
    lessonId: string;
    response: Subject<Lesson>;
    cancelled: boolean;
  }[] = [];
  const loader = new LessonContentLoader((courseSlug, lessonId) => {
    const request = { courseSlug, lessonId, response: new Subject<Lesson>(), cancelled: false };
    requests.push(request);
    return request.response.pipe(
      finalize(() => {
        request.cancelled = true;
      })
    );
  }, limits);
  const states: LessonContentState[] = [];
  const subscription = loader.state$.subscribe((state) => states.push(state));
  return {
    loader,
    requests,
    states,
    subscription,
    last: () => states.at(-1)!,
    select: (lessonId: string, courseSlug = 'curso-a') => loader.select({ courseSlug, lessonId }),
    respond: (index: number, result = lesson(requests[index].lessonId)) => {
      requests[index].response.next(result);
      requests[index].response.complete();
    },
  };
}

test('consulta somente a aula selecionada e limpa o conteúdo enquanto carrega', () => {
  const h = harness();
  try {
    assert.equal(h.requests.length, 0);
    h.select('728');
    assert.equal(h.requests.length, 1);
    assert.equal(h.last().loading, true);
    assert.equal(h.last().lesson, null);
    h.respond(0);
    assert.equal(h.last().lesson?.id, '728');
    h.select('729');
    assert.equal(h.last().selection?.lessonId, '729');
    assert.equal(h.last().lesson, null);
  } finally {
    h.loader.destroy();
  }
});

test('a resposta tardia da 728 não substitui a 729 e a inscrição antiga é cancelada', () => {
  const h = harness();
  try {
    h.select('728');
    h.select('729');
    assert.equal(h.requests[0].cancelled, true);
    h.respond(1);
    h.respond(0);
    assert.equal(h.last().lesson?.id, '729');
    assert.equal(h.last().selection?.lessonId, '729');
    h.select('728');
    assert.equal(h.requests.length, 3); // A resposta cancelada não foi incluída no cache.
  } finally {
    h.loader.destroy();
  }
});

test('voltar a uma aula em cache também cancela a requisição pendente', () => {
  const h = harness();
  try {
    h.select('728');
    h.respond(0);
    h.select('729');
    h.select('728');
    assert.equal(h.requests.length, 2);
    assert.equal(h.requests[1].cancelled, true);
    assert.equal(h.last().lesson?.id, '728');
    h.respond(1);
    assert.equal(h.last().lesson?.id, '728');
  } finally {
    h.loader.destroy();
  }
});

test('a mesma chave de aula em cursos diferentes não compartilha conteúdo', () => {
  const h = harness();
  try {
    h.select('m1-a1', 'curso-a');
    h.respond(0, lesson('m1-a1', 'Curso A'));
    h.select('m1-a1', 'curso-b');
    h.respond(1, lesson('m1-a1', 'Curso B'));
    assert.equal(h.last().lesson?.content[0].text, 'Curso B');
    h.select('m1-a1', 'curso-a');
    assert.equal(h.requests.length, 2);
    assert.equal(h.last().lesson?.content[0].text, 'Curso A');
  } finally {
    h.loader.destroy();
  }
});

test('o limite de entradas remove a aula menos recentemente acessada', () => {
  const h = harness({ maxEntries: 2, maxBytes: 10000 });
  try {
    h.select('a');
    h.respond(0);
    h.select('b');
    h.respond(1);
    h.select('a');
    h.select('c');
    h.respond(2);
    h.select('b');
    assert.deepEqual(
      h.requests.map((request) => request.lessonId),
      ['a', 'b', 'c', 'b']
    );
    assert.equal(h.last().loading, true);
  } finally {
    h.loader.destroy();
  }
});

test('o limite de bytes também remove entradas mesmo abaixo do limite de quantidade', () => {
  const a = lesson('a'),
    b = lesson('b');
  const bytes = (value: Lesson) => new TextEncoder().encode(JSON.stringify(value)).byteLength;
  const h = harness({ maxEntries: 10, maxBytes: bytes(a) + bytes(b) - 1 });
  try {
    h.select('a');
    h.respond(0, a);
    h.select('b');
    h.respond(1, b);
    h.select('a');
    assert.equal(h.requests.length, 3);
  } finally {
    h.loader.destroy();
  }
});

test('uma aula maior que o limite pode ser exibida e não fica em cache', () => {
  const h = harness({ maxEntries: 10, maxBytes: 100 });
  try {
    h.select('grande');
    h.respond(0, lesson('grande', 'texto'.repeat(1000)));
    assert.equal(h.last().lesson?.id, 'grande');
    h.select('grande');
    assert.equal(h.requests.length, 2);
    assert.equal(h.last().lesson, null);
  } finally {
    h.loader.destroy();
  }
});

test('404 e falha transitória não encerram o fluxo nem entram no cache; nova tentativa funciona', () => {
  const h = harness();
  try {
    h.select('a');
    h.requests[0].response.error({ status: 404 });
    assert.equal(h.last().error, 'not-found');
    h.select('b');
    h.requests[1].response.error({ status: 503 });
    assert.equal(h.last().error, 'unavailable');
    h.loader.retry();
    h.respond(2);
    assert.equal(h.last().lesson?.id, 'b');
    assert.equal(h.last().error, null);
    h.select('a');
    assert.equal(h.requests.length, 4);
  } finally {
    h.loader.destroy();
  }
});

test('uma resposta com ID diferente não é exibida nem reutilizada', () => {
  const h = harness();
  try {
    h.select('729');
    h.respond(0, lesson('728'));
    assert.equal(h.last().error, 'unavailable');
    assert.equal(h.last().lesson, null);
    h.loader.retry();
    h.respond(1);
    assert.equal(h.last().lesson?.id, '729');
  } finally {
    h.loader.destroy();
  }
});

test('recarregar o índice limpa o cache e cancela a seleção pendente', () => {
  const h = harness();
  try {
    h.select('a');
    h.respond(0);
    h.select('b');
    h.loader.reset();
    assert.equal(h.requests[1].cancelled, true);
    assert.equal(h.last().selection, null);
    h.select('a');
    assert.equal(h.requests.length, 3);
  } finally {
    h.loader.destroy();
  }
});

test('destruir a AVA interrompe a requisição e encerra a inscrição de estado', () => {
  const h = harness();
  h.select('a');
  h.loader.destroy();
  assert.equal(h.requests[0].cancelled, true);
  assert.equal(h.subscription.closed, true);
  const size = h.states.length;
  h.respond(0);
  assert.equal(h.states.length, size);
});

test('limites inválidos são rejeitados antes de iniciar o carregador', () => {
  assert.throws(
    () => new LessonContentLoader(() => new Subject(), { maxEntries: 0, maxBytes: 100 })
  );
  assert.throws(
    () => new LessonContentLoader(() => new Subject(), { maxEntries: 10, maxBytes: 0 })
  );
});
