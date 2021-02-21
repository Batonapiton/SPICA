using System;
using System.IO;
using System.Linq;
using System.Dynamic;
using Mono.Options;
using SPICA.Formats.CtrH3D;
using YamlDotNet.Serialization;
using System.Collections.Generic;
using SPICA.Formats.CtrH3D.Animation;
using Newtonsoft.Json;


namespace SPICA.CLI
{
    public class ModelData
    {
        public string name;
        public string[] files;
        public bool needDeleteBadAnims;
        public string[] animations;
    }
    class Program
    {
        private static OptionSet options;

        static void Main(string[] args)
        {
            string here = AppDomain.CurrentDomain.BaseDirectory;

            Console.WriteLine("SPICA CLI");

            //default args
            string Pokemon = "bulbasaur";
            string MapLocation = $"{here}/data/model_bin_map.yml";
            string BinLocation = $"{here}/in";
            string TextureLocation = $"{here}/tex";
            string ModelLocation = $"{here}/out";
            string JsonData = $"{here}/data/animationsWOBadAnims.json";
            bool UseJsonData = false;
            int Start = 0;
            int End = 1;
            string ModelsDir = $"{here}/out/models";
            
            options = new OptionSet() {
                { "?|h|help", "show the help", _ => HelpText()},
                { "p=|pokemon=", "desired pokemon to dump", input => Pokemon = input},
                { "map=|map-file=", "", input => MapLocation = input},
                { "in=|bin=|bin-dir=", "", input => BinLocation = input},
                { "tex=|texture=|texture-out=", "", input => TextureLocation = input},
                { "model=|model-out", "", input => ModelLocation = input},
                { "usejsondata=", "", input => UseJsonData = bool.Parse(input)},
                { "start=", "", input => Start = int.Parse(input)},
                { "end=", "", input => End = int.Parse(input)},
            };

            var errors = options.Parse(args);

            if (errors.Count > 0) {
                foreach (var err in errors) {
                    Console.Error.WriteLine("Unrecognized option {0}", err);
                }
                Environment.Exit(1);
            }

            if (UseJsonData)
            {
                using (StreamReader r = new StreamReader(JsonData))
                {
                    string json = r.ReadToEnd();
                    List<ModelData> items = JsonConvert.DeserializeObject<List<ModelData>>(json);
                    for (int i = Start; i < End; i++)
                    {
                        ModelData pokemon = items[i];
                        string pokemonDir = $"{ModelsDir}/{pokemon.name}";
                        string texturesDir = $"{pokemonDir}/normal-textures";
                        string texturesShinyDir = $"{pokemonDir}/shiny-textures";
                        Directory.CreateDirectory(texturesDir);
                        Directory.CreateDirectory(texturesShinyDir);
                        
                        string[] files = pokemon.files.Select(file => $"{BinLocation}/{file}").ToArray();

                        Console.Write($"Building Scene with files:\n{string.Join("\n", files)}\n");
                        H3D Scene = FileIO.Merge(files, pokemon.animations, pokemon.needDeleteBadAnims);
                        Console.WriteLine($"Exporting {Scene.Textures.Count} textures");
                        FileIO.ExportTextures(Scene, texturesDir, texturesShinyDir);
                        
                        
                        int[] motions = GetMotionIndices(Scene, MotionLexicon.StandardMotion.Values);

                        // Console.WriteLine($"exporting {pokemonDir}/{Pokemon}.dae model with {motions.Length} motions");
                        // FileIO.ExportDae(Scene, $"{pokemonDir}/{Pokemon}.dae", motions);
                        Console.WriteLine($"exporting {pokemonDir}/{pokemon.name}.dae model with {motions.Length} motions");
                        
                        FileIO.ExportDae(Scene, $"{pokemonDir}/{pokemon.name}.dae", motions);
                        if (motions.Length == 0)
                        {
                            File.AppendAllText("C:\\Users\\User\\Documents\\spice-enchanted\\SPICA\\SPICA.CLI\\bin\\Debug\\net462\\out\\ZeroMotions.txt",
                                pokemon.name + "\n");
                        }
                        Console.WriteLine($"completed exports for: {pokemon.name}");
                    }
                }
            }
            else
            {
                //Directory.CreateDirectory(outDir);
                //string pokemonDir = $"{outDir}/{Pokemon}";
                //string texturesDir = $"{pokemonDir}/textures";
                //string texturesShinyDir = $"{pokemonDir}/textures_shiny";
                //Directory.CreateDirectory(pokemonDir);
                //Directory.CreateDirectory(texturesDir);
                //Directory.CreateDirectory(texturesShinyDir);
                Directory.CreateDirectory(TextureLocation);
                Directory.CreateDirectory(ModelLocation);

                Console.WriteLine($"Searching for: {Pokemon}");
                string[] files = GetFileNames(Pokemon, MapLocation).Select(file => $"{BinLocation}/{file}").ToArray();

                Console.Write($"Building Scene with files:\n{string.Join("\n", files)}\n");
                H3D Scene = FileIO.Merge(files);

                Console.WriteLine($"Exporting {Scene.Textures.Count} textures");
                //FileIO.ExportTextures(Scene, texturesDir, texturesShinyDir);
                FileIO.ExportTextures(Scene, TextureLocation);

                int[] motions = GetMotionIndices(Scene, MotionLexicon.StandardMotion.Values);

                // Console.WriteLine($"exporting {pokemonDir}/{Pokemon}.dae model with {motions.Length} motions");
                // FileIO.ExportDae(Scene, $"{pokemonDir}/{Pokemon}.dae", motions);
                Console.WriteLine($"exporting {ModelLocation}/{Pokemon}.dae model with {motions.Length} motions");
                FileIO.ExportDae(Scene, $"{ModelLocation}/{Pokemon}.dae", motions);

                Console.WriteLine($"completed exports for: {Pokemon}");
            }
        }

        public static int[] GetMotionIndices(H3D Scene, IEnumerable<string> MotionNames)
        {
            List<string> motionNames = new List<string>(MotionNames);
            List<int> targets = new List<int>();

            for (int i = 0; i < Scene.SkeletalAnimations.Count; i++)
            {
                if (motionNames.Contains(Scene.SkeletalAnimations[i].Name))
                {
                    targets.Add(i);
                }
            }
            return targets.ToArray();
        }

        private static string[] GetFileNames(string Pokemon, string MapLocation) {
            var yaml = new DeserializerBuilder().Build();
            dynamic expando = yaml.Deserialize<ExpandoObject>(File.ReadAllText(MapLocation));
            var lookup = expando as IDictionary<string, Object>;

            List<string> files = new List<string>();
            foreach (var file in (lookup[Pokemon] as List<Object>))
                files.Add(file.ToString());

            return files.ToArray();
        }

        private static void HelpText() {
            options.WriteOptionDescriptions(Console.Out);
            Console.WriteLine("SPICA Command Line Interface");

            Environment.Exit(1);
        }
    }
}
