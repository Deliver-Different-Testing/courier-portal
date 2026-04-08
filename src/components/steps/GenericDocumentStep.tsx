import { NavButtons, StepWrapper, UploadZone } from './shared'

interface Props {
  title: string; description?: string; isRequired: boolean
  file: File | null; uploading: boolean; uploaded: boolean
  onFile: (f: File) => void
  onNext: () => void; onBack: () => void
}

export default function GenericDocumentStep({ title, description, isRequired, file, uploading, uploaded, onFile, onNext, onBack }: Props) {
  return (
    <StepWrapper title={title} description={description} isRequired={isRequired}>
      <UploadZone
        label={`📷 Upload ${title}`}
        description="Take a photo or upload a file"
        file={file}
        uploading={uploading}
        onFile={onFile}
      />
      <NavButtons onBack={onBack} onNext={onNext}
        disabled={isRequired && !uploaded && !file}
        onSkip={!isRequired ? onNext : undefined} />
    </StepWrapper>
  )
}
