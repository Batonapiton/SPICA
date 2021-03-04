using System;
using System.Collections.Generic;
using SPICA.Formats.CtrH3D;
using SPICA.Formats.CtrH3D.Animation;
using SPICA.Formats.CtrH3D.Model;
using SPICA.Formats.GFL2;
using SPICA.Formats.GFL2.Model;
using SPICA.Formats.GFL2.Motion;
using SPICA.Formats.GFL2.Shader;
using SPICA.Formats.GFL2.Texture;

using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace SPICA.WinForms.Formats
{
    class GFPkmnModel
    {
        const uint GFModelConstant   = 0x15122117;
        const uint GFTextureConstant = 0x15041213;
        const uint GFMotionConstant  = 0x00060000;

        const uint BCHConstant = 0x00484342;

        public static H3D OpenAsH3D(Stream Input, GFPackage.Header Header, H3DDict<H3DBone> Skeleton = null)
        {
            H3D Output = default(H3D);

            BinaryReader Reader = new BinaryReader(Input);

            try
            {
                Input.Seek(Header.Entries[0].Address, SeekOrigin.Begin);

            uint MagicNum = Reader.ReadUInt32();

            switch (MagicNum)
            {
                case GFModelConstant:
                    GFModelPack MdlPack = new GFModelPack();

                    //High Poly Pokémon model
                    Input.Seek(Header.Entries[0].Address, SeekOrigin.Begin);

                    MdlPack.Models.Add(new GFModel(Reader, "PM_HighPoly"));

                    //Low Poly Pokémon model
                    Input.Seek(Header.Entries[1].Address, SeekOrigin.Begin);
                    
                    MdlPack.Models.Add(new GFModel(Reader, "PM_LowPoly"));

                    //Pokémon Shader package
                    Input.Seek(Header.Entries[2].Address, SeekOrigin.Begin);

                    GFPackage.Header PSHeader = GFPackage.GetPackageHeader(Input);

                    foreach (GFPackage.Entry Entry in PSHeader.Entries)
                    {
                        Input.Seek(Entry.Address, SeekOrigin.Begin);

                        MdlPack.Shaders.Add(new GFShader(Reader));
                    }

                    //More shaders
                    Input.Seek(Header.Entries[3].Address, SeekOrigin.Begin);

                    if (GFPackage.IsValidPackage(Input))
                    {
                        GFPackage.Header PCHeader = GFPackage.GetPackageHeader(Input);

                        foreach (GFPackage.Entry Entry in PCHeader.Entries)
                        {
                            Input.Seek(Entry.Address, SeekOrigin.Begin);

                            MdlPack.Shaders.Add(new GFShader(Reader));
                        }
                    }

                    Output = MdlPack.ToH3D();

                    break;

                case GFTextureConstant:
                    Output = new H3D();

                    foreach (GFPackage.Entry Entry in Header.Entries)
                    {
                        Input.Seek(Entry.Address, SeekOrigin.Begin);

                        Output.Textures.Add(new GFTexture(Reader).ToH3DTexture());
                    }

                    break;

                case GFMotionConstant:
                    Output = new H3D();

                    if (Skeleton == null) break;

                    for (int Index = 0; Index < Header.Entries.Length; Index++)
                    {
                        Input.Seek(Header.Entries[Index].Address, SeekOrigin.Begin);
                        if (Input.Position + 4 > Input.Length) break;
                        if (Reader.ReadUInt32() != GFMotionConstant) continue;

                        Input.Seek(-4, SeekOrigin.Current);

                        GFMotion Mot = new GFMotion(Reader, Index);

                        H3DAnimation    SklAnim = Mot.ToH3DSkeletalAnimation(Skeleton);
                        H3DMaterialAnim MatAnim = Mot.ToH3DMaterialAnimation();
                        H3DAnimation    VisAnim = Mot.ToH3DVisibilityAnimation();

                        if (SklAnim != null)
                        {
                            SklAnim.Name = $"Motion_{Index+1}";
                                //SklAnim.Name = $"Motion_{Mot.Index}";

                            // Output.SkeletalAnimations.Add(SklAnim);

                            // if (!sklAdresses.Contains(Header.Entries[Index].Address))
                            if (Header.Entries[Index].Length != 0)
                            {
                                Output.SkeletalAnimations.Add(SklAnim);
                            }
                        }

                        if (MatAnim != null)
                        {
                            //MatAnim.Name = $"Motion_{Mot.Index}";

                            MatAnim.Name = $"Motion_{Index+1}";
                            if (Header.Entries[Index].Length != 0)
                            {
                                Output.MaterialAnimations.Add(MatAnim);
                            }
                        }

                        if (VisAnim != null)
                        {
                            //VisAnim.Name = $"Motion_{Mot.Index}";

                            VisAnim.Name = $"Motion_{Index+1}";
                            if (Header.Entries[Index].Length != 0)
                            {
                                Output.VisibilityAnimations.Add(VisAnim);
                            }
                        }
                    }

                    break;

                case BCHConstant:
                    Output = new H3D();

                    foreach (GFPackage.Entry Entry in Header.Entries)
                    {
                        Input.Seek(Entry.Address, SeekOrigin.Begin);

                        MagicNum = Reader.ReadUInt32();

                        if (MagicNum != BCHConstant) continue;

                        Input.Seek(-4, SeekOrigin.Current);

                        byte[] Buffer = Reader.ReadBytes(Entry.Length);

                        Output.Merge(H3D.Open(Buffer));
                    }

                    break;
            }

            return Output;
            }
            catch (EndOfStreamException e)
            {
                return new H3D();
                throw;
            }
            
        }
        public static H3D OpenAsH3D(Stream Input, GFPackage.Header Header, int FileIndex, int AnimCount, H3DDict<H3DBone> Skeleton = null)
        {
            int fileIndex = FileIndex;
            H3D Output = default(H3D);

            BinaryReader Reader = new BinaryReader(Input);

            try
            {
                Input.Seek(Header.Entries[0].Address, SeekOrigin.Begin);

            uint MagicNum = Reader.ReadUInt32();

            switch (MagicNum)
            {
                case GFModelConstant:
                    GFModelPack MdlPack = new GFModelPack();

                    //High Poly Pokémon model
                    Input.Seek(Header.Entries[0].Address, SeekOrigin.Begin);

                    MdlPack.Models.Add(new GFModel(Reader, "PM_HighPoly"));

                    //Low Poly Pokémon model
                    Input.Seek(Header.Entries[1].Address, SeekOrigin.Begin);
                    
                    MdlPack.Models.Add(new GFModel(Reader, "PM_LowPoly"));

                    //Pokémon Shader package
                    Input.Seek(Header.Entries[2].Address, SeekOrigin.Begin);

                    GFPackage.Header PSHeader = GFPackage.GetPackageHeader(Input);

                    foreach (GFPackage.Entry Entry in PSHeader.Entries)
                    {
                        Input.Seek(Entry.Address, SeekOrigin.Begin);

                        MdlPack.Shaders.Add(new GFShader(Reader));
                    }

                    //More shaders
                    Input.Seek(Header.Entries[3].Address, SeekOrigin.Begin);

                    if (GFPackage.IsValidPackage(Input))
                    {
                        GFPackage.Header PCHeader = GFPackage.GetPackageHeader(Input);

                        foreach (GFPackage.Entry Entry in PCHeader.Entries)
                        {
                            Input.Seek(Entry.Address, SeekOrigin.Begin);

                            MdlPack.Shaders.Add(new GFShader(Reader));
                        }
                    }

                    Output = MdlPack.ToH3D();

                    break;

                case GFTextureConstant:
                    Output = new H3D();

                    foreach (GFPackage.Entry Entry in Header.Entries)
                    {
                        Input.Seek(Entry.Address, SeekOrigin.Begin);

                        Output.Textures.Add(new GFTexture(Reader).ToH3DTexture());
                    }

                    break;

                case GFMotionConstant:
                    Output = new H3D();

                    if (Skeleton == null) break;
                    HashSet<uint> sklAdresses = new HashSet<uint>();
                    HashSet<uint> materialAdresses = new HashSet<uint>();
                    HashSet<uint> visibilityAdresses = new HashSet<uint>();
                    for (int Index = 0; Index < Header.Entries.Length; Index++)
                    {
                        Input.Seek(Header.Entries[Index].Address, SeekOrigin.Begin);
                        if (Input.Position + 4 > Input.Length) break;
                        if (Reader.ReadUInt32() != GFMotionConstant) continue;

                        Input.Seek(-4, SeekOrigin.Current);

                        GFMotion Mot = new GFMotion(Reader, Index);

                        H3DAnimation    SklAnim = Mot.ToH3DSkeletalAnimation(Skeleton);
                        H3DMaterialAnim MatAnim = Mot.ToH3DMaterialAnimation();
                        H3DAnimation    VisAnim = Mot.ToH3DVisibilityAnimation();

                        if (SklAnim != null)
                        {
                            // SklAnim.Name = $"Motion_{Mot.Index}";

                            // Output.SkeletalAnimations.Add(SklAnim);

                            if (!sklAdresses.Contains(Header.Entries[Index].Address))
                            {
                                Output.SkeletalAnimations.Add(SklAnim);
                                sklAdresses.Add(Header.Entries[Index].Address);
                                Console.WriteLine("skeletal " + Header.Entries[Index].Address);
                            }
                        }

                        if (MatAnim != null)
                        {
                            //MatAnim.Name = $"Motion_{Mot.Index}";

                            if (!materialAdresses.Contains(Header.Entries[Index].Address))
                            {
                                Output.MaterialAnimations.Add(MatAnim);
                                materialAdresses.Add(Header.Entries[Index].Address);
                                Console.WriteLine("material " +Header.Entries[Index].Address);
                            }
                        }

                        if (VisAnim != null)
                        {
                            //VisAnim.Name = $"Motion_{Mot.Index}";

                            Output.VisibilityAnimations.Add(VisAnim);
                            if (!visibilityAdresses.Contains(Header.Entries[Index].Address))
                            {
                                Output.VisibilityAnimations.Add(VisAnim);
                                visibilityAdresses.Add(Header.Entries[Index].Address);
                                Console.WriteLine("visibility " +Header.Entries[Index].Address);
                            }
                        }
                    }
                    // Console.WriteLine(Output.SkeletalAnimations.Count);
                    // Console.WriteLine(AnimCount);
                    while (Output.SkeletalAnimations.Count > AnimCount)
                    {
                        Output.SkeletalAnimations.Remove(Output.SkeletalAnimations.Count-1);
                    }
                    // if (fileIndex == 4)
                    // {
                    //     
                    // }
                    //todo здесь проверку 
                    // if (Output.SkeletalAnimations.Any())
                    // {
                    //     Output.SkeletalAnimations.Remove(Output.SkeletalAnimations.Count-1);
                    // }

                    break;

                case BCHConstant:
                    Output = new H3D();

                    foreach (GFPackage.Entry Entry in Header.Entries)
                    {
                        Input.Seek(Entry.Address, SeekOrigin.Begin);

                        MagicNum = Reader.ReadUInt32();

                        if (MagicNum != BCHConstant) continue;

                        Input.Seek(-4, SeekOrigin.Current);

                        byte[] Buffer = Reader.ReadBytes(Entry.Length);

                        Output.Merge(H3D.Open(Buffer));
                    }

                    break;
            }

            return Output;
            }
            catch (EndOfStreamException e)
            {
                return new H3D();
                throw;
            }
            
        }
    }
}
