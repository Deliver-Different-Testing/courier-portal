import api from './np_api';
import type { QuizDefinition, QuizAttempt } from '@/types';

export const quizService = {
  async getQuizForDocType(documentTypeId: number): Promise<QuizDefinition | null> {
    try {
      const { data } = await api.get<QuizDefinition | null>(`/quizzes`, { params: { documentTypeId } });
      return data;
    } catch (e) {
      console.error('quizService.getQuizForDocType failed:', e);
      return null;
    }
  },

  async getQuizById(quizId: number): Promise<QuizDefinition | null> {
    try {
      const { data } = await api.get<QuizDefinition>(`/quizzes/${quizId}`);
      return data;
    } catch (e) {
      console.error('quizService.getQuizById failed:', e);
      return null;
    }
  },

  async getAllQuizzes(): Promise<QuizDefinition[]> {
    try {
      const { data } = await api.get<QuizDefinition[]>('/quizzes');
      return data;
    } catch (e) {
      console.error('quizService.getAllQuizzes failed:', e);
      return [];
    }
  },

  async saveQuiz(quiz: Partial<QuizDefinition> & { documentTypeId: number; title: string }): Promise<QuizDefinition> {
    if (quiz.id) {
      const { data } = await api.put<QuizDefinition>(`/quizzes/${quiz.id}`, quiz);
      return data;
    }
    const { data } = await api.post<QuizDefinition>('/quizzes', quiz);
    return data;
  },

  async deleteQuiz(id: number): Promise<void> {
    await api.delete(`/quizzes/${id}`);
  },

  async getAttempts(quizId: number, courierId?: number): Promise<QuizAttempt[]> {
    try {
      const params: Record<string, number> = {};
      if (courierId !== undefined) params.courierId = courierId;
      const { data } = await api.get<QuizAttempt[]>(`/quizzes/${quizId}/attempts`, { params });
      return data;
    } catch (e) {
      console.error('quizService.getAttempts failed:', e);
      return [];
    }
  },

  async submitAttempt(quizId: number, courierId: number, answers: { questionId: number; selectedOptionIds: number[] }[]): Promise<QuizAttempt> {
    const { data } = await api.post<QuizAttempt>(`/quizzes/${quizId}/attempts`, { courierId, answers });
    return data;
  },

  async hasPassedQuiz(quizId: number, courierId: number): Promise<boolean> {
    try {
      const { data } = await api.get<{ passed: boolean }>(`/quizzes/${quizId}/attempts/passed`, { params: { courierId } });
      return data.passed;
    } catch (e) {
      console.error('quizService.hasPassedQuiz failed:', e);
      return false;
    }
  },

  async getAttemptCount(quizId: number, courierId: number): Promise<number> {
    try {
      const { data } = await api.get<{ count: number }>(`/quizzes/${quizId}/attempts/count`, { params: { courierId } });
      return data.count;
    } catch (e) {
      console.error('quizService.getAttemptCount failed:', e);
      return 0;
    }
  },

  async getBestScore(quizId: number, courierId: number): Promise<number | null> {
    try {
      const { data } = await api.get<{ bestScore: number | null }>(`/quizzes/${quizId}/attempts/best`, { params: { courierId } });
      return data.bestScore;
    } catch (e) {
      console.error('quizService.getBestScore failed:', e);
      return null;
    }
  },
};
