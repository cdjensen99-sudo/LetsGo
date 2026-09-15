# Changelog

## 1.0.0 — Valheim 1.0

- Requires **Valheim 1.0** (tested on 1.0.7+) and BepInEx Pack 5.4.2350 (Unity 6).
- First release of LetsGo: Follow, Stay, pack call, portal-follow, and vanilla cart hitch, extracted from Beast Hird.
- **Follow / Stay** on any tamed creature — MonsterAI and AnimalAI, adults and young (wolves, boars, lox, Asksvin, hens, cubs, calves, chicks, piglets, hatchlings).
- Size-aware follow distance so large beasts actually walk instead of standing on the vanilla short leash.
- **T** pack call reaches Stay tames in the open. Tames enclosed by player-built structures (fences, walls, barns, gates) do not answer.
- Vanilla cart hitch for **adult boar-sized or larger** tames. Hens and young never pull. Pull strength scales with the animal's radius.
- Large pullers (lox, moose, Asksvin) hitch behind their mesh; cart/puller collision is ignored while hitched so the vanilla cart is not crushed.
- Following tames travel vanilla portals with you (skipped if TeleportEverything or ServersideQoL TeleportFollow is loaded).
- **Multiplayer:** commands run on the peer that has the animals in their active zone. The host does not need to stand nearby. Dedicated and listen-server RPCs bounce back to the requesting client.
