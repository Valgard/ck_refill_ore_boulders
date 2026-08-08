# Refill Ore Boulders

A small Core Keeper mod that **restores ore boulders you've mined down**.

It adds one chat command, `/refillboulders`, which tops every currently
loaded ore boulder back up to full health via a `RequiresDrillCD` +
`HealthCD` entity query — an ore boulder has no separate yield counter, its
remaining ore *is* its health, so refilling the health refills the boulder.

Personal-use, non-commercial (Pugstorm EULA).

## Install

- **mod.io:** subscribe to the mod; Core Keeper downloads it on next launch.
- **Local build:** see `CLAUDE.md` → *Build and deploy*.

## Usage

Type `/refillboulders` in chat. The command reports how many boulders it
refilled, or tells you there was nothing to do:

```
Refilled 19 ore boulder(s).
No damaged ore boulders loaded.
```

Anyone connected can type it — a client's command travels to the server by
RPC and runs there, regardless of who sent it. What actually has to be true
is that **the server** (the dedicated server, or your own world if you're
playing solo) has this mod installed; if it doesn't, CoreLib answers with
its own `Command refillboulders does not exist!` instead of running
anything.

**The command repairs; it does not prevent consumption.** Boulders keep
wearing down from mining and drills exactly as before — running the command
again is how you top them back up. Making boulders permanently indestructible
is a different mod's job.

**Reach is limited to loaded chunks.** The command only sees entities inside
the player's chunk-load bubble, out to roughly 300 tiles — a boulder outside
that radius isn't loaded as an entity at all, so there's nothing to refill.

## How it works

See `CLAUDE.md` for the full architecture. In short: a CoreLib server command
handler queries every entity with `RequiresDrillCD` (the component that marks
an ore boulder, added to nothing else) and `HealthCD`, skips boulders that are
already full or already at zero health, and sets the rest back to their max.
