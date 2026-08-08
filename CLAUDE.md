# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with
code in this repository.

## What this repo is

A Core Keeper mod that adds one chat command, `/refillboulders`, restoring
every currently loaded ore boulder to full health. An ore boulder has no
separate yield counter — its remaining ore *is* its `HealthCD.health`, so
topping the health back up refills the boulder. One CoreLib server command
handler against Pugstorm's `CoreKeeperModSDK`. No content of its own; hard-
depends on CoreLib for command registration. Personal-use, non-commercial
(Pugstorm EULA).

The parent `../CLAUDE.md` holds the mod-agnostic SDK/CrossOver guidance
shared with the sibling mods.

## Build and deploy

```bash
source .envrc           # or, from a worktree: source ../../../.envrc && source .envrc
../utils/build.sh       # Unity batchmode build; on Darwin auto-runs install-macos.sh
                        # from a worktree: ../../../utils/build.sh
```

Unity Editor must be closed (it locks the project). `utils/link.sh` symlinks
the repo's `unity/` mirror into `$SDK_PATH/Assets/`: one **directory** symlink
for `unity/RefillOreBoulders/`, plus the file symlinks for the Assets-level
files beside it (`RefillOreBoulders.asset`, `.asset.meta`, `.meta`).
`build.sh` invokes it idempotently on every run, so worktree switches and
repo moves self-heal.

**Concurrent-build / shared-SDK caveat:** all sibling mods share one
`CoreKeeperModSDK` clone with a single `UnityLockfile`. If another session is
building, wait for the lock to release — do not kill it.

No automated tests — verification is a manual in-game check: run
`/refillboulders` near a mined-down ore boulder and confirm it reports back
to full health, run it again with nothing damaged and confirm it reports
`No damaged ore boulders loaded.`, and confirm `/help refillboulders` shows
the command's description.

## Architecture

Two runtime classes, plus the shared editor helpers symlinked in from
`../utils/`:

- **`RefillOreBouldersMod` (`IMod`)** — bootstrap. `EarlyInit()` loads
  CoreLib's `CommandModule` and registers this mod with it
  (`CommandModule.AddCommands`); CoreLib then discovers
  `RefillBouldersCommand` by scanning the mod's assembly — no explicit
  handler registration call is needed beyond that. `IsHostOrSinglePlayer`
  (`Manager.ecs?.ServerWorld != null`) is the shared guard the command uses
  to refuse running on a pure client.
- **`RefillBouldersCommand` (`IServerCommandHandler`, in `Commands/`)** — the
  whole feature. Builds an `EntityQuery` for
  `ComponentType.ReadOnly<RequiresDrillCD>()` +
  `ComponentType.ReadWrite<HealthCD>()`, walks every matching entity, skips
  ones at `health <= 0` (already mid-destruction — reviving one would leave
  a half-destroyed boulder in an inconsistent state) or already at
  `maxHealth`, and sets the rest to `maxHealth`. `sender` (the invoking
  player entity) is deliberately unused — the command's scope is "everything
  currently loaded", not "everything near the player".

### Why `RequiresDrillCD` needs no ObjectID list

`RequiresDrillCD` is added by exactly one converter,
`DestructibleObjectConverter`, when `DestructibleObjectAuthoring.requiresDrill`
is set on the prefab. Of the 177 prefabs carrying `DestructibleObjectAuthoring`,
exactly 12 set `requiresDrill: 1` — the ten ore boulder types (Copper, Tin,
Iron, Gold, Scarlet, Octarine, Galaxite, Solarite, Pandorium, Relucite) plus
two scene variants. Amber Boulder and Crystal Meteor Boulder are
`DestructibleObject` too but deliberately do *not* set `requiresDrill`, so
they never match. The query therefore already selects exactly and only ore
boulders — a hardcoded ObjectID list would be redundant today and would
silently miss any future ore tier that follows the same authoring pattern.

### Why `IncludeDisabledEntities` is mandatory, not defensive

Core Keeper disables entities beyond `DISTANCE_FROM_PLAYER_TO_UPDATE_ENTITY`
(40 tiles) while keeping them *loaded* out to the player's
`KeepAreaLoadedCD` radius (`KeepLoadedRadius` 300 tiles). Without
`EntityQueryOptions.IncludeDisabledEntities` the query would only ever see
the small fraction of boulders inside that 40-tile update radius, not the
full load bubble — a single real run against a 19-boulder result confirmed
the command reaches far beyond 40 tiles.

### Why direct `EntityManager` writes are safe here

CoreLib's `CommandCommSystem` (which dispatches to registered
`IServerCommandHandler`s) is itself a `PugSimulationSystemBase`, and command
handlers run from inside its `OnUpdate` — i.e. on the `ServerWorld` main
thread, inside the ECS frame, not from some background thread or a Harmony
patch reached through unrelated code paths. Writing component data
(`SetComponentData`) from there is sound; creating or destroying entities
from there would not be, which is why the command tops up health instead of,
say, spawning a fresh boulder entity.

### Why `requiredOn: 0`

This mod changes neither the item database nor the recipe database, so it
creates no client/server divergence to protect against — a `Server` flag
would make the *client* demand the mod on the server, and a `Client` flag
would make the *server* demand it on the client (see the parent
`../CLAUDE.md` § SDK quirks for the full crossed-flag explanation). Either
would needlessly block joining a world that doesn't happen to run this mod.
`None` is correct: everyone can play together regardless of who has it
installed, and the command simply won't exist for those who don't.

### Scaffolding gotcha: CoreLib is not on the generated asmdef

This mod was scaffolded with `utils/new_mod.py --corelib`. That flag adds the
CoreLib *loader dependency* to the ModBuilderSettings `.asset`
(`dependencies: - modName: CoreLib`), which is what makes the loader require
CoreLib to be present — but the generated runtime `.asmdef` is emitted from a
fixed reference list that has no CoreLib entry. Without adding `"CoreLib"` to
the asmdef's `references` by hand, `using CoreLib.Submodule.Command;` fails
to compile with `CS0246`.

`unity/` is the canonical source — a 1:1 mirror of the SDK's `Assets/` tree
holding every file the Editor generates for the mod: the `.cs` sources, both
`.asmdef` files, the ModBuilderSettings `.asset`, and all `.meta` GUID
carriers.

## macOS / CrossOver

Deployed through the fake-mod.io workaround (see parent `../CLAUDE.md`). This
mod's fake mod.io ID is **`9999989`** (siblings use the other IDs in the
`9999990`..`9999999` block; they must differ). Do not open the in-game Mods
menu while a fake-ID install is active; re-run `../utils/build.sh` to restore
if the cache is wiped.

## Publishing to mod.io

Not yet published — the real mod ID in
`unity/RefillOreBoulders/Editor/RefillOreBoulders_modio.asset` is still `0`.
When publishing, `../utils/upload.sh` uses the shared
`CoreKeeperModUtils.CLIPublishHelper.Publish` Editor class the same way as
every sibling mod: the version comes from the topmost `## [x.y.z]` entry of
`CHANGELOG.md`, and the profile logo is
`unity/RefillOreBoulders/Editor/logo.png` (already in place). Set the mod.io
profile type tag to **`Script`** (an `Asset` tag silently disables the mod's
scripts).

## Conventions

- Commit messages: Conventional Commits (`type(scope): subject`), imperative,
  no emoji.
- Documentation files (`CLAUDE.md`, `README.md`, `docs/`) are English; chat
  answers are German.
- Prefer `git commit --amend` / `git reset --soft` over fix-up commits on a
  personal branch, and `git rebase` over `git merge`.
