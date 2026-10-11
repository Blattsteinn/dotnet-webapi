// Shared form primitives. Field owns the label + error layout so every input on
// every screen reads the same.
export function Field({ label, htmlFor, error, hint, children, className = '' }) {
  return (
    <div className={`flex flex-col gap-1 ${className}`}>
      {label && (
        <label htmlFor={htmlFor} className="text-sm font-medium text-slate-700">
          {label}
        </label>
      )}
      {children}
      {hint && <p className="text-xs text-slate-500">{hint}</p>}
      {error && <p className="text-xs text-rose-700">{error}</p>}
    </div>
  )
}

const CONTROL_CLASSES =
  'w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 shadow-sm placeholder:text-slate-400 focus:border-slate-500 focus:outline-none focus:ring-1 focus:ring-slate-500 disabled:bg-slate-50 disabled:text-slate-500'

export function Input({ label, id, error, hint, className = '', ...props }) {
  return (
    <Field label={label} htmlFor={id} error={error} hint={hint} className={className}>
      <input id={id} className={CONTROL_CLASSES} {...props} />
    </Field>
  )
}

export function Textarea({ label, id, error, hint, rows = 3, className = '', ...props }) {
  return (
    <Field label={label} htmlFor={id} error={error} hint={hint} className={className}>
      <textarea id={id} rows={rows} className={CONTROL_CLASSES} {...props} />
    </Field>
  )
}

export function Select({ label, id, error, hint, className = '', children, ...props }) {
  return (
    <Field label={label} htmlFor={id} error={error} hint={hint} className={className}>
      <select id={id} className={CONTROL_CLASSES} {...props}>
        {children}
      </select>
    </Field>
  )
}

export function Checkbox({ label, id, className = '', ...props }) {
  return (
    <label
      htmlFor={id}
      className={`flex items-center gap-2 text-sm text-slate-700 ${className}`}
    >
      <input
        id={id}
        type="checkbox"
        className="h-4 w-4 rounded border-slate-300 text-slate-900 focus:ring-slate-500"
        {...props}
      />
      {label}
    </label>
  )
}
