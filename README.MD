# XnbTiledRecover

**XnbTiledRecover** is a command-line tool that recovers **Tiled (.tmx)** maps from
**MonoGame / MonoGame.Extended TiledMap XNB files**.

This tool is intended for **one-time recovery or inspection** of Tiled maps that were
compiled into `.xnb` format using **MonoGame.Extended**.

---

## Features

* Load `.xnb` files built with **MonoGame.Extended.Tiled**
* Recover map data into standard **Tiled `.tmx` (XML)** format
* Supports:

  * Map size, orientation, tile size
  * Tilesets (basic information)
  * Tile layers (CSV encoding)
* Wildcard input support (`*.xnb`)
* Skips non-TiledMap XNB files and continues processing

---

## Requirements

### Runtime

* **.NET 8.0 or later**
* **MonoGame DesktopGL**
* **MonoGame.Extended (same or compatible version used when building the XNB)**

> ⚠️ The MonoGame.Extended version **must match** (or be very close to)
> the version used when the XNB file was originally built
> (for example: maps built around October 2022 usually work with `MonoGame.Extended 3.8.x`).

---

## Required NuGet Packages

```powershell
dotnet add package MonoGame.Framework.DesktopGL
dotnet add package MonoGame.Extended
dotnet add package MonoGame.Extended.Tiled
```

---

## Usage

### Single file

```powershell
XnbTiledRecover.exe map.xnb
```

### Multiple files (wildcard)

```powershell
XnbTiledRecover.exe *.xnb
```

Each valid **TiledMap XNB** will be converted to:

```text
map.xnb → map.tmx
```

---

## Behavior for Non-Tiled XNB Files

If the XNB file does **not** contain a `TiledMap`
(e.g. Texture2D, SpriteFont, SoundEffect):

* An error message is printed
* Processing continues with the next file
* The program does **not** terminate

Example:

```text
Processing: enemy.png.xnb
Error: This XNB is not a TiledMap. Skipped.
```

---

## Output Notes

* Tile layer data is exported as **CSV**
* Tile Global IDs are written using `GlobalIdentifier`
* Tileset image filenames are generated heuristically
  (manual adjustment may be required)
* Object layers, properties, and custom metadata are **not fully restored**

This tool focuses on **map structure recovery**, not perfect reconstruction.

---

## Typical Use Cases

* Recover lost `.tmx` maps from compiled games
* Inspect Tiled maps embedded in XNB files
* Migrate old MonoGame projects
* Debug or analyze legacy content pipelines

---

## Limitations

* Requires MonoGame runtime (not a pure console app)
* Tileset image paths may need manual fixing
* Exact original tileset ordering may differ
* Some advanced Tiled features are not exported

---

## License

This project is licensed under the **MIT License**.

You are free to use, modify, and distribute this software,
including for commercial purposes.

### Important

> Recovered `.tmx` files and game assets are **not covered** by this license.
> The copyright of recovered content belongs to the original rights holders.

---

## Disclaimer

This tool is provided **"AS IS"**, without warranty of any kind.
The author is not responsible for any damage or data loss caused by using this tool.