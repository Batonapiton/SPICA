using System;
using System.IO;
using SPICA.WinForms;
using SPICA.Formats.CtrH3D;
using SPICA.Formats.CtrH3D.Model;
using SPICA.Formats.Generic.COLLADA;
using SPICA.WinForms.Formats;

namespace SPICA.CLI
{
    class FileIO
    {
        public static H3D Merge(string[] FileNames, H3D Scene = null)
        {
            if (Scene == null)
            {
                Scene = new H3D();
            }

            int OpenFiles = 0;

            foreach (string FileName in FileNames)
            {
                H3DDict<H3DBone> Skeleton = null;

                if (Scene.Models.Count > 0) Skeleton = Scene.Models[0].Skeleton;

                H3D Data = FormatIdentifier.IdentifyAndOpen(FileName, Skeleton);

                if (Data != null)
                {
                    Scene.Merge(Data);
                    OpenFiles++;
                }
            }
            
            // string[] names =
            // {
            //     "FightingAction1",
            //     "FightingAction2",
            //     "FightingAction4",
            //     "FightingAction5",
            //     "FightingAction6",
            //     "FightingAction9",
            //     "FightingAction13",
            //     "FightingAction17",
            //     "FightingAction18",
            //     "PetAction1-Pose",
            //     "PetAction5-Falling Asleep",
            //     "PetAction6-Sleepy",
            //     "PetAction7-Sleepy Awaken",
            //     "PetAction8-Sleeping",
            //     "PetAction9-Awaken",
            //     "PetAction10-Refuse",
            //     "PetAction12-Agree",
            //     "PetAction13-Happy",
            //     "PetAction14-Very Happy",
            //     "PetAction15-Look Around",
            //     "PetAction17-Comfortable",
            //     "PetAction19-Sad",
            //     "PetAction20-Salutate",
            //     "PetAction22-Angry",
            //     "PetAction23-Begin Eating",
            //     "PetAction24-Eating",
            //     "PetAction25-Eating Finished",
            //     "PetAction26-No Eating",
            //     "MapAction1",
            //     "MapAction3",
            //     "MapAction4"
            // };
            // for (var i = 0; i < Scene.SkeletalAnimations.Count; i++)
            // {
            //     Scene.SkeletalAnimations[i].Name = names[i];
            // }
            if (OpenFiles == 0)
            {
                //todo: improve this error message by making the format discovery return some kind of report
                Console.Write("Unsupported file format!", "Can't open file!");
            }

            return Scene;
        }
        public static H3D Merge(string[] FileNames, string[] MotionNames, bool DeleteBadAnims, H3D Scene = null)
        {
            string[] motionNames = MotionNames;
            
            if (Scene == null)
            {
                Scene = new H3D();
            }

            int OpenFiles = 0;

            foreach (string FileName in FileNames)
            {
                H3DDict<H3DBone> Skeleton = null;

                if (Scene.Models.Count > 0) Skeleton = Scene.Models[0].Skeleton;

                H3D Data = FormatIdentifier.IdentifyAndOpen(FileName, DeleteBadAnims, Skeleton);

                if (Data != null)
                {
                    Scene.Merge(Data);
                    OpenFiles++;
                }
            }
            for (var i = 0; i < Scene.SkeletalAnimations.Count; i++)
            {
                Scene.SkeletalAnimations[i].Name = motionNames[i];
            }
            if (OpenFiles == 0)
            {
                //todo: improve this error message by making the format discovery return some kind of report
                Console.Write("Unsupported file format!", "Can't open file!");
            }

            return Scene;
        }

        public static void ExportDae(H3D Scene, string Filename, int[] SelectedAnimations)
        {
            int highPolyModel = 0;
            new DAE(Scene, highPolyModel, SelectedAnimations).Save(Filename);
        }

        public static void ExportTextures(H3D Scene, string path)
        {
            TextureManager.Textures = Scene.Textures;
            for (int i = 0; i < Scene.Textures.Count; i++)
            {
                string FileName = Path.Combine(path, $"{Scene.Textures[i].Name}.png");

                TextureManager.GetTexture(i).Save(FileName);
            }
        }
        
        public static void ExportTextures(H3D Scene, string texturesDir, string texturesShinyDir)
        {
            TextureManager.Textures = Scene.Textures;
            for (int i = 0; i < Scene.Textures.Count; i++)
            {
                string textureName = Scene.Textures[i].Name;
                string fileName;
                if (textureName.Contains(".tga_1"))
                {
                    textureName = textureName.Replace(".tga_1", ".tga");
                    fileName = Path.Combine(texturesShinyDir, $"{textureName}.png");
                }
                else
                {
                    fileName = Path.Combine(texturesDir, $"{textureName}.png");
                }
                TextureManager.GetTexture(i).Save(fileName);
            }
        }
    }
}
