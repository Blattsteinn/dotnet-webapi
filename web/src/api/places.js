import { api } from './client.js'

// GET /api/v1/places → PlaceSummaryDto[] (hidden places excluded, isOpen computed)
export function listPlaces() {
  return api.get('/api/v1/places')
}

// GET /api/v1/places/{id} → PublicPlaceDto (404 when hidden)
export function getPlace(id) {
  return api.get(`/api/v1/places/${id}`)
}

// GET /api/v1/product-categories/{categoryId}/products → ProductSummaryDto[]
export function listCategoryProducts(categoryId) {
  return api.get(`/api/v1/product-categories/${categoryId}/products`)
}

// GET /api/v1/product-categories/{categoryId}/products/{id} → GetProductDto,
// the only response carrying the add-on tree. Hidden products/categories 404.
export function getProduct(categoryId, productId) {
  return api.get(`/api/v1/product-categories/${categoryId}/products/${productId}`)
}
