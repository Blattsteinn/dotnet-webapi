// Helpers for the owner's place editor, which edits the whole
// category → product → add-on-category → add-on graph in one payload.
//
// The API's PATCH is NOT a plain full replace: a node is deleted only when it is
// sent with `destroy: true` and a non-zero id, and an array left empty leaves the
// stored children untouched. So deletions are marked in place, new nodes carry
// id 0, and every node the owner can still see is sent in full.

export const newKey = () =>
  typeof crypto !== 'undefined' && crypto.randomUUID
    ? crypto.randomUUID()
    : `k${Math.random().toString(36).slice(2)}`

export function mapNode(list, key, fn) {
  return list.map((node) => (node.key === key ? fn(node) : node))
}

// New (unsaved) nodes disappear; persisted ones are marked for deletion.
export function deleteNode(list, key) {
  return list
    .map((node) =>
      node.key === key ? (node.id === 0 ? null : { ...node, destroy: true }) : node,
    )
    .filter(Boolean)
}

export function mapCategory(categories, categoryKey, fn) {
  return mapNode(categories, categoryKey, fn)
}

export function mapProduct(categories, categoryKey, productKey, fn) {
  return mapCategory(categories, categoryKey, (category) => ({
    ...category,
    products: mapNode(category.products, productKey, fn),
  }))
}

export function mapAddOnCategory(categories, categoryKey, productKey, addOnCategoryKey, fn) {
  return mapProduct(categories, categoryKey, productKey, (product) => ({
    ...product,
    addOnCategories: mapNode(product.addOnCategories, addOnCategoryKey, fn),
  }))
}

export function mapAddOn(
  categories,
  categoryKey,
  productKey,
  addOnCategoryKey,
  addOnKey,
  fn,
) {
  return mapAddOnCategory(
    categories,
    categoryKey,
    productKey,
    addOnCategoryKey,
    (addOnCategory) => ({
      ...addOnCategory,
      addOns: mapNode(addOnCategory.addOns, addOnKey, fn),
    }),
  )
}

function categoryToPayload(category) {
  return {
    id: category.id,
    destroy: Boolean(category.destroy),
    name: category.name,
    description: category.description || null,
    hidden: Boolean(category.hidden),
    products: category.products.map(productToPayload),
  }
}

function productToPayload(product) {
  return {
    id: product.id,
    destroy: Boolean(product.destroy),
    name: product.name,
    description: product.description || null,
    hidden: Boolean(product.hidden),
    price: Number(product.price),
    // A hidden category/product cannot be read back through the public product
    // endpoint, so its add-ons are unknown. Sending an empty list leaves them
    // exactly as they are on the server.
    addOnCategories: product.addOnsUnavailable
      ? []
      : product.addOnCategories.map(addOnCategoryToPayload),
  }
}

function addOnCategoryToPayload(addOnCategory) {
  return {
    id: addOnCategory.id,
    destroy: Boolean(addOnCategory.destroy),
    name: addOnCategory.name,
    required: Boolean(addOnCategory.required),
    multichoice: Boolean(addOnCategory.multichoice),
    addOns: addOnCategory.addOns.map((addOn) => ({
      id: addOn.id,
      destroy: Boolean(addOn.destroy),
      name: addOn.name,
      price: Number(addOn.price),
      hidden: Boolean(addOn.hidden),
    })),
  }
}

export function toUpdatePayload(place) {
  return {
    name: place.name,
    latitude: Number(place.latitude),
    longtitude: Number(place.longtitude),
    description: place.description || null,
    address: place.address || null,
    hidden: Boolean(place.hidden),
    acceptingOrders: Boolean(place.acceptingOrders),
    productCategories: place.productCategories.map(categoryToPayload),
  }
}

// Turns the GET /owner/places/{id} response into editor state. Add-ons are not
// part of that response (PrivatePlaceDto stops at the product), so they are
// fetched per product; `unavailable` records a product whose add-ons could not
// be read because it — or its category — is hidden.
export function toEditorCategories(place, addOnsByProductId) {
  return place.productCategories.map((category) => ({
    key: newKey(),
    id: category.id,
    destroy: false,
    name: category.name,
    description: category.description ?? '',
    hidden: category.hidden,
    products: category.products.map((product) => {
      const addOns = addOnsByProductId.get(product.id)
      return {
        key: newKey(),
        id: product.id,
        destroy: false,
        name: product.name,
        description: product.description ?? '',
        hidden: product.hidden,
        price: product.price,
        addOnsUnavailable: addOns?.unavailable ?? false,
        addOnCategories: (addOns?.addOnCategories ?? []).map((addOnCategory) => ({
          key: newKey(),
          id: addOnCategory.id,
          destroy: false,
          name: addOnCategory.name,
          required: addOnCategory.required,
          multichoice: addOnCategory.multichoice,
          addOns: addOnCategory.addOns.map((addOn) => ({
            key: newKey(),
            id: addOn.id,
            destroy: false,
            name: addOn.name,
            price: addOn.price,
            hidden: false,
          })),
        })),
      }
    }),
  }))
}
