# Continuing this project on another machine

Cursor chat history lives **per-machine** and does not sync across PCs. When you switch from one Cursor install to another, your chats stay behind, but everything important about this project is captured in the repository itself so a fresh chat picks up the context immediately.

## On the new PC

1. **Clone and open in Cursor:**
   ```bash
   git clone https://github.com/loopstudioorg-lab/pharmacy-saas.git
   cd pharmacy-saas
   cursor .
   ```

2. **Open a new chat** in Cursor.

3. **First message** should reference the persistent context files:

   > Read `AGENTS.md` and `docs/PHASE1_PLAN.md` then continue Phase 1.

   The agent automatically auto-loads `AGENTS.md` and `.cursor/rules/conventions.mdc`, so even without this prompt the architectural rules and decisions will be active. Adding the `@docs/PHASE1_PLAN.md` reference primes the current phase.

## Persistent context files in this repo

| File | Purpose |
|---|---|
| [`AGENTS.md`](../AGENTS.md) | All locked-in decisions, architecture rules, phase plan. Auto-loaded by Cursor. |
| [`.cursor/rules/conventions.mdc`](../.cursor/rules/conventions.mdc) | Hard coding rules (audit-on-mutation, soft-delete, layering, etc.). Auto-loaded. |
| [`docs/PHASE1_PLAN.md`](PHASE1_PLAN.md) | Detailed Phase 1 plan with todos and DoD. |
| [`docs/Pharmacy_Management_System_Complete_Blueprint_v2.0.md`](Pharmacy_Management_System_Complete_Blueprint_v2.0.md) | The original blueprint - source of truth for product requirements. |
| [`README.md`](../README.md) | Build/test instructions. |
| GitHub issues / PR descriptions | Per-task context (start using these in Phase 2 onward). |

## What you usually had to remember from the chat

All of it now lives in `AGENTS.md`:

- Stack: WPF + .NET 8 + SQL Express + EF Core/Dapper hybrid
- `BranchId` baked into schema from day 1
- Tax engine optional, FBR out of scope
- Phase 1 demo credentials: `PHM-DEMO` / `123456`
- Connection string default: `Server=.\SQLEXPRESS;Database=PharmacySaas;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;`
- CI builds installer on every push to `main` (`.github/workflows/build-windows.yml`)
- Phase plan: 1 (foundation, current) -> 2 (Medicines+Stock) -> 3 (Purchase) -> 3.5 (Smart Invoice Import mock) -> 4 (POS) -> ...

## Daily workflow on the new PC

```bash
# Pull latest before each session
git pull

# Make changes, build, test
dotnet build PharmacySaas.sln
dotnet test
dotnet run --project src/PMS.Desktop          # only on Windows

# Commit + push -> CI auto-builds a fresh installer
git add -A
git commit -m "..."
git push

# Watch CI / download installer
gh run list --repo loopstudioorg-lab/pharmacy-saas --limit 1
# Then go to https://github.com/loopstudioorg-lab/pharmacy-saas/actions to download the artifact
```

## When you're stuck mid-task and want to switch machines

1. Commit your work-in-progress to a branch:
   ```bash
   git checkout -b wip/<short-description>
   git add -A
   git commit -m "WIP: <what you were doing>"
   git push -u origin HEAD
   ```
2. On the other PC: `git fetch && git checkout wip/<short-description>`
3. New chat, point it at the branch and the file you were working on:
   > I was in the middle of `<file>` doing `<task>`. Continue.

That's it. The repo is the source of truth.
