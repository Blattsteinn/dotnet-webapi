// The API serialises OrderStatus as its numeric value (Pending = 0 … Cancelled
// = 5) in `status` fields and accepts the same number in PATCH bodies.
export const OrderStatus = {
  Pending: 0,
  Confirmed: 1,
  Preparing: 2,
  Ready: 3,
  Completed: 4,
  Cancelled: 5,
}

const LABELS = {
  [OrderStatus.Pending]: 'Pending',
  [OrderStatus.Confirmed]: 'Confirmed',
  [OrderStatus.Preparing]: 'Preparing',
  [OrderStatus.Ready]: 'Ready',
  [OrderStatus.Completed]: 'Completed',
  [OrderStatus.Cancelled]: 'Cancelled',
}

// Mirrors OrderService.IsTransitionAllowed: one forward step, or Cancelled from
// any non-terminal state. The server still rejects anything illegal with 409.
const NEXT = {
  [OrderStatus.Pending]: OrderStatus.Confirmed,
  [OrderStatus.Confirmed]: OrderStatus.Preparing,
  [OrderStatus.Preparing]: OrderStatus.Ready,
  [OrderStatus.Ready]: OrderStatus.Completed,
}

const NEXT_ACTIONS = {
  [OrderStatus.Pending]: 'Confirm',
  [OrderStatus.Confirmed]: 'Start preparing',
  [OrderStatus.Preparing]: 'Mark as ready',
  [OrderStatus.Ready]: 'Mark as completed',
}

export function statusLabel(status) {
  return LABELS[status] ?? `Unknown (${status})`
}

export function nextStatus(status) {
  return NEXT[status] ?? null
}

export function nextStatusAction(status) {
  return NEXT_ACTIONS[status] ?? null
}

export function isTerminal(status) {
  return status === OrderStatus.Completed || status === OrderStatus.Cancelled
}

// Cancelling is allowed until the order is completed.
export function isCancellable(status) {
  return !isTerminal(status)
}

export const STATUS_BADGE_CLASSES = {
  [OrderStatus.Pending]: 'bg-amber-100 text-amber-800 ring-amber-200',
  [OrderStatus.Confirmed]: 'bg-sky-100 text-sky-800 ring-sky-200',
  [OrderStatus.Preparing]: 'bg-indigo-100 text-indigo-800 ring-indigo-200',
  [OrderStatus.Ready]: 'bg-emerald-100 text-emerald-800 ring-emerald-200',
  [OrderStatus.Completed]: 'bg-slate-100 text-slate-700 ring-slate-200',
  [OrderStatus.Cancelled]: 'bg-rose-100 text-rose-800 ring-rose-200',
}
