import { statusLabel, STATUS_BADGE_CLASSES } from '../lib/orderStatus.js'

export function Badge({ className = '', children }) {
  return (
    <span
      className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ring-1 ring-inset ${className}`}
    >
      {children}
    </span>
  )
}

// Open/closed comes straight from the server's computed `isOpen` — the browser
// never evaluates opening hours itself.
export function OpenBadge({ isOpen, acceptingOrders = true }) {
  if (!acceptingOrders) {
    return <Badge className="bg-rose-100 text-rose-800 ring-rose-200">Closed</Badge>
  }
  return isOpen ? (
    <Badge className="bg-emerald-100 text-emerald-800 ring-emerald-200">Open</Badge>
  ) : (
    <Badge className="bg-slate-100 text-slate-700 ring-slate-200">Closed</Badge>
  )
}

export function StatusBadge({ status }) {
  return (
    <Badge className={STATUS_BADGE_CLASSES[status] ?? 'bg-slate-100 text-slate-700 ring-slate-200'}>
      {statusLabel(status)}
    </Badge>
  )
}

export function HiddenBadge({ hidden }) {
  if (!hidden) return null
  return (
    <Badge className="bg-purple-100 text-purple-800 ring-purple-200">Hidden</Badge>
  )
}
