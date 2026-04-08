import { useState, useEffect, useRef } from 'react'
import { Routes, Route, useNavigate } from 'react-router-dom'
import * as api from '../lib/api'
import { getStepType } from '../lib/stepTypes'
import { CheckCircle, XCircle, Clock, FileText, AlertCircle, Camera, ChevronRight, Car, Shield, FileCheck, Building, Hash, Loader2, Eye } from 'lucide-react'
import DetailsStep from './steps/DetailsStep'
import VehicleStep from './steps/VehicleStep'
import DriverLicenseStep from './steps/DriverLicenseStep'
import DocumentUploadStep from './steps/DocumentUploadStep'
import GenericDocumentStep from './steps/GenericDocumentStep'
import ReviewStep from './steps/ReviewStep'

// ─── Dynamic Step Indicator ──────────────────────────────
function StepIndicator({ steps, current }: { steps: { key: string; label: string }[]; current: number }) {
  return (
    <div className="bg-white border-b sticky top-0 z-20">
      <div className="max-w-3xl mx-auto px-4 py-3 overflow-x-auto">
        <div className="flex items-center min-w-max">
          {steps.map((s, i) => (
            <div key={s.key} className="flex items-center">
              <div className={`flex items-center gap-1.5 ${i <= current ? '' : 'opacity-40'}`}>
                <div className={`w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold shrink-0
                  ${i < current ? 'bg-green-500 text-white' : i === current ? 'bg-[#0d0c2c] text-white' : 'bg-gray-200 text-gray-600'}`}>
                  {i < current ? '✓' : i + 1}
                </div>
                <span className="text-xs font-medium text-gray-700 hidden sm:inline max-w-[70px] truncate">{s.label}</span>
              </div>
              {i < steps.length - 1 && <div className={`w-4 sm:w-6 h-0.5 mx-1 ${i < current ? 'bg-green-400' : 'bg-gray-200'}`} />}
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}

// ─── Main Apply Flow (STEP-DRIVEN) ──────────────────────
const PREVIEW_DUMMY_DATA: Record<string, any> = {
  firstName: 'Jane', lastName: 'Smith', email: 'jane.smith@example.com',
  phone: '021 555 0123', dateOfBirth: '1990-05-15',
  address: '42 Queen Street', city: 'Auckland', region: 'Auckland', postcode: '1010',
  source: 'Website', vehicleType: 'Van', hasOwnVehicle: true,
  vehicleMake: 'Toyota', vehicleModel: 'HiAce', vehicleYear: '2020',
  registrationPlate: 'ABC123', licenseType: 'Class 2', licenseExpiry: '2028-12-01',
  licenseNumber: 'AB123456',
}

function ApplyFlow() {
  const isPreview = new URLSearchParams(window.location.search).get('preview') === 'true'
  const [config, setConfig] = useState<api.PortalConfig | null>(null)
  const [loading, setLoading] = useState(true)
  const [step, setStep] = useState(0)
  const [form, setForm] = useState<Record<string, any>>(isPreview ? { ...PREVIEW_DUMMY_DATA } : {
    firstName: '', lastName: '', email: '', phone: '', dateOfBirth: '',
    address: '', city: '', region: '', postcode: '', source: '',
    vehicleType: '', hasOwnVehicle: false, vehicleMake: '', vehicleModel: '', vehicleYear: '',
    registrationPlate: '', licenseType: '', licenseExpiry: '', licenseNumber: '',
  })
  const [applicantId, setApplicantId] = useState<number | null>(null)
  const [files, setFiles] = useState<Record<string, File | null>>({})
  const [uploading, setUploading] = useState<string | null>(null)
  const [uploadedDocs, setUploadedDocs] = useState<Set<number>>(new Set())
  const [stepFieldData, setStepFieldData] = useState<Record<number, Record<string, any>>>({})
  const [vehiclePhotos, setVehiclePhotos] = useState<Record<string, File | null>>({})
  const [answers, setAnswers] = useState<Record<number, string>>({})
  const [quizResult, setQuizResult] = useState<{ score: number; passed: boolean; totalQuestions: number } | null>(null)
  const [quizStarted, setQuizStarted] = useState<Date | null>(null)
  const [timeLeft, setTimeLeft] = useState<number | null>(null)
  const [submittingQuiz, setSubmittingQuiz] = useState(false)
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)
  const [submitted, setSubmitted] = useState(false)
  const timerRef = useRef<ReturnType<typeof setInterval> | null>(null)

  const update = (field: string, value: string | boolean) => setForm(f => ({ ...f, [field]: value }))

  useEffect(() => {
    api.getPortalConfig().then(c => { setConfig(c); setLoading(false) }).catch(() => setLoading(false))
  }, [])

  // Build steps from config.steps (new flow) or fall back to old toggles
  const portalSteps = config?.steps && config.steps.length > 0 ? config.steps : []

  // Quiz timer
  useEffect(() => {
    if (quizStarted && config?.quiz?.timeLimit && timeLeft !== null && timeLeft > 0) {
      timerRef.current = setInterval(() => {
        setTimeLeft(prev => {
          if (prev !== null && prev <= 1) { clearInterval(timerRef.current!); return 0 }
          return prev !== null ? prev - 1 : null
        })
      }, 1000)
      return () => { if (timerRef.current) clearInterval(timerRef.current) }
    }
  }, [quizStarted])

  const saveApplicant = async () => {
    setSaving(true); setError('')
    try {
      const result = await api.applyAsApplicant({
        firstName: form.firstName, lastName: form.lastName, email: form.email,
        phone: form.phone, address: form.address, city: form.city, region: form.region,
        vehicleType: form.vehicleType, hasOwnVehicle: form.hasOwnVehicle,
        licenseType: form.licenseType, licenseExpiry: form.licenseExpiry || undefined,
        source: form.source,
        notes: [form.postcode && `Postcode: ${form.postcode}`, form.dateOfBirth && `DOB: ${form.dateOfBirth}`,
          form.vehicleMake && `Vehicle: ${form.vehicleMake} ${form.vehicleModel} ${form.vehicleYear}`,
          form.licenseNumber && `License #: ${form.licenseNumber}`,
          form.registrationPlate && `Rego: ${form.registrationPlate}`].filter(Boolean).join('; ') || undefined,
      } as any)
      setApplicantId(result.id)
      return result.id
    } catch (e: any) { setError(e.message || 'Error saving'); return null }
    finally { setSaving(false) }
  }

  const handleUpload = async (docTypeId: number, file: File, fileKey?: string) => {
    let id = applicantId
    if (!id) { id = await saveApplicant(); if (!id) return }
    const key = fileKey || `doc-${docTypeId}`
    setUploading(key)
    setFiles(f => ({ ...f, [key]: file }))
    try {
      await api.uploadDocument(id, docTypeId, file)
      setUploadedDocs(prev => new Set([...prev, docTypeId]))
    } catch (e: any) { alert('Upload failed: ' + (e.message || 'Unknown error')) }
    finally { setUploading(null) }
  }

  const handleSubmitQuiz = async () => {
    if (!applicantId || !config?.quiz) return
    setSubmittingQuiz(true)
    if (timerRef.current) clearInterval(timerRef.current)
    try {
      const answerList = Object.entries(answers).map(([qId, answer]) => ({ questionId: parseInt(qId), answer }))
      const result = await api.submitQuiz({
        applicantId, quizId: config.quiz.id,
        startedDate: quizStarted?.toISOString(),
        timeTaken: quizStarted ? Math.round((Date.now() - quizStarted.getTime()) / 1000) : undefined,
        answers: answerList
      })
      setQuizResult(result)
    } catch { alert('Error submitting quiz') }
    finally { setSubmittingQuiz(false) }
  }

  const handleNext = async () => {
    if (isPreview) {
      // Preview mode — skip all API calls, just advance
      if (step < portalSteps.length - 1) { setStep(s => s + 1); window.scrollTo(0, 0) }
      return
    }
    // Save applicant on first step completion
    if (step === 0 && !applicantId) {
      const id = await saveApplicant()
      if (!id) return
    }
    // Save step field data if present
    const currentPortalStep = portalSteps[step]
    if (currentPortalStep && stepFieldData[currentPortalStep.id] && applicantId) {
      try {
        await api.saveStepData(applicantId, currentPortalStep.id, currentPortalStep.stepType, stepFieldData[currentPortalStep.id])
      } catch {}
    }
    if (step < portalSteps.length - 1) { setStep(s => s + 1); window.scrollTo(0, 0) }
  }

  const handleBack = () => { setStep(s => Math.max(s - 1, 0)); window.scrollTo(0, 0) }
  const formatTime = (secs: number) => `${Math.floor(secs / 60)}:${(secs % 60).toString().padStart(2, '0')}`

  if (loading) return <div className="min-h-screen bg-gray-50 flex items-center justify-center"><Loader2 size={32} className="animate-spin text-gray-400" /></div>
  if (!config) return <div className="min-h-screen bg-gray-50 flex items-center justify-center text-gray-500">Unable to load application form.</div>

  const currentPortalStep = portalSteps[step]

  const updateStepField = (stepId: number, key: string, value: string) => {
    setStepFieldData(prev => ({
      ...prev,
      [stepId]: { ...(prev[stepId] || {}), [key]: value }
    }))
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Company header */}
      <header className="bg-[#0d0c2c] text-white py-4 px-6 flex items-center gap-3">
        <div className="w-8 h-8 rounded-md bg-[#FFD200] flex items-center justify-center">
          <span className="text-[#0d0c2c] font-bold text-sm">U</span>
        </div>
        <span className="font-semibold">{config?.companyName || 'Urgent Couriers'}</span>
      </header>
      <StepIndicator steps={portalSteps.map(s => ({ key: `step-${s.id}`, label: s.title }))} current={step} />

      <div className="max-w-2xl mx-auto py-8 px-4">
        {isPreview && (
          <div className="bg-amber-50 border border-amber-300 text-amber-800 px-4 py-3 rounded-lg mb-4 flex items-center gap-2 text-sm font-medium">
            <Eye size={16} /> Preview Mode — dummy data filled, no records will be created
          </div>
        )}
        {error && <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg mb-4 flex items-center gap-2"><AlertCircle size={18} />{error}</div>}

        {/* Render step based on stepType */}
        {currentPortalStep?.stepType === 'details' && (
          <DetailsStep form={form} update={update} onNext={handleNext} saving={saving}
            title={currentPortalStep.title} description={currentPortalStep.description} />
        )}

        {currentPortalStep?.stepType === 'vehicle' && (
          <VehicleStep form={form} update={update} onNext={handleNext} onBack={handleBack}
            title={currentPortalStep.title} description={currentPortalStep.description}
            vehiclePhotos={vehiclePhotos} onVehiclePhoto={(key: string, file: File) => setVehiclePhotos(p => ({ ...p, [key]: file }))} />
        )}

        {currentPortalStep?.stepType === 'driver_license' && (
          <DriverLicenseStep
            title={currentPortalStep.title} description={currentPortalStep.description}
            isRequired={currentPortalStep.isRequired}
            form={stepFieldData[currentPortalStep.id] || {}}
            update={(field, value) => updateStepField(currentPortalStep.id, field, value)}
            files={files} onFile={(key, f) => setFiles(prev => ({ ...prev, [key]: f }))}
            uploading={uploading === 'license_front'}
            onNext={handleNext} onBack={handleBack}
            onUpload={currentPortalStep.documentTypeId ? (f) => handleUpload(currentPortalStep.documentTypeId!, f, 'license_front') : undefined}
          />
        )}

        {['vehicle_registration', 'vehicle_insurance', 'wof_certificate', 'tsl_certificate'].includes(currentPortalStep?.stepType || '') && (
          <DocumentUploadStep
            stepType={currentPortalStep.stepType}
            title={currentPortalStep.title} description={currentPortalStep.description}
            isRequired={currentPortalStep.isRequired}
            file={files[`doc-${currentPortalStep.documentTypeId}`] || null}
            uploading={uploading === `doc-${currentPortalStep.documentTypeId}`}
            uploaded={currentPortalStep.documentTypeId ? uploadedDocs.has(currentPortalStep.documentTypeId) : false}
            onFile={f => currentPortalStep.documentTypeId && handleUpload(currentPortalStep.documentTypeId, f)}
            fields={stepFieldData[currentPortalStep.id] || {}}
            updateField={(key, value) => updateStepField(currentPortalStep.id, key, value)}
            onNext={handleNext} onBack={handleBack}
          />
        )}

        {currentPortalStep?.stepType === 'generic_document' && (
          <GenericDocumentStep
            title={currentPortalStep.title} description={currentPortalStep.description}
            isRequired={currentPortalStep.isRequired}
            file={files[`step-${currentPortalStep.id}`] || null}
            uploading={uploading === `step-${currentPortalStep.id}`}
            uploaded={false}
            onFile={f => {
              setFiles(prev => ({ ...prev, [`step-${currentPortalStep.id}`]: f }))
              if (currentPortalStep.documentTypeId) handleUpload(currentPortalStep.documentTypeId, f, `step-${currentPortalStep.id}`)
            }}
            onNext={handleNext} onBack={handleBack}
          />
        )}

        {currentPortalStep?.stepType === 'quiz' && config.quiz && (
          <div className="bg-white rounded-2xl p-8 shadow-sm border border-gray-100">
            <h2 className="text-2xl font-bold text-[#0d0c2c] mb-2">{currentPortalStep.title}</h2>

            {quizResult ? (
              <div className="text-center py-8">
                {quizResult.passed ? <CheckCircle size={48} className="mx-auto mb-3 text-green-500" /> : <XCircle size={48} className="mx-auto mb-3 text-red-500" />}
                <h3 className="text-2xl font-bold">{quizResult.passed ? 'Passed!' : 'Not Passed'}</h3>
                <p className="text-gray-600 mt-2">Score: {quizResult.score} ({quizResult.totalQuestions} questions)</p>
                <div className="flex gap-4 mt-8">
                  <button onClick={handleBack} className="flex-1 border border-gray-300 py-3.5 rounded-xl font-medium hover:bg-gray-50">← Back</button>
                  <button onClick={handleNext} className="flex-1 bg-[#0d0c2c] text-white py-3.5 rounded-xl font-semibold hover:bg-[#1a1950]">Next <ChevronRight size={18} className="inline" /></button>
                </div>
              </div>
            ) : !quizStarted ? (
              <div className="text-center py-8">
                <h3 className="text-lg font-semibold">{config.quiz.title}</h3>
                {config.quiz.description && <p className="text-gray-500 mt-2">{config.quiz.description}</p>}
                <div className="flex justify-center gap-6 mt-4 text-sm text-gray-600">
                  <span>{config.quiz.questions?.length || 0} questions</span>
                  <span>Pass: {config.quiz.passingScore}%</span>
                  {config.quiz.timeLimit && <span>Time: {config.quiz.timeLimit} min</span>}
                </div>
                <button onClick={() => { setQuizStarted(new Date()); if (config.quiz?.timeLimit) setTimeLeft(config.quiz.timeLimit * 60) }}
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
                  {config.quiz.questions?.map((q, i) => {
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
                  <span className="text-sm text-gray-500">{Object.keys(answers).length} / {config.quiz.questions?.length || 0} answered</span>
                  <button onClick={handleSubmitQuiz} disabled={submittingQuiz || Object.keys(answers).length === 0}
                    className="bg-[#FFD200] text-[#0d0c2c] px-8 py-3 rounded-xl font-semibold hover:bg-[#E87C1E] hover:text-white disabled:opacity-50 transition-colors">
                    {submittingQuiz ? 'Submitting...' : 'Submit Quiz'}
                  </button>
                </div>
              </div>
            )}
          </div>
        )}

        {currentPortalStep?.stepType === 'review' && !submitted && (
          <ReviewStep form={form} steps={portalSteps} files={files} uploadedDocs={uploadedDocs}
            stepFieldData={stepFieldData} onBack={handleBack} onSubmit={() => setSubmitted(true)}
            title={currentPortalStep.title} description={currentPortalStep.description}
            isPreview={isPreview} />
        )}

        {/* Confirmation */}
        {submitted && (
          <div className="bg-white rounded-2xl p-8 shadow-sm border border-gray-100 text-center">
            <div className="w-20 h-20 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-6">
              <CheckCircle size={40} className="text-green-600" />
            </div>
            <h2 className="text-2xl font-bold mb-2">Application Submitted!</h2>
            <p className="text-gray-600 mb-6">Thank you, <strong>{form.firstName}</strong>. We'll review your application and be in touch.</p>
            <div className="bg-gray-50 rounded-xl p-5 text-left max-w-sm mx-auto mb-6">
              <h3 className="font-semibold text-sm mb-3">What happens next?</h3>
              <ol className="text-sm text-gray-600 space-y-2">
                <li className="flex items-start gap-2"><span className="bg-[#FFD200] text-[#0d0c2c] rounded-full w-6 h-6 flex items-center justify-center text-xs font-bold shrink-0">1</span> We review your details & documents</li>
                <li className="flex items-start gap-2"><span className="bg-[#FFD200] text-[#0d0c2c] rounded-full w-6 h-6 flex items-center justify-center text-xs font-bold shrink-0">2</span> Document verification</li>
                <li className="flex items-start gap-2"><span className="bg-[#FFD200] text-[#0d0c2c] rounded-full w-6 h-6 flex items-center justify-center text-xs font-bold shrink-0">3</span> We'll contact you for next steps</li>
              </ol>
            </div>
            <p className="text-sm text-gray-500">Check your status: <a href={`/apply/status?email=${form.email}`} className="text-[#E87C1E] underline font-medium">Check Status</a></p>
          </div>
        )}
      </div>
    </div>
  )
}

// ─── Status Check ────────────────────────────────────────
function StatusCheck() {
  const [email, setEmail] = useState('')
  const [applicant, setApplicant] = useState<api.Applicant | null>(null)
  const [error, setError] = useState('')
  const [docs, setDocs] = useState<api.ApplicantDocument[]>([])
  const [loading, setLoading] = useState(false)

  const check = async () => {
    setLoading(true); setError('')
    try {
      const res = await api.getApplicantByEmail(email)
      setApplicant(res)
      if (res.id) { const d = await api.getApplicantDocuments(res.id); setDocs(d) }
    } catch { setError('No application found for this email address.') }
    finally { setLoading(false) }
  }

  const statusColors: Record<string, string> = {
    Applied: 'bg-blue-100 text-blue-800', Screening: 'bg-yellow-100 text-yellow-800',
    Interview: 'bg-purple-100 text-purple-800', DocumentCheck: 'bg-orange-100 text-orange-800',
    QuizPending: 'bg-indigo-100 text-indigo-800', Approved: 'bg-green-100 text-green-800', Rejected: 'bg-red-100 text-red-800',
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-[#0d0c2c] text-white py-4 px-6 flex items-center gap-3">
        <div className="w-8 h-8 rounded-md bg-[#FFD200] flex items-center justify-center">
          <span className="text-[#0d0c2c] font-bold text-sm">U</span>
        </div>
        <span className="font-semibold">Urgent Couriers</span>
      </header>
      <div className="max-w-xl mx-auto py-8 px-4">
        <h2 className="text-xl font-semibold mb-4">Check Application Status</h2>
        <div className="flex gap-2 mb-6">
          <input className="flex-1 border rounded-lg p-3" placeholder="Enter your email address" type="email"
            value={email} onChange={e => setEmail(e.target.value)} onKeyDown={e => e.key === 'Enter' && check()} />
          <button onClick={check} disabled={loading || !email}
            className="bg-[#FFD200] text-[#0d0c2c] px-6 rounded-lg font-semibold hover:bg-[#E87C1E] hover:text-white disabled:opacity-50 transition-colors">
            {loading ? '...' : 'Check'}
          </button>
        </div>
        {error && <p className="text-red-500 bg-red-50 p-3 rounded-lg">{error}</p>}
        {applicant && (
          <div className="space-y-4">
            <div className="bg-white rounded-xl p-6 shadow-sm">
              <div className="flex items-start justify-between">
                <div><h3 className="font-bold text-lg">{applicant.firstName} {applicant.lastName}</h3><p className="text-gray-600">{applicant.email}</p></div>
                <span className={`px-3 py-1 rounded-full text-sm font-semibold ${statusColors[applicant.status] || 'bg-gray-100'}`}>{applicant.status}</span>
              </div>
              <p className="text-sm text-gray-500 mt-3">Applied: {new Date(applicant.appliedDate).toLocaleDateString('en-NZ')}</p>
            </div>
            {docs.length > 0 && (
              <div className="bg-white rounded-xl p-6 shadow-sm">
                <h3 className="font-semibold mb-3">Documents</h3>
                {docs.map(d => (
                  <div key={d.id} className="flex items-center gap-3 text-sm p-2 rounded-lg bg-gray-50 mb-2">
                    {d.status === 'Verified' ? <CheckCircle size={14} className="text-green-600" /> : d.status === 'Rejected' ? <XCircle size={14} className="text-red-600" /> : <Clock size={14} className="text-yellow-600" />}
                    <span className="font-medium">{d.documentType?.name || 'Document'}</span>
                    <span className={`ml-auto text-xs px-2 py-0.5 rounded-full ${d.status === 'Verified' ? 'bg-green-100 text-green-700' : d.status === 'Rejected' ? 'bg-red-100 text-red-700' : 'bg-yellow-100 text-yellow-700'}`}>{d.status}</span>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  )
}

// ─── Landing Page ────────────────────────────────────────
function LandingPage() {
  const navigate = useNavigate()
  const [config, setConfig] = useState<api.PortalConfig | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => { api.getPortalConfig().then(setConfig).finally(() => setLoading(false)) }, [])

  if (loading) return <div className="min-h-screen bg-gray-50 flex items-center justify-center"><Loader2 size={32} className="animate-spin text-gray-400" /></div>

  // Build flow steps from config.steps
  const flowSteps = (config?.steps || []).filter(s => s.stepType !== 'review').map(s => {
    const type = getStepType(s.stepType)
    return { label: s.title, description: s.description, required: s.isRequired, icon: type?.icon || '📄' }
  })

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-[#0d0c2c] text-white py-4 px-6 flex items-center gap-3">
        <div className="w-8 h-8 rounded-md bg-[#FFD200] flex items-center justify-center">
          <span className="text-[#0d0c2c] font-bold text-sm">U</span>
        </div>
        <span className="font-semibold">{config?.companyName || 'Urgent Couriers'}</span>
      </header>
      <div className="max-w-2xl mx-auto py-8 px-4">
        <div className="bg-[#0d0c2c] rounded-2xl p-8 mb-8">
          <h1 className="text-3xl font-bold text-white mb-2">{config?.welcomeTitle || 'Join Our Team'}</h1>
          <p className="text-gray-400">{config?.welcomeSubtitle || "We're looking for reliable courier drivers across New Zealand."}</p>
        </div>

        {flowSteps.length > 0 && (
          <div className="mb-8">
            <h2 className="text-lg font-semibold text-[#0d0c2c] mb-4">What you'll need to apply</h2>
            <div className="grid grid-cols-2 gap-3">
              {flowSteps.map((step, i) => (
                <div key={i} className="bg-white rounded-xl p-4 border border-gray-100 flex items-start gap-3">
                  <div className="w-10 h-10 rounded-lg bg-gray-100 flex items-center justify-center shrink-0 text-lg">{step.icon}</div>
                  <div className="min-w-0">
                    <div className="flex items-center gap-2 flex-wrap">
                      <span className="font-semibold text-sm text-[#0d0c2c]">{step.label}</span>
                      {step.required && <span className="text-xs text-red-500 font-medium">Required</span>}
                    </div>
                    {step.description && <p className="text-xs text-gray-500 mt-0.5">{step.description}</p>}
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        <div className="bg-green-50 border border-green-100 rounded-xl p-4 flex items-start gap-3 mb-6">
          <div className="w-10 h-10 rounded-lg bg-green-100 flex items-center justify-center shrink-0"><Clock size={20} className="text-green-600" /></div>
          <div><p className="font-semibold text-sm text-[#0d0c2c]">Quick & Easy — About {(config?.steps || []).length} steps</p><p className="text-xs text-green-700 mt-0.5">Take photos of your documents with your phone camera. Upload them directly.</p></div>
        </div>

        <button onClick={() => navigate('/apply/form')}
          className="w-full sm:w-auto bg-[#FFD200] text-[#0d0c2c] px-8 py-3.5 rounded-xl font-bold text-lg hover:bg-[#E87C1E] hover:text-white transition-colors shadow-sm">
          Start Application →
        </button>
        <p className="text-sm text-gray-400 mt-4">Already applied? <a href="/apply/status" className="text-[#E87C1E] underline font-medium">Check your status</a></p>
      </div>
    </div>
  )
}

// ─── Routes ──────────────────────────────────────────────
export default function ApplicantPortal() {
  return (
    <Routes>
      <Route index element={<LandingPage />} />
      <Route path="form" element={<ApplyFlow />} />
      <Route path="status" element={<StatusCheck />} />
      <Route path="status/:email" element={<StatusCheck />} />
    </Routes>
  )
}
