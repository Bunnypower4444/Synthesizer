
using System.Diagnostics;

namespace Synthesizer;

public static class Assets
{
    // public const float FontSize = Calculator.RelativeScale * 16;
	public const string AssetFolder = "Content";

	private static string? contentPath = null;

	public static string ContentPath
	{
		get
		{
			if (contentPath == null)
			{
				var baseFolder = AppContext.BaseDirectory;
				var searchUpPath = "";
				int up = 0;
				while (!Directory.Exists(Path.Join(baseFolder, searchUpPath, AssetFolder)) && up++ < 5)
					searchUpPath = Path.Join(searchUpPath, "..");
				if (!Directory.Exists(Path.Join(baseFolder, searchUpPath, AssetFolder)))
					throw new Exception($"Unable to find {AssetFolder} Directory from '{baseFolder}'");
				contentPath = Path.Join(baseFolder, searchUpPath, AssetFolder);
			}

			return contentPath;
		}
	}

	/* public static readonly Dictionary<string, Shader> Shaders = new(StringComparer.OrdinalIgnoreCase);
	public static readonly Dictionary<string, Texture> Textures = new(StringComparer.OrdinalIgnoreCase);
	// public static readonly Dictionary<string, SkinnedTemplate> Models = new(StringComparer.OrdinalIgnoreCase);
	public static readonly Dictionary<string, Subtexture> Subtextures = new(StringComparer.OrdinalIgnoreCase);
	public static readonly Dictionary<string, Font> Fonts = new(StringComparer.OrdinalIgnoreCase);
	// public static readonly Dictionary<string, Language> Languages = new(StringComparer.OrdinalIgnoreCase);
	private static SpriteFont? mainFont;
	public static SpriteFont MainFont { get {
			if (mainFont == null)
				return mainFont = new SpriteFont(Fonts["Renogare"], FontSize, SpriteFont.Ascii);
			return mainFont;
		}
	} */
	public static readonly Dictionary<string, Soundfont> Soundfonts = new(StringComparer.OrdinalIgnoreCase);

    public static bool IsLoaded { get; private set; } = false;
	public static void Load(Action? callback = null)
	{
		var timer = Stopwatch.StartNew();
        IsLoaded = false;

		Soundfonts.Clear();
		/*Shaders.Clear();
		Textures.Clear();
		Subtextures.Clear();
		// Models.Clear();
		Fonts.Clear();
		// Languages.Clear();
		// Audio.Unload();

		// var images = new ConcurrentBag<(string, Image)>();
		// var models = new ConcurrentBag<(string, SkinnedTemplate)>();
		// var langs = new ConcurrentBag<Language>();*/
		var tasks = new List<Task>();

		// load soundfonts
		var soundfontPath = Path.Join(ContentPath, "Soundfonts");
		foreach (var file in Directory.EnumerateFiles(soundfontPath, "*.sf2", SearchOption.AllDirectories))
		{
			var name = GetResourceName(soundfontPath, file);
			tasks.Add(Task.Run(() =>
			{
				var soundfont = Soundfont.LoadFile(file);
				Soundfonts.Add(name, soundfont);
			}));
		}

		// load texture pngs
		/* var texturesPath = Path.Join(ContentPath, "Textures");
		foreach (var file in Directory.EnumerateFiles(texturesPath, "*.png", SearchOption.AllDirectories))
		{
			var name = GetResourceName(texturesPath, file);
			tasks.Add(Task.Run(() =>
			{
				var img = new Image(file);
				img.Premultiply();
				images.Add((name, img));
			}));
		} */


		// load glb models
		/* var modelPath = Path.Join(ContentPath, "Models");
		foreach (var file in Directory.EnumerateFiles(modelPath, "*.glb", SearchOption.AllDirectories))
		{
			var name = GetResourceName(modelPath, file);

			tasks.Add(Task.Run(() =>
			{
				var input = SharpGLTF.Schema2.ModelRoot.Load(file);
				var model = new SkinnedTemplate(input);
				models.Add((name, model));
			}));
		} */

		// load languages
		/* var textPath = Path.Join(ContentPath, "Text");
		foreach (var file in Directory.EnumerateFiles(textPath, "*.json", SearchOption.AllDirectories))
		{
			tasks.Add(Task.Run(() =>
			{
				var data = File.ReadAllText(file);
				if (JsonSerializer.Deserialize(data, LanguageContext.Default.Language) is {} lang)
					langs.Add(lang);
			}));
		} */

		// load audio
		// Audio.Load(Path.Join(ContentPath, "Audio"));

		// load glsl shaders
		/*var shadersPath = Path.Join(ContentPath, "Shaders");
		foreach (var file in Directory.EnumerateFiles(shadersPath, "*.glsl"))
		{
			if (LoadShader(file) is Shader shader)
			{
				shader.Name = GetResourceName(shadersPath, file);
				Shaders[shader.Name] = shader;
			}
		}

		// load font files
		 var fontsPath = Path.Join(ContentPath, "Fonts");
		foreach (var file in Directory.EnumerateFiles(fontsPath, "*.*", SearchOption.AllDirectories))
			if (file.EndsWith(".ttf") || file.EndsWith(".otf"))
				Fonts.Add(GetResourceName(fontsPath, file), new Font(file));
 
		// pack sprites into single texture
		/* {
			var packer = new Packer
			{
				Trim = false,
				CombineDuplicates = false,
				Padding = 1
			};

			var spritesPath = Path.Join(ContentPath, "Sprites");
			foreach (var file in Directory.EnumerateFiles(spritesPath, "*.png", SearchOption.AllDirectories))
				packer.Add(GetResourceName(spritesPath, file), new Image(file));

			var result = packer.Pack();
			var pages = new List<Texture>();
			foreach (var it in result.Pages)
			{
				it.Premultiply();
				pages.Add(new Texture(it));
			}

			foreach (var it in result.Entries)
				Subtextures.Add(it.Name, new Subtexture(pages[it.Page], it.Source, it.Frame));
		} */

		// wait for tasks to finish
		{
			foreach (var task in tasks)
				task.Wait();
			/* foreach (var (name, img) in images)
				Textures.Add(name, new Texture(img) { Name = name });*/
			
			/*
			foreach (var (name, model) in models)
			{
				model.ConstructResources();
				Models[name] = model;
			}
			foreach (var lang in langs)
			{
				if (Languages.TryGetValue(lang.ID, out var existing))
					existing.Absorb(lang);
				else
					Languages[lang.ID] = lang;
			} */
		}

		// make sure the active language is ready for use
		// Language.Current.Use();

		Log.Info($"Loaded Assets in {timer.ElapsedMilliseconds}ms");
        IsLoaded = true;
		callback?.Invoke();
	}

	private static string GetResourceName(string contentFolder, string path)
	{
		var fullname = Path.Join(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
		var relative = Path.GetRelativePath(contentFolder, fullname);
		var normalized = relative.Replace("\\", "/");
		return normalized;
	}
}