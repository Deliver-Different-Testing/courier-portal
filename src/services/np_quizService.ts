// TODO: Implement quiz service — stubs for build
const BASE = '/api/quizzes';

export const quizService = {
  async getAll() {
    const res = await fetch(BASE);
    const data = await res.json();
    return data.data ?? [];
  },
  async getById(id: number) {
    const res = await fetch(`${BASE}/${id}`);
    const data = await res.json();
    return data.data;
  },
  async getQuizForDocType(_docTypeId: number) {
    return null;
  },
  async hasPassedQuiz(_courierId: number, _quizId: number) {
    return false;
  },
  async getAttemptCount(_courierId: number, _quizId: number) {
    return 0;
  },
  async getBestScore(_courierId: number, _quizId: number) {
    return 0;
  },
  async submitAttempt(quizId: number, courierId: number, answers: unknown[]) {
    const res = await fetch(`${BASE}/attempts`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ quizId, courierId, answers }),
    });
    const data = await res.json();
    return data.data;
  },
};

// Also export functions directly for namespace imports
export const getQuizzes = quizService.getAll;
export const getQuiz = quizService.getById;
export const submitAttempt = quizService.submitAttempt;
export const getAttempts = async (courierId: number) => {
  const res = await fetch(`${BASE}/attempts/courier/${courierId}`);
  const data = await res.json();
  return data.data ?? [];
};
