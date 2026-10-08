# iScrap Mod-Pack — GTA IV Complete Edition

Mod-Pack + Launcher fuer GTA IV Complete Edition (1.2.0.59, Steam).

## Installation (fuer Spieler)

1. `launcher/Launcher.exe` starten.
2. Der Launcher findet das Spiel automatisch (Steam). Falls nicht: **Pfad...** klicken
   und den GTA IV-Ordner (enthält `GTAIV.exe`) waehlen.
3. **Update pruefen & installieren** klicken — der Launcher laedt das Pack, entfernt
   alte Mod-Reste und installiert sauber. **Deine Spielstaende und Einstellungen
   bleiben erhalten** (Schutzliste `keep.txt`).
4. **Spiel starten.**

Voraussetzung: installiertes GTA IV CE (Steam) + [Git](https://git-scm.com) fuer Updates.

## Was das Pack enthaelt (Kern)

- iScrap/PhonePlus (Eigenentwicklung) + gepatchte Versionen der genutzten Mods
  (Business Complete, Jewellery & Pawn, Hideout Cash Storage, Liberty City Dealership)
- Liberty Loadout, FusionFix, Project2DFX-LOD-Lights, ScriptHookDotNet (CE)

## Fuer Maintainer

- `version.txt` = Pack-Version (Datum-basiert). Nach Aenderungen: Version hochsetzen,
  commit + push. Der Launcher vergleicht sie mit `scripts\pack_version.txt` im Spiel.
- `uninstall.txt` = Dateien, die der Launcher vor der Installation entfernt.
- `keep.txt` = Schutzliste (Saves/Konfiguration, wird nie angefasst).
- `modpack/` = die Dateien, die ins Spielverzeichnis kopiert werden.
- Repo-URL/Branch: `launcher.ini` next to Launcher.exe.

## Hinweis

Dieses Repo enthaelt Community-Mods (Scottyus1, Prof. Farnsworth) in gepatchter Form.
Bitte die Re-Upload-Regeln der Original-Autoren beachten; bei Bedarf verschiebt man die
Fremd-Dateien in einen eigenen Download und haelt `modpack/` schlank.
