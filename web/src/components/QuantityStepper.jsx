export function QuantityStepper({
  value,
  onChange,
  min = 1,
  max = 99,
  label = 'Quantity',
  size = 'md',
}) {
  const buttonClasses =
    size === 'sm'
      ? 'h-6 w-6 text-xs'
      : 'h-8 w-8 text-sm'

  function step(delta) {
    const next = Math.min(max, Math.max(min, value + delta))
    if (next !== value) onChange(next)
  }

  return (
    <div className="inline-flex items-center gap-1" role="group" aria-label={label}>
      <button
        type="button"
        onClick={() => step(-1)}
        disabled={value <= min}
        aria-label={`Decrease ${label.toLowerCase()}`}
        className={`${buttonClasses} rounded border border-slate-300 bg-white font-medium text-slate-700 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-40`}
      >
        −
      </button>
      <span className={`${size === 'sm' ? 'w-6' : 'w-8'} text-center text-sm tabular-nums`}>
        {value}
      </span>
      <button
        type="button"
        onClick={() => step(1)}
        disabled={value >= max}
        aria-label={`Increase ${label.toLowerCase()}`}
        className={`${buttonClasses} rounded border border-slate-300 bg-white font-medium text-slate-700 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-40`}
      >
        +
      </button>
    </div>
  )
}
