# Shopwise: e-commerce V1

Angular 22 storefront and admin, ASP.NET Core (.NET 10) API, MongoDB.

## Run locally

```powershell
powershell -ExecutionPolicy Bypass -File .\start.ps1   # MongoDB + API + Angular
powershell -ExecutionPolicy Bypass -File .\stop.ps1
```

| What | URL |
|---|---|
| Store | http://localhost:4200 |
| API + Swagger | http://localhost:5211/swagger |

Seeded admin: `admin@example.com` / `Admin@123` (set under `Seed` in `backend/appsettings.json`).
On first start the demo catalog is seeded: Men and Women categories, each with T-Shirts, Jeans, Shoes and Sneakers
(88 products) with sizes, stock, size charts, reviews and real product photos, plus the coupons
`WELCOME10` (10%, min ₹500), `FLAT200` (₹200 off, min ₹1,500) and `SAVE20` (20%, min ₹3,000).
To wipe products, reviews and categories and re-seed, start the API once with `Seed__ResetCatalog=true`.

Product photos are hotlinked from Unsplash (free licence) and listed in
`backend/Infrastructure/Seed/PhotoCatalog.cs` with the colour and style taken from each photo's description,
so they need internet access. Replace them with your own images through the admin product form.

MongoDB runs as a portable process from `tools/mongodb` (not committed) with data in `data/db`. If you have MongoDB
installed as a service, skip it: `start.ps1` only starts `mongod` when port 27017 is free, and the connection string
is in `appsettings.json`. The 8.x rapid-release build did not load on Windows 10 22H2 here, so 7.0.x is used.

## Hosting

Three pieces are needed: a MongoDB database (e.g. a free MongoDB Atlas cluster), the API, and the static frontend.

**API** (`backend/Dockerfile`, listens on port 8080; set these environment variables):

| Variable | Value |
|---|---|
| `MongoDb__ConnectionString` | your MongoDB connection string |
| `MongoDb__DatabaseName` | e.g. `ecommerce` |
| `Jwt__Secret` | a long random string (32+ characters); the API refuses to start in Production with the dev default |
| `Seed__AdminPassword` | the admin password to create on first start (must not be `Admin@123` in Production) |
| `Cors__AllowedOrigins__0` | your frontend URL, e.g. `https://my-shop.example.com` |

**Frontend**: set `apiUrl` in `frontend/src/environments/environment.ts` to your deployed API (ending in `/api`), then
`cd frontend && npm ci && npm run build`. Publish `frontend/dist/ecommerce-web/browser` on any static host
(Netlify, Vercel, GitHub Pages, ...) with single-page-app fallback to `index.html`.

The Dockerfile has not been test-built on this machine. Payment, SMS and Google login are still the mocked
providers described below.

## Layout

- `backend/` is a single project (`ECommerce.Api.csproj`, so any builder that looks for a `.csproj` in the folder
  works; set the build root to `backend`) organised in folders: `Domain` (Mongo documents), `Application` (DTOs and
  service interfaces), `Infrastructure` (Mongo context, services, mocked providers), `Api` (controllers, middleware),
  plus `Program.cs` for wiring, JWT and Swagger.
- `frontend/src/app/`: `core/` (auth, API client, cart state, guards), `shared/`, `features/`
  (auth, catalog, cart, checkout, orders, admin).

## Mocked integrations (swap each by implementing one interface)

| Feature | Interface | Dev implementation |
|---|---|---|
| Payment gateway | `IPaymentGateway` | `MockPaymentGateway`: always succeeds |
| Mobile OTP | `IOtpSender` | `ConsoleOtpSender`: OTP is written to the API log |
| Google login | `IGoogleTokenValidator` | `DevGoogleTokenValidator`: accepts a base64 JSON `{sub,email,name}`; the login page has a simulated Google form |

Register replacements in `backend/Program.cs`. Before going live, change `Jwt:Secret`.

## Feature map

Customer: email / mobile OTP / Google login, categories, brands, filters, sorting, search, product page
(images, video, size chart, ratings, reviews), cart, wishlist, save for later, address book, coupons, checkout,
order history, tracking timeline, cancellation (restocks), returns.
Admin (`/admin`): products (variants, media, size chart), categories and brands, inventory adjustments, order
status management, users (enable/disable), coupons.
