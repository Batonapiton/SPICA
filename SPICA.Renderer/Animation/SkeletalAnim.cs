﻿using OpenTK;

using SPICA.Formats.CtrH3D;
using SPICA.Formats.CtrH3D.Animation;
using SPICA.Formats.CtrH3D.Model;
using SPICA.Renderer.SPICA_GL;

using System;

namespace SPICA.Renderer.Animation
{
    public class SkeletalAnim : AnimControl
    {
        private struct Bone
        {
            public int ParentIndex;

            public Vector3 Scale;
            public Vector3 Rotation;
            public Vector3 Translation;

            public Quaternion QuatRotation;

            public bool IsQuatRotation;
            public bool HasMtxTransform;
        }

        private const string InvalidPrimitiveTypeEx = "Invalid Primitive type used on Skeleton Bone!";

        internal Matrix4[] GetSkeletonTransforms(PatriciaList<H3DBone> Skeleton)
        {
            Matrix4[] Output = new Matrix4[Skeleton.Count];
            Bone[] FrameSkeleton = new Bone[Skeleton.Count];

            int Index = 0;

            foreach (H3DBone Bone in Skeleton)
            {
                Bone B = new Bone
                {
                    ParentIndex = Bone.ParentIndex,

                    Scale       = Bone.Scale.ToVector3(),
                    Rotation    = Bone.Rotation.ToVector3(),
                    Translation = Bone.Translation.ToVector3()
                };

                int Elem = BaseAnimation?.Elements.FindIndex(x => x.Name == Bone.Name) ?? -1;

                if (Elem != -1 && State != AnimState.Stopped)
                {
                    H3DAnimationElement Element = BaseAnimation.Elements[Elem];

                    switch (Element.PrimitiveType)
                    {
                        case H3DAnimPrimitiveType.Transform:
                            H3DAnimTransform Transform = (H3DAnimTransform)Element.Content;

                            if (Transform.ScaleX.HasData)       B.Scale.X       = Transform.ScaleX.GetFrameValue(Frame);
                            if (Transform.ScaleY.HasData)       B.Scale.Y       = Transform.ScaleY.GetFrameValue(Frame);
                            if (Transform.ScaleZ.HasData)       B.Scale.Z       = Transform.ScaleZ.GetFrameValue(Frame);

                            if (Transform.RotationX.HasData)    B.Rotation.X    = Transform.RotationX.GetFrameValue(Frame);
                            if (Transform.RotationY.HasData)    B.Rotation.Y    = Transform.RotationY.GetFrameValue(Frame);
                            if (Transform.RotationZ.HasData)    B.Rotation.Z    = Transform.RotationZ.GetFrameValue(Frame);

                            if (Transform.TranslationX.HasData) B.Translation.X = Transform.TranslationX.GetFrameValue(Frame);
                            if (Transform.TranslationY.HasData) B.Translation.Y = Transform.TranslationY.GetFrameValue(Frame);
                            if (Transform.TranslationZ.HasData) B.Translation.Z = Transform.TranslationZ.GetFrameValue(Frame);

                            break;

                        case H3DAnimPrimitiveType.QuatTransform:
                            H3DAnimQuatTransform QuatTransform = (H3DAnimQuatTransform)Element.Content;

                            int IntFrame = (int)Frame;
                            float Weight = Frame - IntFrame;

                            if (QuatTransform.HasScale)
                            {
                                Vector3 L = QuatTransform.GetScaleValue(IntFrame + 0).ToVector3();
                                Vector3 R = QuatTransform.GetScaleValue(IntFrame + 1).ToVector3();

                                B.Scale = Vector3.Lerp(L, R, Weight);
                            }

                            if (B.IsQuatRotation = QuatTransform.HasRotation)
                            {
                                Quaternion L = QuatTransform.GetRotationValue(IntFrame + 0).ToQuaternion();
                                Quaternion R = QuatTransform.GetRotationValue(IntFrame + 1).ToQuaternion();

                                B.QuatRotation = Quaternion.Slerp(L, R, Weight);
                            }

                            if (QuatTransform.HasTranslation)
                            {
                                Vector3 L = QuatTransform.GetTranslationValue(IntFrame + 0).ToVector3();
                                Vector3 R = QuatTransform.GetTranslationValue(IntFrame + 1).ToVector3();

                                B.Translation = Vector3.Lerp(L, R, Weight);
                            }

                            break;

                        case H3DAnimPrimitiveType.MtxTransform:
                            H3DAnimMtxTransform MtxTransform = (H3DAnimMtxTransform)Element.Content;

                            Output[Index] = MtxTransform.GetTransform((int)Frame).ToMatrix4();

                            B.HasMtxTransform = true;

                            break;

                        default: throw new InvalidOperationException(InvalidPrimitiveTypeEx);
                    }
                }

                FrameSkeleton[Index++] = B;
            }

            for (Index = 0; Index < Skeleton.Count; Index++)
            {
                Bone B = FrameSkeleton[Index];

                if (B.HasMtxTransform) continue;

                Output[Index] = Matrix4.CreateScale(B.Scale);

                while (true)
                {
                    if (B.IsQuatRotation)
                        Output[Index] *= Matrix4.CreateFromQuaternion(B.QuatRotation);
                    else
                        Output[Index] *= RenderUtils.EulerRotate(B.Rotation);

                    Vector3 Translation = B.Translation;

                    if (B.ParentIndex != -1) Translation *= FrameSkeleton[B.ParentIndex].Scale;

                    Output[Index] *= Matrix4.CreateTranslation(Translation);

                    if (B.ParentIndex == -1) break;

                    B = FrameSkeleton[B.ParentIndex];
                }
            }

            return Output;
        }
    }
}