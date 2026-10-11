import { api } from './client.js'

// GET /api/v1/manager/places → OwnedPlaceDto[]: the places the caller owns or
// manages. "Manager" is not a role — access is per place.
export function listManagedPlaces() {
  return api.get('/api/v1/manager/places')
}

// GET /api/v1/manager/places/{placeId}/orders
export function listPlaceOrders(placeId) {
  return api.get(`/api/v1/manager/places/${placeId}/orders`)
}

// GET /api/v1/manager/places/{placeId}/orders/{orderId}
export function getPlaceOrder(placeId, orderId) {
  return api.get(`/api/v1/manager/places/${placeId}/orders/${orderId}`)
}

// PATCH /api/v1/manager/places/{placeId}/orders/{orderId}/status
// The status is sent as the numeric OrderStatus the API emits; an illegal or
// replayed transition comes back as 409 Problem Details.
export function setOrderStatus(placeId, orderId, status) {
  return api.patch(`/api/v1/manager/places/${placeId}/orders/${orderId}/status`, {
    status,
  })
}
