interface Props {
  className?: string;
}

export default function TierBadge({ className = '' }: Props) {
  return (
    <span className={`inline-block bg-brand-cyan/10 text-brand-cyan text-xs px-2 py-0.5 rounded-full ${className}`}>
      Base NP
    </span>
  );
}
