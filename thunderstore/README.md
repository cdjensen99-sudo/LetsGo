# LetsGo

Valheim BepInEx mod: **Follow / Stay** on any tamed creature, pack call, portal-follow, and hitch the vanilla cart.

**Requires:** Valheim 1.0 and [BepInEx Pack for Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/) 5.4.2350 (Unity 6).

## Controls

| Input | Action |
|---|---|
| **E** on a tame | Cycle **Follow** / **Stay** |
| **Shift+E** | Vanilla pet / rename |
| **T** | Call nearby tames that are **not inside a pen** |
| **Y** | Nearby tames that are **Following you** Stay (no pen check) |
| Use a cart | Hitch the nearest eligible adult tame, then the player |

## Follow

If it is tamed, it can follow — wolves, boars, lox, Asksvin, hens, cubs, calves, chicks, piglets, and hatchlings. MonsterAI and AnimalAI both work. Stop distance scales with the animal's radius so lox and Asksvin actually walk.

## Pack call

**T** calls nearby Stay tames in the open. Tames enclosed by **player-built structures** (fences, walls, barns, gates — not only fences) do not answer. Standing along a wall with open sides still answers. Set `PackCall.IgnorePenned` to false to call everyone in range.

**Y** tells nearby tames that are Following you to Stay. Pens are ignored. Range and cap match pack call (`PackCall.Radius` / `PackCall.MaxTames`).

## Carts

Adult **boar-sized or larger** tames can pull. Hens cannot. Young cannot, even large calves. Pull strength scales with the adult's radius. If no eligible tame is in range, you pull the cart yourself.

Large animals hitch farther behind their body. The cart does not collide with the animal pulling it, so a lox/moose/Asksvin should not grind the cart to pieces. No oversized cart piece.

## Portal-follow

Tames currently Following you within range of a vanilla portal come through. Carts detach first. Skipped if TeleportEverything or ServersideQoL `TeleportFollow` is loaded.

## Compatibility

Install on **server/host and every client**.

**Disable these:** Beast Hird (same follow patches), Tames Follow, AsksvinImproved, BetterTames, FollowDistance, WolfPack, Beasts of Burden.

If **OffspringPortal** is loaded, turn off its `EnableFollowCommand` so juvenile E is not double-handled.

Pick one portal system: TeleportEverything ally teleport **or** this mod's portal-follow. ServersideQoL `TeleportFollow` already warps distant followers — we skip ours if that pack is loaded.

## Config (`BepInEx/config/hardwire99.letsgo.cfg`)

| Key | Default | Notes |
|---|---|---|
| EnableFollow | true | Follow / Stay |
| EnableCarts | true | Hitch vanilla carts |
| EnablePackCall | true | T hotkey |
| EnablePackStay | true | Y hotkey: Following tames Stay |
| TameNameMaxLength | 100 | Rename character limit (vanilla 10) |
| EnablePortalFollow | true | Following tames through vanilla portals |
| FollowDistanceFactor | 3 | Stop = max(MediumFollowDistance, radius × this) |
| MediumFollowDistance | 3 | Minimum follow stop |
| Carts.MinRadius | 0.45 | Adult boar is the floor |
| Carts.HitchBackPadding | 1.25 | Extra meters behind the animal's rear |
| Carts.IgnorePullerCollision | true | Cart does not physically hit the animal pulling it |
| PackCall.IgnorePenned | true | Skip tames enclosed by player-built structures |
| PackCall.PenCheckDistance | 6 | How far to look for enclosing structures |
| PackCall.PenHitMin | 5 | Penned if this many of 8 directions hit a player piece |

## Install

Install on **every client**. LetsGo runs on the peer that has the animals in their **active zone** — the host does not need to stand nearby. Dedicated/listen-server RPCs bounce back to the player who issued the command, same pattern as Offspring Portal.

Place `LetsGo.dll` in `BepInEx/plugins/` (Gale folder `Hardwire99-LetsGo`).
