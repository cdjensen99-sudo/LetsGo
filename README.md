# LetsGo

Valheim BepInEx mod: **Follow / Stay** on any tamed creature, pack call, portal-follow, and hitch the vanilla cart.

**Valheim 1.0 compatible** (Unity 6 / BepInExPack Valheim 5.4.2350).

Source: https://github.com/cdjensen99-sudo/LetsGo

Follow logic was extracted from Beast Hird / Training. Do **not** run both mods together.

## Controls

| Input | Action |
|---|---|
| **E** on a tame | Cycle **Follow** / **Stay** |
| **Shift+E** | Vanilla pet / rename |
| **T** | Call nearby tames that are **not inside a pen** |
| Use a cart | Hitch the nearest eligible adult tame, then the player |

## Follow

If it is tamed, it can follow — MonsterAI and AnimalAI, adults and young (cubs, calves, chicks, piglets, hatchlings, hens). Stop distance scales with radius so lox and Asksvin walk instead of standing on the vanilla short leash.

## Pack call

**T** calls Stay tames in the open. Tames enclosed by **player-built structures** (fences, walls, barns, gates) do not answer. A tame standing along a wall with open sides still answers.

## Carts

Adult boar-sized or larger tames can pull. Hens and young never pull. Joint break force scales with the adult's radius. Player pull remains the fallback.

The vanilla cart stays vanilla-sized. Large pullers (lox, moose, Asksvin) hitch farther behind their mesh so the cart is not under them, and physics between the cart and that animal is ignored while hitched so body clips do not damage the cart.

## Portal-follow

Following tames near a vanilla portal come with you. Carts detach first.

## Build

```powershell
.\build.ps1 -Deploy
```

Output: `artifacts\LetsGo.dll`, copied to the Gale **New Release** profile when that folder exists.

## Gale New Release smoke test

Disable Beast Hird, Tames Follow, AsksvinImproved, BetterTames, FollowDistance, WolfPack, and Beasts of Burden. If OffspringPortal is loaded, set `EnableFollowCommand = false`.

- Wolf / boar / lox / Asksvin: E Follow, they walk; E Stay.
- Hen: E Follow and she walks; Shift+E still pets.
- Chick, piglet, cub, calf, hatchling: E Follow and they walk.
- T: Stay tames in the open answer. Stay tames inside a fenced or walled pen do not. Tames along a fence line still answer.
- Cart: adult boar hitch. Lox/moose/Asksvin hitch farther behind and should not smash the cart. Hen / young do not hitch (player pull still works).
- Portal: a Following tame comes through.

Log line: `LetsGo 1.0.0 loaded (valheim-1.0, dll ...)`.

## Multiplayer

Install on the **dedicated server or listen host and every client**. Commands run on whoever has the tames loaded — you do **not** need the host standing in the area. Dedicated-server RPCs bounce back to the player who issued the command.

## Config

`BepInEx/config/hardwire99.letsgo.cfg`

See [thunderstore/README.md](thunderstore/README.md) for the key list.
