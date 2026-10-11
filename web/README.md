# FoodOrdering web client

React + Vite frontend for the FoodOrderingApi (pickup-only food ordering).
JavaScript/JSX, React Router, Tailwind CSS v4, plain `fetch`. No state library and
no component library.

## Reference docs

The API reference lives with the API, at the repository root:

- [`../ARCHITECTURE.md`](../ARCHITECTURE.md) — domain model, guards, error shape.
- [`../ROUTES.md`](../ROUTES.md) — every route and the guard in front of it.

Request/response shapes were taken from the running API's OpenAPI document
(`/openapi/v1.json`, Development only), not guessed.

Out of scope (per the brief's "Things the system will not have" / "Post
Front-End" lists): delivery options, payments, a server-side cart, holiday
unavailability, pre-orders, search, images, reviews, pagination, notifications,
rate limiting, health checks, and anything published by an admin other than
granting/revoking the Owner role.

## Running it

Two terminals. The API first:

```bash
cd ../FoodOrderingApi
dotnet run --launch-profile https        # https://localhost:7039 (+ http on 5258)
```

The profile matters: `dotnet run` on its own picks the **first** profile in
`launchSettings.json`, which is `http` — that binds only `http://localhost:5258`,
so the proxy's `https://localhost:7039` target has nothing to connect to and the
app shows "Could not reach the API". Use the `https` profile (or your IDE's
"https" launch profile). Don't point the proxy at `5258` instead: while the https
profile is running, that port answers `307` towards `https://localhost:7039`, and
the browser ends up on the untrusted certificate anyway.

Then the frontend:

```bash
cd web
npm install
npm run dev                              # http://localhost:5173
```

Development-only accounts seeded by the API (`appsettings.Development.json`):

| Account | Password | What it can do |
|---|---|---|
| `admin@admin.com` | `Admin123!` | Grant/revoke the Owner role |
| `owner@example.com` | `Owner123!` | Owns All Night Kebab (menu, hours, managers) |
| `customer@example.com` | `Customer123!` | Places and cancels orders |

All Night Kebab is open 24 hours, so it is the place to order from at any time;
Burger Joint has no opening hours and is therefore always closed.

## Where the API URL lives

One place: `.env`.

- `VITE_API_URL` — empty in development, so requests go to the dev server's own
  origin and Vite proxies `/api` to the API. Set it to the API's public origin
  (`https://api.example.com`) for a deployed build.
- `VITE_API_PROXY_TARGET` — the proxy target, i.e. the `https://` URL from
  `FoodOrderingApi/Properties/launchSettings.json`. Never the `http://` one: it
  only redirects to https.

Proxying means the browser never talks to `https://localhost:7039` directly, so
the ASP.NET development certificate does not have to be trusted by the browser
(no `ERR_CERT_AUTHORITY_INVALID`) and there is no cross-origin request to
preflight. With `VITE_API_URL` set to a real origin the app calls the API
cross-origin, which the API allows for the origins in `Cors:AllowedOrigins`.

## Layout

```
src/
  api/         client.js (request + ApiError) and one module per area
  auth/        AuthContext, localStorage token store
  components/  shared UI: Button, Field, Card, Spinner, ErrorMessage, Badge, …
  hooks/       useApi (fetch on mount + reload)
  lib/         order status, opening hours, formatting, place-graph helpers
  pages/       public/ customer/ manager/ owner/ admin/
  routes.jsx   route table and guards
```

## Notes worth knowing

- **Roles are cosmetic in the client.** `roles` decides which navigation links
  appear; the server enforces everything. "Manager" is not a role — manager
  access is per place and is discovered from `GET /api/v1/manager/places`.
- **Errors.** Every API failure is RFC 7807 Problem Details, surfaced as
  `detail ?? title` plus the per-field `errors` entries. A 401 on a request that
  carried a token clears the stored session and returns to `/login` with an
  explanation, because there is no refresh token.
- **Owner place PATCH is not a plain full replace.** Deletion requires
  `destroy: true` alongside a non-zero id; an omitted or empty array leaves the
  stored children alone. The editor therefore marks removals instead of dropping
  them, and sends every node it can still see.
- **Hidden items cannot be read back.** `PrivatePlaceDto` stops at the product
  and the only product endpoint that returns add-ons is public, so a hidden
  product (or one under a hidden category) has no readable add-on list. The
  editor says so and leaves those add-ons untouched rather than deleting them.
- **`isOpen` is always the server's answer.** The browser renders the computed
  value and never evaluates opening hours.
