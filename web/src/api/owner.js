import { api } from './client.js'

// GET /api/v1/owner/places → OwnedPlaceDto[]
export function listOwnedPlaces() {
  return api.get('/api/v1/owner/places')
}

// POST /api/v1/owner/places → OwnedPlaceDto (201). The ownership row is written
// server-side from the caller's id, so no owner field is sent.
export function createOwnedPlace({ name, latitude, longtitude, description, address }) {
  return api.post('/api/v1/owner/places', {
    name,
    latitude,
    longtitude,
    description,
    address,
  })
}

// GET /api/v1/owner/places/{id} → PrivatePlaceDto
export function getOwnedPlace(id) {
  return api.get(`/api/v1/owner/places/${id}`)
}

// PATCH /api/v1/owner/places/{id} → PrivatePlaceDto
//
// Full-replace payload: the place scalars plus the whole category → product →
// add-on graph. A node is deleted only when it is sent with destroy: true and a
// non-zero id, so anything left out of the payload survives untouched.
export function updateOwnedPlace(id, payload) {
  return api.patch(`/api/v1/owner/places/${id}`, payload)
}

// PUT /api/v1/owner/places/{placeId}/hours → { placeId, openingHours }
// Full replace of the weekly schedule; an empty list clears it (place closed).
export function replaceOpeningHours(placeId, hours) {
  return api.put(`/api/v1/owner/places/${placeId}/hours`, { hours })
}

// GET /api/v1/owner/places/{placeId}/managers → ManagerDto[]
export function listManagers(placeId) {
  return api.get(`/api/v1/owner/places/${placeId}/managers`)
}

// PATCH /api/v1/owner/places/{placeId}/managers/roles — assigns by email
export function addManager(placeId, email) {
  return api.patch(`/api/v1/owner/places/${placeId}/managers/roles`, { email })
}

// DELETE /api/v1/owner/places/{placeId}/managers/{userId}/roles — removes by id
export function removeManager(placeId, userId) {
  return api.del(`/api/v1/owner/places/${placeId}/managers/${userId}/roles`)
}
