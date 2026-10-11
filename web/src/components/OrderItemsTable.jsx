import { formatPrice, lineTotal, orderTotal } from '../lib/format.js'

// Shared by the customer's order detail and the manager's view of an order. The
// name/price on every row are snapshots the API took when the order was placed,
// so editing the menu later never rewrites history.
export function OrderItemsTable({ order }) {
  return (
    <div>
      <table className="w-full text-sm">
        <thead>
          <tr className="text-left text-xs tracking-wide text-slate-500 uppercase">
            <th className="pb-2 font-medium">Item</th>
            <th className="pb-2 text-right font-medium">Unit</th>
            <th className="pb-2 text-right font-medium">Qty</th>
            <th className="pb-2 text-right font-medium">Total</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100">
          {order.orderItems.map((item) => (
            <tr key={item.id} className="align-top">
              <td className="py-2">
                <p className="font-medium">{item.name}</p>
                {item.orderItemAddOns.length > 0 && (
                  <ul className="mt-0.5 text-xs text-slate-500">
                    {item.orderItemAddOns.map((addOn) => (
                      <li key={addOn.id}>
                        {addOn.quantity} × {addOn.name} ({formatPrice(addOn.price)})
                      </li>
                    ))}
                  </ul>
                )}
              </td>
              <td className="py-2 text-right tabular-nums">{formatPrice(item.price)}</td>
              <td className="py-2 text-right tabular-nums">{item.quantity}</td>
              <td className="py-2 text-right tabular-nums">{formatPrice(lineTotal(item))}</td>
            </tr>
          ))}
        </tbody>
        <tfoot>
          <tr className="border-t border-slate-200 font-medium">
            <td className="pt-2" colSpan={3}>
              Total
            </td>
            <td className="pt-2 text-right tabular-nums">{formatPrice(orderTotal(order))}</td>
          </tr>
        </tfoot>
      </table>
    </div>
  )
}
