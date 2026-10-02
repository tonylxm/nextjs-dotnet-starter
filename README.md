# nextjs-dotnet-starter

Phase 0 walking skeleton for the "serious SaaS / marketplace" default stack: a Next.js + TypeScript web app and an ASP.NET Core (.NET 10) API on PostgreSQL. CI checks both apps, including an API integration test against real Postgres via Testcontainers.

## Layout

| Path                 | What                                                                                                            |
| -------------------- | --------------------------------------------------------------------------------------------------------------- |
| `apps/web`           | Next.js (App Router), Tailwind, shadcn/ui, Zod, Vitest, Playwright                                              |
| `apps/api`           | ASP.NET Core minimal API, EF Core + Npgsql, OpenAPI, health check, Supabase JWT auth, xUnit v3 + Testcontainers |
| `docker-compose.yml` | Local Postgres for the API                                                                                      |

## Create a project from it

```bash
gh repo create <app> --template tonylxm/nextjs-dotnet-starter --private --clone
```

Then:

1. Rename: set `name` in `package.json`, rename `Starter.*` in `apps/api` (projects, namespaces, `Starter.slnx`, the database name), and replace this README.
2. `pnpm install` (also installs the Husky hook) and `dotnet tool restore` in `apps/api`.
3. `docker compose up -d`, then `dotnet run --project apps/api/src/Starter.Api` (http://localhost:5080, health at `/health`).
4. Copy `apps/web/.env.example` to `apps/web/.env.local`, then `pnpm dev` (http://localhost:3000).
5. In the GitHub repo settings, turn on secret scanning and push protection (templates don't copy settings).
6. Deploy: the web app on Vercel (root directory `apps/web`). The API needs a container host plus a `deploy.yml` on `push: main` that runs migrations as an explicit step (see project-starter's tech-stack.md, CI/CD).

**Team mode** (branch + PR, CI blocks merge): remove the hook with `pnpm remove -w husky lint-staged && rm -rf .husky`, delete the `prepare` and `lint-staged` entries from `package.json` and the `.lintstagedrc.json` files, and protect `main` so it requires both CI jobs.

## Commands

| Where      | Command                                                     | Does                                                         |
| ---------- | ----------------------------------------------------------- | ------------------------------------------------------------ |
| root       | `pnpm lint` / `typecheck` / `test` / `test:e2e` / `build`   | Runs the web app's scripts                                   |
| root       | `pnpm format` / `format:check`                              | Prettier for the whole repo                                  |
| `apps/api` | `dotnet build` / `dotnet test`                              | Build, and tests (Docker must be running for Testcontainers) |
| `apps/api` | `dotnet format`                                             | C# formatting, checked in CI with `--verify-no-changes`      |
| `apps/api` | `dotnet ef migrations add <Name> --project src/Starter.Api` | First migration, once there are entities                     |

## What's included

- Deny-by-default auth: the API validates Supabase Auth JWTs (issuer `<Supabase:Url>/auth/v1`, audience `authenticated`, keys from its JWKS). Every endpoint requires a signed-in user unless it calls `.AllowAnonymous()`; `/me` is the example protected endpoint.
- Fail-fast config: the API throws on start without `ConnectionStrings:Default` or `Supabase:Url` (env var `Supabase__Url`; local default is `supabase start` on port 54321), and the web app validates `API_URL` at server start (`apps/web/src/instrumentation.ts`).
- CI with a job per app, and Dependabot for npm, NuGet and Actions.
- A pre-commit hook that runs lint-staged (auto-fix only, no tests). C# files get `dotnet format whitespace`.
- `apps/web/AGENTS.md` holds only the Next.js agent rules block. project-starter's agents-md step writes the root `AGENTS.md`.
