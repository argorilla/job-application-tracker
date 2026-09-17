# Job Application Tracker

Job Application Tracker is a server-rendered web application for keeping a personal job search organized in one place. It records applications, tracks their current status, supports focused search and filtering, and summarizes the pipeline on a dashboard.

## Overview

The project is an ASP.NET Core MVC application backed by SQLite and Entity Framework Core. Razor Views provide the UI, Bootstrap supplies the layout and components, and data annotations enforce validation on both the server and, where supported, in the browser. Its configurable appearance combines Light, Dark, or System mode with one of five independent accent palettes.

The repository also explores a local authentication design: the application seeds one configured user, verifies an ASP.NET Core Identity password hash, issues a JWT, and stores that token in an HttpOnly cookie. This is an educational implementation for local use, not a production-ready identity system.

For a more detailed explanation of boundaries and request flows, see [docs/architecture.md](docs/architecture.md).

## Key features

- Create, view, edit, and delete job applications.
- Track company, position, source, URL, location, application date, status, and notes.
- Validate required values, field lengths, URLs, enum values, and future application dates.
- Search company and position names and filter by application status.
- Browse results with forward and backward cursor-based pagination.
- View total, active, interview, and offer counts plus the five most recent applications.
- Protect the dashboard and application-management routes with authorization.
- Seed starter application records and one locally configured user.
- Manage the schema through committed Entity Framework Core migrations.
- Use a responsive application shell with aligned header, content, and footer regions.
- Choose Light, Dark, or System mode independently from the Ocean, Fall, Coffee, Sakura, or Forest accent palette through one compact Appearance dropdown.
- Preserve browser-local appearance choices across navigation and synchronize changes across tabs; System mode follows live operating-system color-scheme changes.
- Review the anonymously accessible Data & Privacy implementation overview.
- Display configured application metadata in a responsive footer.

The five palettes support both effective Bootstrap modes, producing ten palette/mode visual combinations. These combinations, along with Light, Dark, and System behavior, have been manually verified in multiple browsers.

## Screenshots

Screenshots are not currently included in the repository. Useful portfolio additions would be:

1. The authenticated dashboard with summary cards and recent applications.
2. The application list with a search term, status filter, and pagination controls.
3. The create or edit form showing validation feedback.
4. The login screen.
5. A small representative appearance set: Ocean Light, Ocean Dark, Sakura Light, and Forest Dark.

Store future images in a repository folder such as `docs/images/` and replace this section with relative Markdown links. Use demonstration data rather than real application details.

## Technology stack

| Area | Technology | Version verified in repository |
| --- | --- | --- |
| Runtime and web framework | .NET / ASP.NET Core MVC | .NET 9 (`net9.0`) |
| Language and templates | C# / Razor Views | .NET 9 toolchain |
| ORM | Entity Framework Core SQLite and Design packages | 9.0.9 |
| Authentication handler | ASP.NET Core JWT Bearer | 9.0.9 |
| EF CLI tool | `dotnet-ef` local tool | 9.0.9 |
| UI and color modes | Bootstrap | 5.3.3 |
| Appearance styling | CSS custom properties and Bootstrap variable mappings | Project-owned CSS |
| Browser scripting | jQuery | 3.7.1 |
| Client-side validation | jQuery Validation | 1.21.0 |
| Database | SQLite | via EF Core SQLite 9.0.9 |

Package versions come from `JobApplicationTracker.csproj`, `.config/dotnet-tools.json`, and the checked-in client-library headers.

## Architecture summary

The application follows the standard server-rendered MVC pattern:

- Controllers coordinate HTTP requests, authorization, model validation, persistence, and redirects.
- Domain entities in `Models/` define the persisted data and validation rules.
- ViewModels shape dashboard, login, and list-page data for Razor Views.
- `ApplicationDbContext` provides EF Core access to the SQLite `JobApplications` and `Users` tables.
- `JwtTokenService` creates signed access tokens; JWT Bearer middleware reads them from the authentication cookie.
- `JobApplicationCursorCodec` serializes pagination keys into URL-safe opaque cursor strings.
- `ApplicationInfoOptions` binds presentation metadata, while `ThemeOptions` validates shared appearance defaults.
- A synchronous pre-render initializer resolves browser-local mode and palette preferences before Bootstrap and the palette stylesheet are applied.

All application records currently belong to one shared dataset; there is no user-to-application ownership relationship.

## Project structure

```text
.
├── Configuration/
│   ├── ApplicationInfoOptions.cs # Shared presentation metadata
│   ├── ThemeMode.cs              # Light, Dark, and System preferences
│   ├── ThemePalette.cs           # Closed set of five accent palettes
│   └── ThemeOptions.cs           # Validated appearance defaults and policies
├── Controllers/                # Login, dashboard, and job-application request handlers
├── Data/                       # EF Core context and startup seeders
├── Migrations/                 # Versioned EF Core schema history
├── Models/                     # Persisted entities, status enum, and error model
├── Pagination/                 # Cursor value and Base64 URL codec
├── Security/                   # Authentication cookie-name constant
├── Services/                   # JWT creation and token result
├── ViewModels/                 # Presentation-specific page models
├── Views/                      # Razor pages and shared layout/partials
├── wwwroot/
│   ├── css/theme-palettes.css  # Light/dark palette tokens and Bootstrap mappings
│   └── js/theme-initializer.js # Pre-render resolution and synchronization
├── .config/dotnet-tools.json   # Reproducible local EF CLI version
├── JobApplicationTracker.csproj
├── Program.cs                  # Composition root and HTTP pipeline
└── appsettings.json            # Non-secret shared configuration
```

`Migrations/` and `.config/dotnet-tools.json` should be committed: migrations make database changes reviewable and repeatable, while the tool manifest pins the EF CLI used to manage them. Source, views, static assets, project files, and safe shared configuration should also be committed. Local SQLite files, build output, IDE state, local configuration overrides, and secrets should not be committed.

## Appearance configuration and browser preferences

The shared navbar exposes one Appearance dropdown. Its custom-styled controls remain native radio inputs: Mode is presented as a segmented group with Light, Dark, and System choices, while Palette uses labeled cards and swatches for Ocean, Fall, Coffee, Sakura, and Forest. This preserves native radio semantics and keyboard behavior while keeping appearance settings out of the primary navigation.

The committed `Theme` section contains safe, non-secret defaults:

```json
"Theme": {
  "DefaultMode": "System",
  "AllowUserSelection": true,
  "DefaultPalette": "Ocean",
  "AllowPaletteSelection": true
}
```

Valid `DefaultMode` values are `Light`, `Dark`, and `System`. Valid `DefaultPalette` values are `Ocean`, `Fall`, `Coffee`, `Sakura`, and `Forest`. `AllowUserSelection` controls whether the mode radio group and stored mode preference are enabled; `AllowPaletteSelection` independently controls the palette radio group and stored palette preference. The Appearance dropdown is omitted when both policies are disabled. The `Theme` section is required, and unsupported enum values fail configuration binding or startup options validation. These settings are suitable for `appsettings.json`; they are not secrets and do not belong in User Secrets.

Mode resolution uses this precedence:

1. A valid `jobApplicationTracker.theme` value when mode selection is enabled.
2. The configured default mode.
3. A safe browser fallback; `System` resolves through `prefers-color-scheme`.

Palette resolution uses this precedence:

1. A valid `jobApplicationTracker.palette` value when palette selection is enabled.
2. The configured default palette.
3. The Ocean fallback.

Storage access is defensive: unavailable storage does not break rendering or navigation. Explicit choices survive navigation and reload, changes synchronize across tabs, and System reacts to live operating-system changes. Disabling either selector ignores its stored preference without deleting it. Both preferences are browser-local; neither is stored in SQLite nor attached to `AppUser`.

Bootstrap receives only `light` or `dark` through `data-bs-theme`. The independent `data-theme-palette` attribute selects an accent token set. The palette layer maps those tokens to Bootstrap primary, link, button-state, and focus variables while leaving success, danger, warning, info, and neutral semantics unchanged. Mapping only `--bs-primary` would be insufficient because Bootstrap buttons define component-local state variables. See [the architecture document](docs/architecture.md#appearance-resolution-and-styling) for the detailed lifecycle and token design.

Safe display metadata is also committed in `appsettings.json`:

```json
"ApplicationInfo": {
  "Name": "Job Application Tracker",
  "Applicator": "argoraden",
  "CopyrightYear": 2026
}
```

The shared footer reads these values through `ApplicationInfoOptions`; none is secret.

## Data model and status lifecycle

`JobApplication` stores an integer ID, company and position names, source, optional job URL and location, applied date, status, and optional notes. `AppUser` stores a unique normalized username and a password hash; it does not store a plaintext password.

Available application statuses are:

```text
Draft → Applied → Interview → Technical Test → Offer
                         └────────────────────→ Rejected
Any active stage ─────────────────────────────→ Withdrawn
```

This diagram describes the intended human workflow. The code exposes the seven enum values but does not enforce transition rules, so an application can be changed directly from any status to any other status.

## Cursor-based pagination

The application list returns 10 records at a time and uses keyset/cursor pagination rather than offsets or numbered pages. Its deterministic order is:

```text
AppliedDate descending, then Id descending
```

`Id` is the secondary key that disambiguates records with the same application date. A next-page request selects records with an earlier `AppliedDate`, or a smaller `Id` when dates match. A previous-page request applies the inverse comparison in ascending order, takes one extra record to detect another page, and reverses the result back into display order.

Each cursor contains the boundary record's `AppliedDate` and `Id`, serialized as JSON and Base64 URL encoded. The encoding makes the value URL-safe and opaque-looking; it is not encryption, signing, or tamper protection. Invalid cursors produce HTTP 400 responses.

Search and status filters are applied before the cursor boundary. Pagination links carry the active `q` and `status` values so navigation remains in the filtered result set. Cursors do not themselves bind or validate those filters; changing a filter while reusing an old cursor can therefore produce an empty page, which the controller redirects to the first page of the new result set when matching rows exist.

## Authentication and security notes

The current authentication flow is intended for education and local development:

1. A single user is created at startup from `SeedUser` configuration when the `Users` table is empty.
2. Login looks up the normalized username and verifies the stored hash with ASP.NET Core's `PasswordHasher<TUser>`.
3. The application issues an HMAC-SHA256 JWT containing user identifiers, username, and a unique token ID.
4. The JWT is stored in an HttpOnly, SameSite `Strict` cookie. The cookie's `Secure` flag is enabled when the login request uses HTTPS.
5. JWT Bearer middleware reads the token from that cookie, validates issuer, audience, signature, and lifetime, and establishes the user principal.
6. `[Authorize]` protects the dashboard and all job-application actions. Unauthorized browser requests are redirected to login with a local return URL.

State-changing form actions use anti-forgery validation, and Razor form tag helpers emit the corresponding tokens. The login response only redirects to a return URL accepted by `Url.IsLocalUrl`, reducing open-redirect risk.

This design has no refresh-token flow, server-side token revocation, MFA, lockout, password recovery, signing-key rotation, roles, registration, or multi-user administration. The cursor is also unsigned. For a production server-rendered MVC application, prefer ASP.NET Core Identity with cookie authentication, or delegate identity to an OpenID Connect provider such as Keycloak, Auth0, Microsoft Entra ID, or Amazon Cognito.

## Data & Privacy

The existing `Home/Privacy` route remains anonymously accessible and is presented as **Data & Privacy**. It summarizes the current implementation's SQLite records, local account data, authentication cookie, browser appearance preferences, local secret handling, external-service boundaries, self-hosted deployment responsibilities, and known limitations. It is an implementation overview rather than a formal legal policy.

## Local setup

### Prerequisites

- .NET SDK 9.0. The repository currently builds with SDK 9.0.305.
- A shell that can run the .NET CLI.
- HTTPS development-certificate trust if you want the browser to trust the local HTTPS endpoint:

  ```bash
  dotnet dev-certs https --trust
  ```

### Restore the project and tools

From the repository root:

```bash
dotnet restore
dotnet tool restore
```

### Configure the local user and User Secrets

The project has a `UserSecretsId`, and startup requires a JWT signing key plus credentials for the initial local user. Set them without editing `appsettings.json`:

```bash
dotnet user-secrets set "SeedUser:Username" "<your-username>"
dotnet user-secrets set "SeedUser:Password" "<your-password>"
dotnet user-secrets set "Jwt:Key" "<your-random-jwt-key>"
```

Use a high-entropy JWT key of at least 32 UTF-8 bytes. Do not use the literal placeholders, and do not commit secret values. The non-secret JWT issuer, audience, expiration, SQLite connection string, display metadata, and appearance defaults are provided in `appsettings.json`. No additional appearance setup is required.

User Secrets are outside this repository. Do not inspect, copy, or publish the generated secrets file.

### Database setup and migrations

Restore the local EF tool and create/update the local SQLite schema:

```bash
dotnet tool restore
dotnet ef database update
```

The configured local database is `job-applications.db` in the repository root. It and its SQLite sidecar files are ignored by Git. Do not commit them: they may contain password hashes and private job-search information.

On startup, the application inserts starter job-application rows only if none exist and creates the configured local user only if the user table is empty. The initializer does not apply migrations automatically, so `dotnet ef database update` is required for a clean clone.

When intentionally changing the entity model, create and review a migration before committing it:

```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

### Run the application

```bash
dotnet run
```

The checked-in launch profile exposes `https://localhost:7014` and `http://localhost:5076`. Prefer HTTPS so the authentication cookie receives the `Secure` flag. Sign in with the local username and password configured through User Secrets.

## Validation and testing

There is currently no automated test project. Validate changes with:

```bash
dotnet restore
dotnet tool restore
dotnet build --no-restore
dotnet ef database update
dotnet run
```

Then manually verify login/logout, authorization redirects, dashboard totals, CRUD operations, field validation, combined search/status filtering, and both pagination directions. Shell checks should cover anonymous and authenticated header states, Login-page Sign in suppression, the collapsed mobile navbar, short- and long-page footer placement, and responsive footer stacking. Appearance checks should cover keyboard operation of the dropdown and native radio groups, all three mode preferences, all five palettes, all ten effective palette/mode combinations, live System changes, refresh and navigation, cross-tab synchronization, and focus, hover, active, disabled, link, and button states. Verify Data & Privacy anonymously in both effective Light and Dark modes.

The ten visual combinations and multi-browser mode behavior have been manually verified by the project owner. Repository validation covers builds and static checks, but there are currently no automated browser or accessibility tests. For documentation-only changes, also run:

```bash
git diff --check
```

## Known limitations

- Authentication is a local experiment with one seeded account, not a production identity solution.
- Job applications are not associated with individual users.
- JWTs cannot be refreshed or revoked before expiry, and signing-key rotation is not implemented.
- Pagination cursors are encoded but neither encrypted nor signed.
- Seed data is embedded in source and should use clearly fictional examples before public release.
- Database migrations must be applied manually before first run.
- There is no automated test project.
- Screenshots are not present.

## Possible future improvements

- Replace the authentication experiment with ASP.NET Core Identity/cookies or an external OpenID Connect provider.
- Add per-user ownership and authorization checks for application records.
- Add automated controller, validation, authentication, and pagination tests.
- Sign pagination cursors or validate them against the active filter context.
- Add optimistic-concurrency tokens and user-friendly conflict handling.
- Replace starter records with explicitly fictional sample data or an opt-in development seeder.
- Add structured logging, health checks, and deployment-specific configuration validation.
- Add privacy-safe screenshots.

## License

This project is licensed under the [MIT License](LICENSE).
