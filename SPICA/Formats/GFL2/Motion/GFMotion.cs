using SPICA.Formats.CtrH3D;
using SPICA.Formats.CtrH3D.Animation;
using SPICA.Formats.CtrH3D.Model;
using SPICA.Formats.GFL2.Model;
using SPICA.Math3D;

using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace SPICA.Formats.GFL2.Motion
{
    public class GFMotion
    {
        private enum Sect
        {
            SubHeader = 0,
            SkeletalAnim = 1,
            MaterialAnim = 3,
            VisibilityAnim = 6
        }

        private struct Section
        {
            public Sect SectName;

            public uint Length;
            public uint Address;
        }

        public uint FramesCount;

        public bool IsLooping;
        public bool IsBlended;

        public Vector3 AnimRegionMin;
        public Vector3 AnimRegionMax;

        public GFSkeletonMot   SkeletalAnimation;
        public GFMaterialMot   MaterialAnimation;
        public GFVisibilityMot VisibilityAnimation;

        public int Index;

        public GFMotion() { }

        public GFMotion(BinaryReader Reader, int Index)
        {
            this.Index = Index;

            long Position = Reader.BaseStream.Position;

            uint MagicNumber  = Reader.ReadUInt32();
            uint SectionCount = Reader.ReadUInt32();

            Section[] AnimSections = new Section[SectionCount];

            for (int Anim = 0; Anim < AnimSections.Length; Anim++)
            {
                AnimSections[Anim] = new Section()
                {
                    SectName = (Sect)Reader.ReadUInt32(),

                    Length  = Reader.ReadUInt32(),
                    Address = Reader.ReadUInt32()
                };
            }

            //SubHeader
            Reader.BaseStream.Seek(Position + AnimSections[0].Address, SeekOrigin.Begin);

            FramesCount = Reader.ReadUInt32();

            IsLooping = (Reader.ReadUInt16() & 1) != 0;
            IsBlended = (Reader.ReadUInt16() & 1) != 0; //Not sure

            AnimRegionMin = Reader.ReadVector3();
            AnimRegionMax = Reader.ReadVector3();

            uint AnimHash = Reader.ReadUInt32();

            //Content
            for (int Anim = 1; Anim < AnimSections.Length; Anim++)
            {
                Reader.BaseStream.Seek(Position + AnimSections[Anim].Address, SeekOrigin.Begin);

                switch (AnimSections[Anim].SectName)
                {
                    case Sect.SkeletalAnim: SkeletalAnimation = new GFSkeletonMot(Reader, FramesCount); break;
                    case Sect.MaterialAnim: MaterialAnimation = new GFMaterialMot(Reader, FramesCount); break;
                    case Sect.VisibilityAnim: VisibilityAnimation = new GFVisibilityMot(Reader, FramesCount); break;
                }
            }
        }

        public H3DAnimation ToH3DSkeletalAnimation(H3DDict<H3DBone> Skeleton)
        {
            List<GFBone> GFSkeleton = new List<GFBone>();

            foreach (H3DBone Bone in Skeleton)
            {
                GFSkeleton.Add(new GFBone()
                {
                    Name        = Bone.Name,
                    Parent      = Bone.ParentIndex != -1 ? Skeleton[Bone.ParentIndex].Name : string.Empty,
                    Flags       = (byte)(Bone.ParentIndex == -1 ? 2 : 1), //TODO: Fix, 2 = Identity and 1 Normal bone?
                    Translation = Bone.Translation,
                    Rotation    = Bone.Rotation,
                    Scale       = Bone.Scale,
                });
            }

            return ToH3DSkeletalAnimation(GFSkeleton);
        }

        public H3DAnimation ToH3DSkeletalAnimation(List<GFBone> Skeleton)
        {
            return SkeletalAnimation?.ToH3DAnimation(Skeleton, this);
        }

        public H3DMaterialAnim ToH3DMaterialAnimation()
        {
            return MaterialAnimation?.ToH3DAnimation(this);
        }

        public H3DAnimation ToH3DVisibilityAnimation()
        {
            return VisibilityAnimation?.ToH3DAnimation(this);
        }

        public static string GetMotionName(string key) {

            string name = key;

            switch (key.ToLower())
            {
                case "motion_0": name = "idle"; break;
                case "motion_6": name = "attack-close"; break;
                case "motion_9": name = "attack-range"; break;
                case "motion_13": name = "hit"; break;
                case "motion_17": name = "faint"; break;
                case "motion_10_1": name = "celebrate"; break;
                case "motion_1_2": name = "walk"; break;
                case "motion_3_2": name = "run"; break;
            }


            return StandardMotion.ContainsKey(key) ? StandardMotion[key] : key;
                //return name;
        }

        public static Dictionary<string, string> StandardMotion = new Dictionary<string, string>() {
            
            { "FightingAction1", "idle" },
            { "FightingAction9", "attack" },
            {  "FightingAction10", "attack-2"},
            {  "FightingAction11", "attack-3"},
            {  "FightingAction12", "attack-4"},
            {  "FightingAction13", "range-attack"},
            {  "FightingAction14", "range-attack-2"},
            {  "FightingAction15", "range-attack-3"},
            {  "FightingAction16", "range-attack-4"},
            {  "FightingAction17", "hit"},
            {  "FightingAction18", "lost"},
            { "PetAction5", "falling-asleep" },
            {  "PetAction6", "sleepy" },
            { "PetAction8", "sleeping" },
            { "PetAction13", "happy" },
            { "PetAction21", "happy-2" },
            {  "MapAction3","walk" },
            {  "MapAction4", "run"},
        };

        // public static Dictionary<string, string> StandardMotion = new Dictionary<string, string>() {
        //     { "Motion_0", "idle"},
        //     { "Motion_3", "jump"},
        //     { "Motion_4", "airborn"},
        //     { "Motion_5", "fall"},
        //     { "Motion_6", "attack1"},
        //     { "Motion_7", "attack1_1"},
        //     { "Motion_9", "attack2"},
        //     { "Motion_13","hit1@"},
        //     { "Motion_14","hit2@"},
        //     { "Motion_17", "faint"},
        //     {"Motion_5_1", "sleep1@"},
        //     {"Motion_7_1", "sleep2@"},
        //     {"Motion_8_1", "wakeUp@"},
        //     {"Motion_10_1", "celebrate"},
        //     {"Motion_1_2", "walk"},
        //     {"Motion_3_2", "run"}
        // };
    }
}
