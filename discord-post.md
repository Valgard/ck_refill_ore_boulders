# Refill Ore Boulders

Ore boulders are gone for good once their health hits zero. This adds one chat
command, **`/refillboulders`**, that tops every loaded ore boulder back up to
full — so a vein you have been chipping at keeps giving instead of running out.

It repairs, it does not make anything indestructible: mining and drills wear
boulders down exactly as before, and you run the command again whenever you
want. It reports how many it refilled, leaves full ones alone, and never
revives one that is already being destroyed.

Covers Copper, Tin, Iron, Gold, Scarlet, Octarine, Galaxite, Solarite,
Pandorium and Relucite. Amber and Crystal Meteor Boulders are not ore boulders
and are untouched.

## Good to know

The command only reaches boulders loaded around you — roughly the range the
game keeps chunks in. Anything further out is simply not there yet.

## Requirements

**CoreLib**, offered when you subscribe.

The command runs wherever **the server** has the mod — a dedicated server, or
your own world in solo. On a client connecting to a server without it, the game
just replies that the command does not exist. Joining worlds without the mod is
never blocked.
