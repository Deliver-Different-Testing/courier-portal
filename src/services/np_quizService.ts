import type { QuizDefinition, QuizQuestion, QuizOption, QuizAttempt, QuizAttemptAnswer } from '@/types';

const STORAGE_KEY = 'np_quizzes_v2';
const ATTEMPTS_KEY = 'np_quiz_attempts_v2';

let _nextId = 100;
function nextId() { return _nextId++; }

function loadQuizzes(): QuizDefinition[] {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (raw) return JSON.parse(raw);
  const seed = getSeedQuizzes();
  localStorage.setItem(STORAGE_KEY, JSON.stringify(seed));
  return seed;
}

function saveQuizzes(quizzes: QuizDefinition[]) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(quizzes));
}

function loadAttempts(): QuizAttempt[] {
  const raw = localStorage.getItem(ATTEMPTS_KEY);
  return raw ? JSON.parse(raw) : [];
}

function saveAttempts(attempts: QuizAttempt[]) {
  localStorage.setItem(ATTEMPTS_KEY, JSON.stringify(attempts));
}

function getSeedQuizzes(): QuizDefinition[] {
  return [
    {
      id: 1,
      documentTypeId: 7,
      tenantId: 1,
      title: 'Health & Safety Induction Quiz',
      description: 'Complete this quiz after watching the Safety Induction Video to confirm your understanding of workplace safety procedures.',
      passMarkPercent: 80,
      maxAttempts: 3,
      randomizeQuestions: false,
      randomizeOptions: false,
      timeLimitMinutes: 15,
      active: true,
      createdDate: '2025-01-15T10:00:00Z',
      questions: [
        mkQ(1, 1, 'What should you do first if you discover a fire in the workplace?', 'multiple_choice', 1, [
          mkO(1, 1, 'Try to extinguish it yourself', false),
          mkO(2, 1, 'Activate the nearest fire alarm and evacuate', true),
          mkO(3, 1, 'Call your manager', false),
          mkO(4, 1, 'Continue working until told to stop', false),
        ], 'The first priority is always to alert others and evacuate. Only attempt to fight a fire if it is small and you are trained.'),
        mkQ(2, 1, 'Personal Protective Equipment (PPE) is optional when handling packages.', 'true_false', 2, [
          mkO(5, 2, 'True', false),
          mkO(6, 2, 'False', true),
        ], 'PPE is mandatory whenever handling packages, especially those that are heavy or may contain hazardous materials.'),
        mkQ(3, 1, 'Which of the following are correct manual handling techniques? (Select all that apply)', 'multi_select', 3, [
          mkO(7, 3, 'Bend your knees, not your back', true),
          mkO(8, 3, 'Keep the load close to your body', true),
          mkO(9, 3, 'Twist your body while carrying heavy items', false),
          mkO(10, 3, 'Ask for help if the load is too heavy', true),
        ], 'Proper lifting technique protects your back and prevents injury.'),
        mkQ(4, 1, 'What is the maximum weight a single person should lift without assistance?', 'multiple_choice', 4, [
          mkO(11, 4, '10 kg', false),
          mkO(12, 4, '25 kg', true),
          mkO(13, 4, '50 kg', false),
          mkO(14, 4, 'There is no limit', false),
        ], 'The recommended maximum for a single person is 25 kg. Heavier loads require mechanical aids or team lifting.'),
        mkQ(5, 1, 'You must report all workplace injuries, no matter how minor.', 'true_false', 5, [
          mkO(15, 5, 'True', true),
          mkO(16, 5, 'False', false),
        ], 'All injuries must be reported for safety records and to identify potential hazards.'),
      ],
    },
    {
      id: 2,
      documentTypeId: 99,
      tenantId: 1,
      title: 'Cold Chain Training Quiz',
      description: 'Test your knowledge of temperature-controlled delivery procedures.',
      passMarkPercent: 75,
      maxAttempts: null,
      randomizeQuestions: true,
      randomizeOptions: false,
      timeLimitMinutes: null,
      active: true,
      createdDate: '2025-02-01T10:00:00Z',
      questions: [
        mkQ(6, 2, 'What is the maximum temperature for chilled goods during transport?', 'multiple_choice', 1, [
          mkO(17, 6, '0°C', false),
          mkO(18, 6, '5°C', true),
          mkO(19, 6, '10°C', false),
          mkO(20, 6, '15°C', false),
        ], 'Chilled goods must be kept at or below 5°C to prevent bacterial growth.'),
        mkQ(7, 2, 'How often should you check the temperature of your cargo during a delivery run?', 'multiple_choice', 2, [
          mkO(21, 7, 'Only at loading', false),
          mkO(22, 7, 'At each delivery stop', true),
          mkO(23, 7, 'Once per hour', false),
          mkO(24, 7, 'Only if the alarm goes off', false),
        ], 'Temperature should be checked at each stop to ensure compliance throughout the route.'),
        mkQ(8, 2, 'Frozen goods can be refrozen after thawing if done within 2 hours.', 'true_false', 3, [
          mkO(25, 8, 'True', false),
          mkO(26, 8, 'False', true),
        ], 'Refreezing thawed goods is not permitted as it compromises food safety.'),
        mkQ(9, 2, 'Which actions should you take if a temperature breach is detected? (Select all)', 'multi_select', 4, [
          mkO(27, 9, 'Notify dispatch immediately', true),
          mkO(28, 9, 'Record the temperature reading', true),
          mkO(29, 9, 'Continue delivering as normal', false),
          mkO(30, 9, 'Segregate affected goods', true),
        ], 'Immediate notification, documentation, and segregation are all required steps.'),
      ],
    },
  ];
}

function mkQ(id: number, quizId: number, text: string, type: QuizQuestion['questionType'], order: number, options: QuizOption[], explanation?: string): QuizQuestion {
  return { id, quizDefinitionId: quizId, questionText: text, questionType: type, sortOrder: order, points: 1, active: true, options, explanation };
}

function mkO(id: number, qId: number, text: string, correct: boolean): QuizOption {
  return { id, questionId: qId, optionText: text, isCorrect: correct, sortOrder: id };
}

export const quizService = {
  getQuizForDocType(documentTypeId: number): QuizDefinition | null {
    return loadQuizzes().find(q => q.documentTypeId === documentTypeId && q.active) || null;
  },

  getQuizById(quizId: number): QuizDefinition | null {
    return loadQuizzes().find(q => q.id === quizId) || null;
  },

  getAllQuizzes(): QuizDefinition[] {
    return loadQuizzes();
  },

  saveQuiz(quiz: Partial<QuizDefinition> & { documentTypeId: number; title: string }): QuizDefinition {
    const quizzes = loadQuizzes();
    if (quiz.id) {
      const idx = quizzes.findIndex(q => q.id === quiz.id);
      if (idx >= 0) {
        quizzes[idx] = { ...quizzes[idx], ...quiz, modifiedDate: new Date().toISOString() };
        saveQuizzes(quizzes);
        return quizzes[idx];
      }
    }
    const newQuiz: QuizDefinition = {
      id: nextId(),
      documentTypeId: quiz.documentTypeId,
      tenantId: quiz.tenantId || 1,
      title: quiz.title,
      description: quiz.description || '',
      passMarkPercent: quiz.passMarkPercent ?? 80,
      maxAttempts: quiz.maxAttempts ?? null,
      randomizeQuestions: quiz.randomizeQuestions ?? false,
      randomizeOptions: quiz.randomizeOptions ?? false,
      timeLimitMinutes: quiz.timeLimitMinutes ?? null,
      active: quiz.active ?? true,
      questions: quiz.questions || [],
      createdDate: new Date().toISOString(),
    };
    quizzes.push(newQuiz);
    saveQuizzes(quizzes);
    return newQuiz;
  },

  deleteQuiz(id: number): void {
    const quizzes = loadQuizzes().filter(q => q.id !== id);
    saveQuizzes(quizzes);
  },

  getAttempts(quizId: number, courierId?: number): QuizAttempt[] {
    let attempts = loadAttempts().filter(a => a.quizDefinitionId === quizId);
    if (courierId !== undefined) attempts = attempts.filter(a => a.courierId === courierId);
    return attempts;
  },

  submitAttempt(quizId: number, courierId: number, answers: { questionId: number; selectedOptionIds: number[] }[]): QuizAttempt {
    const quiz = this.getQuizById(quizId);
    if (!quiz) throw new Error('Quiz not found');

    const startedAt = new Date(Date.now() - 300000).toISOString();
    const completedAt = new Date().toISOString();
    let correctCount = 0;
    const totalPoints = quiz.questions.filter(q => q.active).reduce((s, q) => s + q.points, 0);
    let earnedPoints = 0;

    const attemptAnswers: QuizAttemptAnswer[] = answers.map((a, i) => {
      const question = quiz.questions.find(q => q.id === a.questionId);
      if (!question) return { id: nextId(), attemptId: 0, questionId: a.questionId, selectedOptionIds: a.selectedOptionIds, isCorrect: false };

      const correctOptionIds = question.options.filter(o => o.isCorrect).map(o => o.id).sort();
      const selectedSorted = [...a.selectedOptionIds].sort();
      const isCorrect = correctOptionIds.length === selectedSorted.length && correctOptionIds.every((v, j) => v === selectedSorted[j]);

      if (isCorrect) { correctCount++; earnedPoints += question.points; }

      return { id: nextId(), attemptId: 0, questionId: a.questionId, selectedOptionIds: a.selectedOptionIds, isCorrect };
    });

    const score = totalPoints > 0 ? Math.round((earnedPoints / totalPoints) * 100) : 0;
    const passed = score >= quiz.passMarkPercent;

    const attempt: QuizAttempt = {
      id: nextId(),
      quizDefinitionId: quizId,
      courierId,
      startedAt,
      completedAt,
      score,
      passed,
      totalQuestions: quiz.questions.filter(q => q.active).length,
      correctAnswers: correctCount,
      timeTakenSeconds: Math.floor((new Date(completedAt).getTime() - new Date(startedAt).getTime()) / 1000),
      answers: attemptAnswers.map(a => ({ ...a, attemptId: 0 })),
    };
    attempt.answers.forEach(a => a.attemptId = attempt.id);

    const attempts = loadAttempts();
    attempts.push(attempt);
    saveAttempts(attempts);
    return attempt;
  },

  hasPassedQuiz(quizId: number, courierId: number): boolean {
    return loadAttempts().some(a => a.quizDefinitionId === quizId && a.courierId === courierId && a.passed);
  },

  getAttemptCount(quizId: number, courierId: number): number {
    return loadAttempts().filter(a => a.quizDefinitionId === quizId && a.courierId === courierId).length;
  },

  getBestScore(quizId: number, courierId: number): number | null {
    const attempts = loadAttempts().filter(a => a.quizDefinitionId === quizId && a.courierId === courierId && a.score !== undefined);
    if (attempts.length === 0) return null;
    return Math.max(...attempts.map(a => a.score!));
  },
};
