import api from './np_api';
import type { QuizDefinition, QuizAttempt } from '@/types';

// ─── Response shapes from QuizController ───────────────────────────────────
// GET /api/quizzes         → { data: QuizDefinition[] }
// GET /api/quizzes/{id}    → { data: QuizDefinition }
// POST /api/quizzes        → { data: QuizDefinition }
// PUT /api/quizzes/{id}    → { data: QuizDefinition }
// DELETE /api/quizzes/{id} → { success: true }
// POST /api/quizzes/attempts → { data: QuizAttempt }  (SubmitQuizAttemptDto)
// GET /api/quizzes/{id}/attempts → { data: QuizAttempt[] }
// GET /api/quizzes/attempts/courier/{courierId} → { data: QuizAttempt[] }

export const quizService = {
  /** @backend GET /api/quizzes */
  async getAllQuizzes(): Promise<QuizDefinition[]> {
    try {
      const { data } = await api.get<{ data: QuizDefinition[] }>('/quizzes');
      return data.data ?? [];
    } catch (e) {
      console.error('quizService.getAllQuizzes failed:', e);
      return [];
    }
  },

  /** @backend GET /api/quizzes?includeInactive=true */
  async getAllQuizzesAdmin(includeInactive = false): Promise<QuizDefinition[]> {
    try {
      const { data } = await api.get<{ data: QuizDefinition[] }>('/quizzes', {
        params: { includeInactive },
      });
      return data.data ?? [];
    } catch (e) {
      console.error('quizService.getAllQuizzesAdmin failed:', e);
      return [];
    }
  },

  /** @backend GET /api/quizzes/{id} */
  async getQuizById(quizId: number): Promise<QuizDefinition | null> {
    try {
      const { data } = await api.get<{ data: QuizDefinition }>(`/quizzes/${quizId}`);
      return data.data ?? null;
    } catch (e) {
      console.error(`quizService.getQuizById(${quizId}) failed:`, e);
      return null;
    }
  },

  /** @backend GET /api/quizzes — filtered client-side by documentTypeId */
  async getQuizForDocType(documentTypeId: number): Promise<QuizDefinition | null> {
    const quizzes = await this.getAllQuizzes();
    return quizzes.find(q => q.documentTypeId === documentTypeId && q.active) ?? null;
  },

  /** @backend POST /api/quizzes */
  async saveQuiz(quiz: Partial<QuizDefinition> & { documentTypeId: number; title: string }): Promise<QuizDefinition> {
    if (quiz.id) {
      // Update
      const { data } = await api.put<{ data: QuizDefinition }>(`/quizzes/${quiz.id}`, quiz);
      return data.data;
    } else {
      // Create
      const { data } = await api.post<{ data: QuizDefinition }>('/quizzes', quiz);
      return data.data;
    }
  },

  /** @backend DELETE /api/quizzes/{id} */
  async deleteQuiz(id: number): Promise<void> {
    await api.delete(`/quizzes/${id}`);
  },

  /**
   * Submit a quiz attempt.
   * @backend POST /api/quizzes/attempts
   * Body: SubmitQuizAttemptDto { quizDefinitionId, courierId, answers: [{ questionId, selectedOptionIds }] }
   */
  async submitAttempt(
    quizId: number,
    courierId: number,
    answers: { questionId: number; selectedOptionIds: number[] }[]
  ): Promise<QuizAttempt> {
    const { data } = await api.post<{ data: QuizAttempt }>('/quizzes/attempts', {
      quizDefinitionId: quizId,
      courierId,
      answers,
    });
    return data.data;
  },

  /** @backend GET /api/quizzes/{quizId}/attempts */
  async getAttempts(quizId: number): Promise<QuizAttempt[]> {
    try {
      const { data } = await api.get<{ data: QuizAttempt[] }>(`/quizzes/${quizId}/attempts`);
      return data.data ?? [];
    } catch (e) {
      console.error(`quizService.getAttempts(${quizId}) failed:`, e);
      return [];
    }
  },

  /** @backend GET /api/quizzes/attempts/courier/{courierId} */
  async getAttemptsByCourier(courierId: number): Promise<QuizAttempt[]> {
    try {
      const { data } = await api.get<{ data: QuizAttempt[] }>(`/quizzes/attempts/courier/${courierId}`);
      return data.data ?? [];
    } catch (e) {
      console.error(`quizService.getAttemptsByCourier(${courierId}) failed:`, e);
      return [];
    }
  },

  /** Convenience: check if a courier has passed a quiz */
  async hasPassedQuiz(quizId: number, courierId: number): Promise<boolean> {
    const attempts = await this.getAttempts(quizId);
    return attempts.some(a => a.courierId === courierId && a.passed);
  },

  /** Convenience: get courier attempt count for a quiz */
  async getAttemptCount(quizId: number, courierId: number): Promise<number> {
    const attempts = await this.getAttempts(quizId);
    return attempts.filter(a => a.courierId === courierId).length;
  },

  /** Convenience: get best score for a courier on a quiz */
  async getBestScore(quizId: number, courierId: number): Promise<number | null> {
    const attempts = await this.getAttempts(quizId);
    const courierAttempts = attempts.filter(a => a.courierId === courierId && a.score !== undefined);
    if (courierAttempts.length === 0) return null;
    return Math.max(...courierAttempts.map(a => a.score!));
  },
};

// ─── Legacy sync shims ───────────────────────────────────────────────────────
// ComplianceDashboard and other pages call these synchronously inside JSX render.
// These stubs return null/false/0 immediately (safe defaults) while the real data
// is loaded elsewhere via useEffect/useState. Pages that need real data should
// call the async methods above in their component hooks.

/** @deprecated Use quizService.getQuizForDocType() (async) instead */
export function getQuizForDocTypeSync(_documentTypeId: number): QuizDefinition | null {
  // FALLBACK: returns null until component loads quiz data via useEffect
  return null;
}

/** @deprecated Use quizService.hasPassedQuiz() (async) instead */
export function hasPassedQuizSync(_quizId: number, _courierId: number): boolean {
  // FALLBACK: returns false until component loads attempt data via useEffect
  return false;
}

/** @deprecated Use quizService.getAttemptCount() (async) instead */
export function getAttemptCountSync(_quizId: number, _courierId: number): number {
  // FALLBACK: returns 0 until component loads attempt data via useEffect
  return 0;
}
