using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
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
        if (args.Length != 1 || !args[0].EndsWith(".xnb", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Usage: RecoverGame <map.xnb>");
            return;
        }

        XnbPath = Path.GetFullPath(args[0]);
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
        var map = _content.Load<TiledMap>(asset);

        var outPath = Path.ChangeExtension(XnbPath, ".tmx");
        WriteTmx(map, outPath);

        Console.WriteLine($"Recovered -> {outPath}");
        Exit();
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
            w.WriteString(string.Join(",", layer.Tiles));
            w.WriteEndElement();

            w.WriteEndElement();
        }

        w.WriteEndElement();
        w.WriteEndDocument();
    }
}
