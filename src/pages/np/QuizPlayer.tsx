import { useState } from 'react';

interface QuizPlayerProps {
  quizId: number;
  courierId: number;
  onComplete?: (passed: boolean, score: number) => void;
  onClose?: () => void;
}

export default function QuizPlayer({ quizId, courierId, onComplete, onClose }: QuizPlayerProps) {
  const [status, setStatus] = useState<'loading' | 'active' | 'complete'>('loading');

  return (
    <div className="bg-white rounded-lg shadow p-6">
      <h2 className="text-xl font-bold mb-4">Quiz #{quizId}</h2>
      {status === 'loading' && <p className="text-gray-500">Loading quiz...</p>}
      {status === 'complete' && <p className="text-green-600">Quiz complete!</p>}
      <div className="mt-4 flex gap-2">
        {onClose && (
          <button onClick={onClose} className="px-4 py-2 bg-gray-200 rounded hover:bg-gray-300">
            Close
          </button>
        )}
      </div>
      {/* TODO: Implement full quiz player with questions, answers, and scoring */}
    </div>
  );
}
