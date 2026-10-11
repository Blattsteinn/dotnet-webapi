import { api } from './client.js'

// GET /api/v1/admin/users?search= → AdminUserDto[] ({ id, email } only)
export function searchUsers(search) {
  return api.get(`/api/v1/admin/users?search=${encodeURIComponent(search ?? '')}`)
}

// POST /api/v1/admin/users/{userId}/roles → grants Owner ("Owner" is the only
// role this endpoint accepts; anything else is a 400).
export function grantOwner(userId) {
  return api.post(`/api/v1/admin/users/${userId}/roles`, { role: 'Owner' })
}

// DELETE /api/v1/admin/users/{userId}/roles/Owner → revokes Owner; the user's
// places are hidden and their manager grants cleared. 400 if they lack it.
export function revokeOwner(userId) {
  return api.del(`/api/v1/admin/users/${userId}/roles/Owner`)
}
