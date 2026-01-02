using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using System;
using System.IO;
using System.Text;
using System.Xml;

class RecoverGame : Game
{
    GraphicsDeviceManager _graphics;
    ContentManager _content;

    static string XnbPath = "";

    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: XnbTiledRecover <file.xnb | *.xnb>");
            return;
        }

        var xnbFiles = new List<string>();

        foreach (var arg in args)
        {
            if (arg.Contains('*') || arg.Contains('?'))
            {
                var dir = Path.GetDirectoryName(arg);
                if (string.IsNullOrEmpty(dir))
                    dir = Directory.GetCurrentDirectory();

                var pattern = Path.GetFileName(arg);

                xnbFiles.AddRange(Directory.GetFiles(dir, pattern));
            }
            else if (arg.EndsWith(".xnb", StringComparison.OrdinalIgnoreCase))
            {
                xnbFiles.Add(Path.GetFullPath(arg));
            }
        }

        if (xnbFiles.Count == 0)
        {
            Console.WriteLine("No .xnb files found.");
            return;
        }

        foreach (var xnb in xnbFiles)
        {
            Console.WriteLine($"Processing: {xnb}");
            RunOnce(xnb);
        }
    }
    static void RunOnce(string xnbPath)
    {
        XnbPath = xnbPath;
        using var g = new RecoverGame();
        g.Run();
    }

    public RecoverGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = Path.GetDirectoryName(XnbPath)!;
        IsMouseVisible = false;
    }

    protected override void LoadContent()
    {
        _content = Content;
        var asset = Path.GetFileNameWithoutExtension(XnbPath);

        try
        {
            var map = _content.Load<TiledMap>(asset);

            var outPath = Path.ChangeExtension(XnbPath, ".tmx");
            WriteTmx(map, outPath);

            Console.WriteLine($"Recovered -> {outPath}");
        }
        catch (InvalidCastException)
        {
            Console.WriteLine($"[SKIP] Not a TiledMap: {XnbPath}");
        }
        catch (ContentLoadException e)
        {
            Console.WriteLine($"[ERROR] Failed to load: {XnbPath}");
            Console.WriteLine($"        {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[ERROR] Unexpected error: {XnbPath}");
            Console.WriteLine(e);
        }
        finally
        {
            Exit(); // ← 重要：必ず次へ進む
        }
    }

    static void WriteObjectLayer(
        XmlWriter w,
        TiledMapObjectLayer layer,
        Dictionary<TiledMapTileset, int> firstGids)
    {
        w.WriteStartElement("objectgroup");

        w.WriteAttributeString("name", layer.Name);
        w.WriteAttributeString("opacity", layer.Opacity.ToString(System.Globalization.CultureInfo.InvariantCulture));
        w.WriteAttributeString("visible", layer.IsVisible ? "1" : "0");

        WriteProperties(w, layer.Properties);

        foreach (var obj in layer.Objects)
        {
            WriteObject(w, obj, firstGids); // ★ 修正
        }

        w.WriteEndElement();
    }

    static Dictionary<TiledMapTileset, int> BuildFirstGidMap(TiledMap map)
    {
        var dict = new Dictionary<TiledMapTileset, int>();
        int firstGid = 1;

        foreach (var ts in map.Tilesets)
        {
            dict[ts] = firstGid;
            firstGid += ts.TileCount;
        }

        return dict;
    }

    static void WriteObject(
        XmlWriter w,
        TiledMapObject obj,
        Dictionary<TiledMapTileset, int> firstGids)
    {
        w.WriteStartElement("object");

        w.WriteAttributeString("id", obj.Identifier.ToString());

        if (!string.IsNullOrEmpty(obj.Name))
            w.WriteAttributeString("name", obj.Name);

        if (!string.IsNullOrEmpty(obj.Type))
            w.WriteAttributeString("type", obj.Type);

        w.WriteAttributeString("x", obj.Position.X.ToString(System.Globalization.CultureInfo.InvariantCulture));
        w.WriteAttributeString("y", obj.Position.Y.ToString(System.Globalization.CultureInfo.InvariantCulture));

        if (obj.Rotation != 0)
            w.WriteAttributeString("rotation", obj.Rotation.ToString(System.Globalization.CultureInfo.InvariantCulture));

        w.WriteAttributeString("visible", obj.IsVisible ? "1" : "0");

        switch (obj)
        {
            // --- Tile Object ---
            case TiledMapTileObject tileObj:
                {
                    int firstGid = firstGids[tileObj.Tileset];
                    int gid = firstGid + tileObj.Tile.LocalTileIdentifier;

                    w.WriteAttributeString("gid", gid.ToString());
                    break;
                }

            // --- Rectangle ---
            case TiledMapRectangleObject:
                w.WriteAttributeString("width", obj.Size.Width.ToString(System.Globalization.CultureInfo.InvariantCulture));
                w.WriteAttributeString("height", obj.Size.Height.ToString(System.Globalization.CultureInfo.InvariantCulture));
                break;

            // --- Ellipse ---
            case TiledMapEllipseObject:
                w.WriteAttributeString("width", obj.Size.Width.ToString(System.Globalization.CultureInfo.InvariantCulture));
                w.WriteAttributeString("height", obj.Size.Height.ToString(System.Globalization.CultureInfo.InvariantCulture));
                w.WriteStartElement("ellipse");
                w.WriteEndElement();
                break;

            // --- Polygon ---
            case TiledMapPolygonObject poly:
                WritePoints(w, "polygon", poly.Points);
                break;

            // --- Polyline ---
            case TiledMapPolylineObject line:
                WritePoints(w, "polyline", line.Points);
                break;

            default:
                Console.WriteLine($"[WARN] Unknown object type: {obj.GetType().Name}");
                break;
        }

        WriteProperties(w, obj.Properties);

        w.WriteEndElement(); // object
    }

    static void WritePoints(XmlWriter w, string element, Point2[] points)
    {
        w.WriteStartElement(element);

        var sb = new StringBuilder();
        for (int i = 0; i < points.Length; i++)
        {
            if (i > 0) sb.Append(' ');
            sb.Append(points[i].X.ToString(System.Globalization.CultureInfo.InvariantCulture));
            sb.Append(',');
            sb.Append(points[i].Y.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        w.WriteAttributeString("points", sb.ToString());
        w.WriteEndElement();
    }

    static void WriteProperties(XmlWriter w, TiledMapProperties props)
    {
        if (props == null || props.Count == 0)
            return;

        w.WriteStartElement("properties");

        foreach (var kv in props)
        {
            w.WriteStartElement("property");
            w.WriteAttributeString("name", kv.Key);
            w.WriteAttributeString("value", kv.Value?.ToString() ?? "");
            w.WriteEndElement();
        }

        w.WriteEndElement(); // properties
    }

    static void WriteTmx(TiledMap map, string path)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            Encoding = Encoding.UTF8
        };

        using var w = XmlWriter.Create(path, settings);

        w.WriteStartDocument();
        w.WriteStartElement("map");

        w.WriteAttributeString("orientation", map.Orientation.ToString().ToLower());
        w.WriteAttributeString("renderorder", "right-down");
        w.WriteAttributeString("width", map.Width.ToString());
        w.WriteAttributeString("height", map.Height.ToString());
        w.WriteAttributeString("tilewidth", map.TileWidth.ToString());
        w.WriteAttributeString("tileheight", map.TileHeight.ToString());
        w.WriteAttributeString("infinite", "0");

        // tilesets
        // tilesets
        int firstGid = 1;

        foreach (var ts in map.Tilesets)
        {
            w.WriteStartElement("tileset");
            w.WriteAttributeString("firstgid", firstGid.ToString());
            w.WriteAttributeString("name", ts.Name);
            w.WriteAttributeString("tilewidth", ts.TileWidth.ToString());
            w.WriteAttributeString("tileheight", ts.TileHeight.ToString());
            w.WriteAttributeString("tilecount", ts.TileCount.ToString());
            w.WriteAttributeString("columns", ts.Columns.ToString());

            w.WriteStartElement("image");
            w.WriteAttributeString("source", ts.Name + ".png"); // 仮
            w.WriteAttributeString("width", ts.Texture.Width.ToString());
            w.WriteAttributeString("height", ts.Texture.Height.ToString());
            w.WriteEndElement();

            w.WriteEndElement();

            firstGid += ts.TileCount;
        }

        // layers
        foreach (var layer in map.TileLayers)
        {
            w.WriteStartElement("layer");
            w.WriteAttributeString("name", layer.Name);
            w.WriteAttributeString("width", layer.Width.ToString());
            w.WriteAttributeString("height", layer.Height.ToString());

                w.WriteStartElement("data");
                w.WriteAttributeString("encoding", "csv");

                w.WriteString(string.Join(",",
                    layer.Tiles.Select(t => t.GlobalIdentifier)));

                w.WriteEndElement(); // data
            w.WriteEndElement(); // layer
        }

        // object layers
        var firstGids = BuildFirstGidMap(map);

        foreach (var objLayer in map.ObjectLayers)
        {
            WriteObjectLayer(w, objLayer, firstGids);
        }

        w.WriteEndElement();
        w.WriteEndDocument();
    }
}
