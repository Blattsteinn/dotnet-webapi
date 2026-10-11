// Prices arrive as plain numbers with no currency information in the API, so
// they are rendered as fixed-point amounts without inventing a symbol.
export function formatPrice(value) {
  const number = Number(value)
  return Number.isFinite(number) ? number.toFixed(2) : '—'
}

export function formatDateTime(value) {
  if (!value) return '—'
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? '—' : date.toLocaleString()
}

// Order line total: (product price + quantity × each add-on price) × quantity.
export function lineTotal(item) {
  const addOns = (item.orderItemAddOns ?? []).reduce(
    (sum, addOn) => sum + Number(addOn.price) * addOn.quantity,
    0,
  )
  return (Number(item.price) + addOns) * item.quantity
}

export function orderTotal(order) {
  return (order?.orderItems ?? []).reduce((sum, item) => sum + lineTotal(item), 0)
}

export function orderItemCount(order) {
  return (order?.orderItems ?? []).reduce((sum, item) => sum + item.quantity, 0)
}
