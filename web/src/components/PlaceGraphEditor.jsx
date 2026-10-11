import { Button } from './Button.jsx'
import { Checkbox, Input, Textarea } from './Field.jsx'
import {
  deleteNode,
  mapAddOn,
  mapAddOnCategory,
  mapCategory,
  mapProduct,
  newKey,
} from '../lib/placeGraph.js'

// Editor for the place's full product graph. It keeps the same nested shape the
// PATCH payload needs, so saving can send exactly what is on screen. Nodes the
// owner removed are kept in state with destroy: true (the API needs both the id
// and that flag); nodes created here carry id 0 until the server assigns one.
export function PlaceGraphEditor({ categories, setCategories }) {
  function addCategory() {
    setCategories((current) => [
      ...current,
      {
        key: newKey(),
        id: 0,
        destroy: false,
        name: '',
        description: '',
        hidden: false,
        products: [],
      },
    ])
  }

  function addProduct(categoryKey) {
    setCategories((current) =>
      mapCategory(current, categoryKey, (category) => ({
        ...category,
        products: [
          ...category.products,
          {
            key: newKey(),
            id: 0,
            destroy: false,
            name: '',
            description: '',
            hidden: false,
            price: 0,
            addOnsUnavailable: false,
            addOnCategories: [],
          },
        ],
      })),
    )
  }

  function addAddOnCategory(categoryKey, productKey) {
    setCategories((current) =>
      mapProduct(current, categoryKey, productKey, (product) => ({
        ...product,
        addOnCategories: [
          ...product.addOnCategories,
          {
            key: newKey(),
            id: 0,
            destroy: false,
            name: '',
            required: false,
            multichoice: false,
            addOns: [],
          },
        ],
      })),
    )
  }

  function addAddOn(categoryKey, productKey, addOnCategoryKey) {
    setCategories((current) =>
      mapAddOnCategory(current, categoryKey, productKey, addOnCategoryKey, (node) => ({
        ...node,
        addOns: [
          ...node.addOns,
          { key: newKey(), id: 0, destroy: false, name: '', price: 0, hidden: false },
        ],
      })),
    )
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-sm font-semibold tracking-wide text-slate-500 uppercase">
          Menu
        </h2>
        <Button variant="secondary" size="sm" onClick={addCategory}>
          Add category
        </Button>
      </div>

      <p className="rounded-md bg-slate-50 p-3 text-xs text-slate-600">
        Saving sends the whole graph. Deleted items are removed server-side; a
        hidden product (or a product under a hidden category) has its add-ons left
        untouched here, because the API only exposes add-ons for products that are
        publicly visible. Hidden add-ons are not listed either, and stay as they are.
      </p>

      {categories.length === 0 && (
        <p className="text-sm text-slate-600">This place has no categories yet.</p>
      )}

      {categories.map((category) => (
        <div key={category.key} className="rounded-lg border border-slate-200 p-3">
          <div className="grid gap-3 sm:grid-cols-2">
            <Input
              id={`cat-name-${category.key}`}
              label="Category name"
              required
              maxLength={50}
              value={category.name}
              onChange={(event) =>
                setCategories((current) =>
                  mapCategory(current, category.key, (node) => ({
                    ...node,
                    name: event.target.value,
                  })),
                )
              }
            />
            <Input
              id={`cat-description-${category.key}`}
              label="Category description"
              maxLength={100}
              value={category.description ?? ''}
              onChange={(event) =>
                setCategories((current) =>
                  mapCategory(current, category.key, (node) => ({
                    ...node,
                    description: event.target.value,
                  })),
                )
              }
            />
          </div>

          <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
            <Checkbox
              id={`cat-hidden-${category.key}`}
              label="Hidden"
              checked={Boolean(category.hidden)}
              onChange={(event) =>
                setCategories((current) =>
                  mapCategory(current, category.key, (node) => ({
                    ...node,
                    hidden: event.target.checked,
                  })),
                )
              }
            />
            <div className="flex gap-2">
              <Button size="sm" variant="secondary" onClick={() => addProduct(category.key)}>
                Add product
              </Button>
              <Button
                size="sm"
                variant="danger"
                onClick={() => setCategories((current) => deleteNode(current, category.key))}
              >
                Delete category
              </Button>
            </div>
          </div>

          <div className="mt-3 space-y-3">
            {category.products.map((product) => (
              <details
                key={product.key}
                className="rounded-md border border-slate-200 bg-slate-50/50 p-3"
              >
                <summary className="cursor-pointer text-sm font-medium text-slate-800">
                  {product.name || '(unnamed product)'}{' '}
                  <span className="font-normal text-slate-500">
                    · {Number(product.price || 0).toFixed(2)}
                    {product.hidden ? ' · hidden' : ''}
                  </span>
                </summary>

                <div className="mt-3 space-y-3">
                  <div className="grid gap-3 sm:grid-cols-2">
                    <Input
                      id={`prod-name-${product.key}`}
                      label="Product name"
                      required
                      maxLength={200}
                      value={product.name}
                      onChange={(event) =>
                        setCategories((current) =>
                          mapProduct(current, category.key, product.key, (node) => ({
                            ...node,
                            name: event.target.value,
                          })),
                        )
                      }
                    />
                    <Input
                      id={`prod-price-${product.key}`}
                      label="Price"
                      type="number"
                      step="0.01"
                      min="0"
                      max="10000"
                      required
                      value={product.price}
                      onChange={(event) =>
                        setCategories((current) =>
                          mapProduct(current, category.key, product.key, (node) => ({
                            ...node,
                            price: event.target.value,
                          })),
                        )
                      }
                    />
                  </div>

                  <Textarea
                    id={`prod-description-${product.key}`}
                    label="Product description"
                    maxLength={250}
                    rows={2}
                    value={product.description ?? ''}
                    onChange={(event) =>
                      setCategories((current) =>
                        mapProduct(current, category.key, product.key, (node) => ({
                          ...node,
                          description: event.target.value,
                        })),
                      )
                    }
                  />

                  <div className="flex flex-wrap items-center justify-between gap-3">
                    <Checkbox
                      id={`prod-hidden-${product.key}`}
                      label="Hidden"
                      checked={Boolean(product.hidden)}
                      onChange={(event) =>
                        setCategories((current) =>
                          mapProduct(current, category.key, product.key, (node) => ({
                            ...node,
                            hidden: event.target.checked,
                          })),
                        )
                      }
                    />
                    <div className="flex gap-2">
                      <Button
                        size="sm"
                        variant="secondary"
                        onClick={() => addAddOnCategory(category.key, product.key)}
                      >
                        Add add-on group
                      </Button>
                      <Button
                        size="sm"
                        variant="danger"
                        onClick={() =>
                          setCategories((current) =>
                            mapCategory(current, category.key, (node) => ({
                              ...node,
                              products: deleteNode(node.products, product.key),
                            })),
                          )
                        }
                      >
                        Delete product
                      </Button>
                    </div>
                  </div>

                  {product.addOnsUnavailable && (
                    <p className="rounded-md bg-amber-50 p-2 text-xs text-amber-900">
                      This product is hidden, so its add-ons cannot be read back —
                      they will be left as they are on the server.
                    </p>
                  )}

                  <div className="space-y-3">
                    {product.addOnCategories.map((addOnCategory) => (
                      <div
                        key={addOnCategory.key}
                        className="rounded-md border border-slate-200 bg-white p-3"
                      >
                        <div className="grid gap-3 sm:grid-cols-3">
                          <Input
                            id={`aoc-name-${addOnCategory.key}`}
                            label="Group name"
                            required
                            maxLength={200}
                            value={addOnCategory.name}
                            onChange={(event) =>
                              setCategories((current) =>
                                mapAddOnCategory(
                                  current,
                                  category.key,
                                  product.key,
                                  addOnCategory.key,
                                  (node) => ({ ...node, name: event.target.value }),
                                ),
                              )
                            }
                          />
                          <Checkbox
                            id={`aoc-required-${addOnCategory.key}`}
                            label="Required"
                            checked={Boolean(addOnCategory.required)}
                            onChange={(event) =>
                              setCategories((current) =>
                                mapAddOnCategory(
                                  current,
                                  category.key,
                                  product.key,
                                  addOnCategory.key,
                                  (node) => ({ ...node, required: event.target.checked }),
                                ),
                              )
                            }
                          />
                          <Checkbox
                            id={`aoc-multi-${addOnCategory.key}`}
                            label="Multiple choices"
                            checked={Boolean(addOnCategory.multichoice)}
                            onChange={(event) =>
                              setCategories((current) =>
                                mapAddOnCategory(
                                  current,
                                  category.key,
                                  product.key,
                                  addOnCategory.key,
                                  (node) => ({ ...node, multichoice: event.target.checked }),
                                ),
                              )
                            }
                          />
                        </div>

                        <ul className="mt-3 space-y-2">
                          {addOnCategory.addOns.map((addOn) => (
                            <li key={addOn.key} className="flex flex-wrap items-end gap-3">
                              <Input
                                id={`addon-name-${addOn.key}`}
                                label="Add-on"
                                required
                                maxLength={200}
                                className="flex-1"
                                value={addOn.name}
                                onChange={(event) =>
                                  setCategories((current) =>
                                    mapAddOn(
                                      current,
                                      category.key,
                                      product.key,
                                      addOnCategory.key,
                                      addOn.key,
                                      (node) => ({ ...node, name: event.target.value }),
                                    ),
                                  )
                                }
                              />
                              <Input
                                id={`addon-price-${addOn.key}`}
                                label="Price"
                                type="number"
                                step="0.01"
                                min="0"
                                max="10000"
                                className="w-28"
                                value={addOn.price}
                                onChange={(event) =>
                                  setCategories((current) =>
                                    mapAddOn(
                                      current,
                                      category.key,
                                      product.key,
                                      addOnCategory.key,
                                      addOn.key,
                                      (node) => ({ ...node, price: event.target.value }),
                                    ),
                                  )
                                }
                              />
                              <Checkbox
                                id={`addon-hidden-${addOn.key}`}
                                label="Hidden"
                                className="mb-2"
                                checked={Boolean(addOn.hidden)}
                                onChange={(event) =>
                                  setCategories((current) =>
                                    mapAddOn(
                                      current,
                                      category.key,
                                      product.key,
                                      addOnCategory.key,
                                      addOn.key,
                                      (node) => ({ ...node, hidden: event.target.checked }),
                                    ),
                                  )
                                }
                              />
                              <Button
                                size="sm"
                                variant="ghost"
                                className="mb-1"
                                onClick={() =>
                                  setCategories((current) =>
                                    mapAddOnCategory(
                                      current,
                                      category.key,
                                      product.key,
                                      addOnCategory.key,
                                      (node) => ({
                                        ...node,
                                        addOns: deleteNode(node.addOns, addOn.key),
                                      }),
                                    ),
                                  )
                                }
                              >
                                Remove
                              </Button>
                            </li>
                          ))}
                        </ul>

                        <div className="mt-3 flex gap-2">
                          <Button
                            size="sm"
                            variant="secondary"
                            onClick={() =>
                              addAddOn(category.key, product.key, addOnCategory.key)
                            }
                          >
                            Add option
                          </Button>
                          <Button
                            size="sm"
                            variant="danger"
                            onClick={() =>
                              setCategories((current) =>
                                mapProduct(current, category.key, product.key, (node) => ({
                                  ...node,
                                  addOnCategories: deleteNode(
                                    node.addOnCategories,
                                    addOnCategory.key,
                                  ),
                                })),
                              )
                            }
                          >
                            Delete group
                          </Button>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              </details>
            ))}
          </div>
        </div>
      ))}
    </div>
  )
}
