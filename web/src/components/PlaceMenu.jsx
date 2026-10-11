import { formatPrice } from '../lib/format.js'

// The menu comes from the public place response, so hidden categories and
// products are already filtered out by the server. Add-ons are not part of it:
// they are fetched per product when the customer opens the item editor.
export function PlaceMenu({ categories, onAdd, disabled }) {
  if (!categories || categories.length === 0) {
    return (
      <p className="text-sm text-slate-600">
        This place has not published a menu yet.
      </p>
    )
  }

  return (
    <div className="space-y-6">
      {categories.map((category) => (
        <section key={category.id}>
          <h3 className="text-sm font-semibold tracking-wide text-slate-500 uppercase">
            {category.name}
          </h3>
          <ul className="mt-2 divide-y divide-slate-100">
            {category.products.length === 0 && (
              <li className="py-2 text-sm text-slate-500">No products listed.</li>
            )}
            {category.products.map((product) => (
              <li
                key={product.id}
                className="flex items-center justify-between gap-4 py-3"
              >
                <div>
                  <p className="text-sm font-medium">{product.name}</p>
                  {product.description && (
                    <p className="text-xs text-slate-500">{product.description}</p>
                  )}
                </div>
                <div className="flex shrink-0 items-center gap-3">
                  <span className="text-sm text-slate-700">
                    {formatPrice(product.price)}
                  </span>
                  <button
                    type="button"
                    onClick={() => onAdd(category.id, product)}
                    disabled={disabled}
                    className="rounded-md bg-slate-900 px-2.5 py-1.5 text-xs font-medium text-white transition hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-50"
                  >
                    Add
                  </button>
                </div>
              </li>
            ))}
          </ul>
        </section>
      ))}
    </div>
  )
}
