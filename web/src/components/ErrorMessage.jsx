import { ApiError } from '../api/client.js'

// Every failure the API produces is Problem Details, so the display rule is
// always "detail ?? title", plus the per-field validation messages. Anything
// else (including a network failure) gets its generic ApiError message.
export function ErrorMessage({ error, className = '' }) {
  if (!error) return null

  const message =
    error instanceof ApiError
      ? error.message
      : error.message || String(error)

  const fieldErrors = error instanceof ApiError ? error.fieldErrors : []
  const status = error instanceof ApiError && error.status ? ` (${error.status})` : ''

  return (
    <div
      role="alert"
      className={`rounded-md border border-rose-200 bg-rose-50 p-3 text-sm text-rose-800 ${className}`}
    >
      <p className="font-medium">
        {message}
        {status}
      </p>
      {fieldErrors.length > 0 && (
        <ul className="mt-2 list-inside list-disc space-y-0.5">
          {fieldErrors.map(({ field, message: text }, index) => (
            <li key={`${field}-${index}`}>
              <span className="font-medium">{field}:</span> {text}
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}

export function SuccessMessage({ children, className = '' }) {
  if (!children) return null
  return (
    <div
      role="status"
      className={`rounded-md border border-emerald-200 bg-emerald-50 p-3 text-sm text-emerald-800 ${className}`}
    >
      {children}
    </div>
  )
}

// Amber, for things the user should know about but that are not failures (for
// example "your session expired, sign in again").
export function NoticeMessage({ children, className = '' }) {
  if (!children) return null
  return (
    <div
      role="status"
      className={`rounded-md border border-amber-200 bg-amber-50 p-3 text-sm text-amber-900 ${className}`}
    >
      {children}
    </div>
  )
}
