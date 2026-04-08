import { useState, useEffect, useRef } from 'react'
import { CheckCircle, XCircle, Clock } from 'lucide-react'
import { NavButtons, StepWrapper } from './shared'
import type { Quiz, PortalStep } from '../../services/np_stepApi'
import { submitQuiz } from '../../services/np_stepApi'

interface Props {
  step: PortalStep
  quiz: Quiz
  applicantId: number | null
  onNext: () => void
  onBack: () => void
}

export default function QuizStep({ step, quiz, applicantId, onNext, onBack }: Props) {
  const [answers, setAnswers] = useState<Record<number, string>>({})
  const [quizResult, setQuizResult] = useState<{ score: number; passed: boolean; totalQuestions: number } | null>(null)
  const [quizStarted, setQuizStarted] = useState<Date | null>(null)
  const [timeLeft, setTimeLeft] = useState<number | null>(null)
  const [submitting, setSubmitting] = useState(false)
  const timerRef = useRef<ReturnType<typeof setInterval> | null>(null)

  useEffect(() => {
    if (quizStarted && quiz.timeLimit && timeLeft !== null && timeLeft > 0) {
      timerRef.current = setInterval(() => {
        setTimeLeft(prev => {
          if (prev !== null && prev <= 1) { clearInterval(timerRef.current!); return 0 }
          return prev !== null ? prev - 1 : null
        })
      }, 1000)
      return () => { if (timerRef.current) clearInterval(timerRef.current) }
    }
  }, [quizStarted])

  const handleSubmit = async () => {
    if (!applicantId) return
    setSubmitting(true)
    if (timerRef.current) clearInterval(timerRef.current)
    try {
      const answerList = Object.entries(answers).map(([qId, answer]) => ({ questionId: parseInt(qId), answer }))
      const result = await submitQuiz({
        applicantId, quizId: quiz.id,
        startedDate: quizStarted?.toISOString(),
        timeTaken: quizStarted ? Math.round((Date.now() - quizStarted.getTime()) / 1000) : undefined,
        answers: answerList
      })
      setQuizResult(result)
    } catch { alert('Error submitting quiz') }
    finally { setSubmitting(false) }
  }

  const formatTime = (secs: number) => `${Math.floor(secs / 60)}:${(secs % 60).toString().padStart(2, '0')}`

  return (
    <StepWrapper title={step.title} description={step.description}>
      {quizResult ? (
        <div className="text-center py-8">
          {quizResult.passed ? <CheckCircle size={48} className="mx-auto mb-3 text-green-500" /> : <XCircle size={48} className="mx-auto mb-3 text-red-500" />}
          <h3 className="text-2xl font-bold">{quizResult.passed ? 'Passed!' : 'Not Passed'}</h3>
          <p className="text-gray-600 mt-2">Score: {quizResult.score} ({quizResult.totalQuestions} questions)</p>
          <NavButtons onBack={onBack} onNext={onNext} nextLabel="Continue →" />
        </div>
      ) : !quizStarted ? (
        <div className="text-center py-8">
          <h3 className="text-lg font-semibold">{quiz.title}</h3>
          {quiz.description && <p className="text-gray-500 mt-2">{quiz.description}</p>}
          <div className="flex justify-center gap-6 mt-4 text-sm text-gray-600">
            <span>{quiz.questions?.length || 0} questions</span>
            <span>Pass: {quiz.passingScore}%</span>
            {quiz.timeLimit && <span>Time: {quiz.timeLimit} min</span>}
          </div>
          <button onClick={() => { setQuizStarted(new Date()); if (quiz.timeLimit) setTimeLeft(quiz.timeLimit * 60) }}
            className="mt-6 bg-[#FFD200] text-[#0d0c2c] px-8 py-3 rounded-xl font-semibold hover:bg-[#E87C1E] hover:text-white transition-colors">
            Start Quiz
          </button>
        </div>
      ) : (
        <div>
          {timeLeft !== null && (
            <div className={`flex items-center justify-end gap-2 mb-4 text-sm font-mono ${timeLeft < 60 ? 'text-red-600 animate-pulse' : 'text-gray-600'}`}>
              <Clock size={16} />{formatTime(timeLeft)}
            </div>
          )}
          <div className="space-y-6">
            {quiz.questions?.map((q, i) => {
              const options = q.options ? JSON.parse(q.options) as string[] : []
              return (
                <div key={q.id} className="border rounded-xl p-5">
                  <p className="font-medium mb-3"><span className="text-[#FFD200] font-bold mr-2">Q{i + 1}.</span>{q.questionText}</p>
                  {q.questionType === 'MultiChoice' && options.map((opt, oi) => (
                    <label key={oi} className={`flex items-center gap-3 p-3 rounded-lg cursor-pointer border mb-2 transition-colors
                      ${answers[q.id] === opt ? 'border-[#FFD200] bg-yellow-50' : 'border-gray-200 hover:bg-gray-50'}`}>
                      <input type="radio" name={`q-${q.id}`} checked={answers[q.id] === opt}
                        onChange={() => setAnswers(a => ({ ...a, [q.id]: opt }))} className="accent-[#FFD200]" />
                      <span>{opt}</span>
                    </label>
                  ))}
                  {q.questionType === 'TrueFalse' && ['True', 'False'].map(opt => (
                    <label key={opt} className={`inline-flex items-center gap-2 p-3 rounded-lg cursor-pointer border mr-3 transition-colors
                      ${answers[q.id] === opt ? 'border-[#FFD200] bg-yellow-50' : 'border-gray-200 hover:bg-gray-50'}`}>
                      <input type="radio" name={`q-${q.id}`} checked={answers[q.id] === opt}
                        onChange={() => setAnswers(a => ({ ...a, [q.id]: opt }))} className="accent-[#FFD200]" />
                      <span className="font-medium">{opt}</span>
                    </label>
                  ))}
                  {q.questionType === 'Text' && (
                    <textarea className="w-full border rounded-lg p-3 mt-1" rows={3} placeholder="Your answer..."
                      value={answers[q.id] || ''} onChange={e => setAnswers(a => ({ ...a, [q.id]: e.target.value }))} />
                  )}
                </div>
              )
            })}
          </div>
          <div className="flex items-center justify-between mt-6">
            <span className="text-sm text-gray-500">{Object.keys(answers).length} / {quiz.questions?.length || 0} answered</span>
            <button onClick={handleSubmit} disabled={submitting || Object.keys(answers).length === 0}
              className="bg-[#FFD200] text-[#0d0c2c] px-8 py-3 rounded-xl font-semibold hover:bg-[#E87C1E] hover:text-white disabled:opacity-50 transition-colors">
              {submitting ? 'Submitting...' : 'Submit Quiz'}
            </button>
          </div>
        </div>
      )}
    </StepWrapper>
  )
}
