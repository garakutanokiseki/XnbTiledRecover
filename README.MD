# XnbTiledRecover

MonoGame / MonoGame.Extended でビルドされた
**TiledMap（.tmx）由来の .xnb ファイルを復元するツール**です。

`.xnb` から **Tiled (.tmx) 形式**を再生成することを目的としています。

---

## 概要

* MonoGame + MonoGame.Extended.Tiled でビルドされた **TiledMap.xnb** を入力
* 中身が **TiledMap の場合のみ** `.tmx` を出力
* Texture2D など **TiledMap 以外の .xnb はスキップ**
* ワイルドカード（`*.xnb`）対応
* コンソールツールとして実行可能

---

## 特徴

* MonoGame の **ContentManager を直接使用**
* XNB 内部の `TiledMapReader` を利用した **正規ロード**
* Tiled が読み込める **CSV エンコード形式でレイヤーを書き出し**
* エラーが発生しても **処理を継続**

---

## 使い方

### 1️⃣ 単一ファイル

```powershell
XnbTiledRecover.exe stage01.xnb
```

### 2️⃣ ワイルドカード指定

```powershell
XnbTiledRecover.exe *.xnb
```

```powershell
XnbTiledRecover.exe maps\*.xnb
```

---

## 実行結果例

```text
Processing: tileset01.xnb
[SKIP] Not a TiledMap: tileset01.xnb

Processing: stage01.xnb
Recovered -> stage01.tmx

Processing: background.xnb
[SKIP] Not a TiledMap: background.xnb
```

---

## 出力されるファイル

* 入力と同じディレクトリに `.tmx` を生成
* ファイル名は `.xnb` を `.tmx` に置換

```text
stage01.xnb → stage01.tmx
```

---

## 対応している要素

### ✅ 対応済み

* Map 情報

  * orientation
  * width / height
  * tilewidth / tileheight
* Tileset

  * タイルサイズ
  * タイル数
  * カラム数
* TileLayer

  * CSV エンコード
  * GlobalTileID（GID）

### ❌ 未対応（今後の拡張余地）

* ObjectLayer
* ImageLayer
* タイルの flip / rotate フラグ
* Tileset の正確な画像パス（現在は仮）

---

## 必要な環境

### ランタイム

* **.NET 8 以上**
* Windows（MonoGame DesktopGL）

### NuGet パッケージ

```powershell
dotnet add package MonoGame.Framework.DesktopGL
dotnet add package MonoGame.Extended
dotnet add package MonoGame.Extended.Tiled
```

※ **XNB をビルドしたときと同じ MonoGame.Extended 系バージョン**を使用してください
（Reader の不一致を防ぐため）

---

## ビルド時の注意点

* このツールは **MonoGame の Game クラスを使用**
* GraphicsDevice が必要なため、**完全な純コンソールではありません**
* `Game.Run()` → `LoadContent()` → `Exit()` という **1回実行構成**

---

## ソースコード構成

### Main / RunOnce

* コマンドライン引数を解析
* ワイルドカード展開
* `.xnb` を1つずつ処理

```csharp
foreach (var xnb in xnbFiles)
{
    RunOnce(xnb);
}
```

---

### LoadContent（中核処理）

```csharp
try
{
    var map = _content.Load<TiledMap>(asset);
    WriteTmx(map, outPath);
}
catch (InvalidCastException)
{
    // TiledMap 以外（Texture2D 等）
}
finally
{
    Exit();
}
```

* **TiledMap でない XNB を安全にスキップ**
* MonoGame 特有の例外構造に対応

---

### WriteTmx

* Tiled がそのまま開ける XML を生成
* レイヤーデータは CSV 形式

```xml
<data encoding="csv">
1,2,3,4,...
</data>
```

---

## なぜ MonoGame が必要なのか？

`.xnb` は **MonoGame/XNA の Content Pipeline 専用フォーマット**であり、

* 独自 Reader（`TiledMapReader`）
* 圧縮・型情報

を含むため、

👉 **MonoGame を使わずに完全復元することは現実的ではありません**

本ツールは **最も確実な方法**を採用しています。

---

## 制限事項

* XNB のビルド環境と **互換性のある MonoGame.Extended が必須**
* 古すぎる / 改変された XNB はロードできない可能性あり
* Tileset の画像パスは手動修正が必要な場合あり

---

## ライセンス

本ツール **XnbTiledRecover** は **MIT License** のもとで公開されています。

- 商用・非商用を問わず利用可能
- 改変・再配布可能
- 本ソフトウェアの利用により生じたいかなる損害についても、作者は責任を負いません

※ 本ツールは以下のライブラリを使用しています。  
それぞれのライセンスは各プロジェクトに従ってください。

- MonoGame
- MonoGame.Extended
