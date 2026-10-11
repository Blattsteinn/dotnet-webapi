import { api } from './client.js'

// POST /api/v1/orders → GetOrderDto (201). 409 when the place is closed.
export function createOrder({ placeId, orderItems }) {
  return api.post('/api/v1/orders', { placeId, orderItems })
}

// GET /api/v1/orders → the caller's own orders
export function listMyOrders() {
  return api.get('/api/v1/orders')
}

// GET /api/v1/orders/{orderId} → 404 when missing or not the caller's
export function getMyOrder(orderId) {
  return api.get(`/api/v1/orders/${orderId}`)
}

// POST /api/v1/orders/{orderId}/cancel → 200 while Pending, 409 once it is not
export function cancelMyOrder(orderId) {
  return api.post(`/api/v1/orders/${orderId}/cancel`)
}
