import { NavLink, useParams } from 'react-router-dom'

const TABS = [
  { to: '', label: 'Details & menu' },
  { to: 'hours', label: 'Opening hours' },
  { to: 'managers', label: 'Managers' },
]

export function OwnerPlaceTabs() {
  const { id } = useParams()

  return (
    <nav className="flex flex-wrap gap-1 border-b border-slate-200">
      {TABS.map((tab) => (
        <NavLink
          key={tab.to}
          to={tab.to ? `/owner/places/${id}/${tab.to}` : `/owner/places/${id}`}
          end={tab.to === ''}
          className={({ isActive }) =>
            `-mb-px border-b-2 px-3 py-2 text-sm font-medium transition ${
              isActive
                ? 'border-slate-900 text-slate-900'
                : 'border-transparent text-slate-600 hover:text-slate-900'
            }`
          }
        >
          {tab.label}
        </NavLink>
      ))}
    </nav>
  )
}
