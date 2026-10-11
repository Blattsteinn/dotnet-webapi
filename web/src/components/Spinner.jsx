export function Spinner({ className = '' }) {
  return (
    <span
      role="status"
      aria-label="Loading"
      className={`inline-block h-5 w-5 animate-spin rounded-full border-2 border-slate-300 border-t-slate-700 ${className}`}
    />
  )
}

export function LoadingBlock({ label = 'Loading…' }) {
  return (
    <div className="flex items-center gap-3 p-4 text-sm text-slate-600">
      <Spinner />
      <span>{label}</span>
    </div>
  )
}
