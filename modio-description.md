# Refill Ore Boulders

**Never abandon a half-mined ore vein again.**

Ore boulders — the big Copper, Tin, Iron, Gold, Scarlet, Octarine, Galaxite,
Solarite, Pandorium and Relucite chunks you drill or pickaxe for ore — are
destroyed for good once their health hits zero. This mod adds one chat
command that tops every ore boulder around you back up to full, so a boulder
you've been chipping away at keeps giving instead of running out.

## What it does

- Adds a chat command, `/refillboulders`, that restores every currently
  loaded ore boulder to full health in one go.
- Reports how many boulders it refilled, or tells you there was nothing to
  refill.
- Leaves already-full boulders untouched, and never revives a boulder that
  is already in the process of being destroyed.

## Good to know

- The command repairs boulders — it does not make them indestructible.
  Mining and drills keep wearing them down exactly as in vanilla; run the
  command again whenever you want to top them off.
- It only reaches boulders that are actually loaded around you — roughly the
  same range the game keeps chunks loaded in. Boulders far outside that
  range simply aren't there to refill yet.
- Only the host of the world (a dedicated server, or you in singleplayer)
  can run the command.
- Amber Boulders and Crystal Meteor Boulders aren't ore boulders and are not
  affected.

## Requirements

Requires **CoreLib** — mod.io will prompt you to install it when you
subscribe.

---

*Built with the official Pugstorm Core Keeper Mod SDK. Personal-use,
non-commercial (Core Keeper EULA). Not affiliated with or endorsed by
Pugstorm.*
