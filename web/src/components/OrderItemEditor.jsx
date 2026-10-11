import { useEffect, useState } from 'react'
import { getProduct } from '../api/places.js'
import { formatPrice } from '../lib/format.js'
import { Button } from './Button.jsx'
import { ErrorMessage } from './ErrorMessage.jsx'
import { LoadingBlock } from './Spinner.jsx'
import { QuantityStepper } from './QuantityStepper.jsx'

// Add-on choices for one product. The rules mirrored from the server:
//   * a `required` category must have at least one choice;
//   * a category that is not `multichoice` allows exactly one (radio), otherwise
//     several (checkbox).
// The server re-validates the submitted order and rejects violations with 400.
export function OrderItemEditor({ selection, onClose, onConfirm }) {
  const [detail, setDetail] = useState(null)
  const [error, setError] = useState(null)
  const [loading, setLoading] = useState(false)
  const [quantity, setQuantity] = useState(1)
  const [choices, setChoices] = useState({})
  const [validationError, setValidationError] = useState(null)

  useEffect(() => {
    if (!selection) return
    let active = true
    setLoading(true)
    setError(null)
    setDetail(null)
    setQuantity(1)
    setChoices({})
    setValidationError(null)

    getProduct(selection.categoryId, selection.product.id)
      .then((product) => {
        if (active) setDetail(product)
      })
      .catch((cause) => {
        if (active) setError(cause)
      })
      .finally(() => {
        if (active) setLoading(false)
      })

    return () => {
      active = false
    }
  }, [selection])

  if (!selection) return null

  const addOnCategories = detail?.productAddOnCategories ?? []

  function selectSingle(addOnCategoryId, addOnId) {
    setChoices((current) => ({ ...current, [addOnCategoryId]: [{ addOnId, quantity: 1 }] }))
    setValidationError(null)
  }

  function toggleMulti(addOnCategoryId, addOnId) {
    setChoices((current) => {
      const selected = current[addOnCategoryId] ?? []
      const exists = selected.some((entry) => entry.addOnId === addOnId)
      return {
        ...current,
        [addOnCategoryId]: exists
          ? selected.filter((entry) => entry.addOnId !== addOnId)
          : [...selected, { addOnId, quantity: 1 }],
      }
    })
    setValidationError(null)
  }

  function setAddOnQuantity(addOnCategoryId, addOnId, nextQuantity) {
    setChoices((current) => ({
      ...current,
      [addOnCategoryId]: (current[addOnCategoryId] ?? []).map((entry) =>
        entry.addOnId === addOnId ? { ...entry, quantity: nextQuantity } : entry,
      ),
    }))
  }

  const selectedAddOns = addOnCategories.flatMap((category) =>
    (choices[category.id] ?? []).flatMap((entry) => {
      const addOn = category.addOns.find((candidate) => candidate.id === entry.addOnId)
      return addOn
        ? [{ addOnId: addOn.id, name: addOn.name, price: Number(addOn.price), quantity: entry.quantity }]
        : []
    }),
  )

  const unitPrice =
    Number(selection.product.price) +
    selectedAddOns.reduce((sum, addOn) => sum + addOn.price * addOn.quantity, 0)

  function handleConfirm() {
    const missing = addOnCategories.find(
      (category) => category.required && (choices[category.id]?.length ?? 0) === 0,
    )
    if (missing) {
      setValidationError(`Choose at least one option in “${missing.name}”.`)
      return
    }

    onConfirm({
      productId: selection.product.id,
      name: selection.product.name,
      price: Number(selection.product.price),
      quantity,
      addOns: selectedAddOns,
    })
  }

  return (
    <div className="fixed inset-0 z-20 flex items-start justify-center overflow-y-auto bg-slate-900/40 p-4">
      <div
        role="dialog"
        aria-modal="true"
        aria-label={`Configure ${selection.product.name}`}
        className="mt-10 w-full max-w-lg rounded-lg bg-white shadow-xl"
      >
        <div className="flex items-start justify-between border-b border-slate-200 p-4">
          <div>
            <h2 className="text-base font-semibold">{selection.product.name}</h2>
            {selection.product.description && (
              <p className="text-xs text-slate-500">{selection.product.description}</p>
            )}
          </div>
          <Button variant="ghost" size="sm" onClick={onClose}>
            Close
          </Button>
        </div>

        <div className="space-y-4 p-4">
          {loading && <LoadingBlock label="Loading options…" />}
          <ErrorMessage error={error} />

          {detail && (
            <>
              {addOnCategories.length === 0 && (
                <p className="text-sm text-slate-600">No options to choose for this item.</p>
              )}

              {addOnCategories.map((category) => {
                const selected = choices[category.id] ?? []
                return (
                  <fieldset key={category.id}>
                    <legend className="text-sm font-medium text-slate-800">
                      {category.name}
                      <span className="ml-2 text-xs font-normal text-slate-500">
                        {category.required ? 'Required' : 'Optional'}
                        {category.multichoice
                          ? ' · choose any'
                          : ' · choose one'}
                      </span>
                    </legend>

                    <ul className="mt-2 space-y-2">
                      {category.addOns.map((addOn) => {
                        const entry = selected.find((item) => item.addOnId === addOn.id)
                        const inputId = `addon-${category.id}-${addOn.id}`
                        return (
                          <li key={addOn.id} className="flex items-center gap-3">
                            <input
                              id={inputId}
                              type={category.multichoice ? 'checkbox' : 'radio'}
                              name={
                                category.multichoice
                                  ? undefined
                                  : `addon-category-${category.id}`
                              }
                              checked={Boolean(entry)}
                              onChange={() =>
                                category.multichoice
                                  ? toggleMulti(category.id, addOn.id)
                                  : selectSingle(category.id, addOn.id)
                              }
                              className="h-4 w-4 border-slate-300 text-slate-900 focus:ring-slate-500"
                            />
                            <label htmlFor={inputId} className="flex-1 text-sm text-slate-700">
                              {addOn.name}
                              <span className="ml-2 text-slate-500">
                                {formatPrice(addOn.price)}
                              </span>
                            </label>
                            {entry && (
                              <QuantityStepper
                                size="sm"
                                label={`${addOn.name} quantity`}
                                value={entry.quantity}
                                onChange={(next) => setAddOnQuantity(category.id, addOn.id, next)}
                              />
                            )}
                          </li>
                        )
                      })}
                      {category.addOns.length === 0 && (
                        <li className="text-xs text-slate-500">No options available.</li>
                      )}
                    </ul>
                  </fieldset>
                )
              })}

              {validationError && (
                <p className="rounded-md bg-rose-50 p-2 text-sm text-rose-800">{validationError}</p>
              )}

              <div className="flex items-center justify-between border-t border-slate-200 pt-4">
                <div className="flex items-center gap-3">
                  <span className="text-sm text-slate-600">Quantity</span>
                  <QuantityStepper value={quantity} onChange={setQuantity} label="Item quantity" />
                </div>
                <Button onClick={handleConfirm} disabled={loading}>
                  Add {formatPrice(unitPrice * quantity)}
                </Button>
              </div>
            </>
          )}
        </div>
      </div>
    </div>
  )
}
