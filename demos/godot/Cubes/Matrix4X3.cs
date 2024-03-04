// SPDX-License-Identifier: MIT

using System.Numerics;
using System.Runtime.InteropServices;

namespace fennecs.demos.godot;

/// <summary>
///     A 4x3 matrix / 3x4 matrix comprised of 12 floats.
///     It uses an orthogonal, non-unit basis, and a translation vector.
///     Works with MultiMesh in <see cref="Godot.MultiMesh.TransformFormat" /> 3D.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 48)]
public struct Matrix4X3
{
    /// <summary>
    ///     <para>
    ///         Alternatively, use:
    ///     </para>
    ///     <c>
    ///         int size
    ///     </c>
    ///     <br />
    ///     <c>
    ///         unsafe { size = sizeof(Matrix4X3) / sizeof(float); }
    ///     </c>
    /// </summary>
    public const int SizeInFloats = 12;


    /// <summary>
    ///     Construct an identity matrix at the origin.
    /// </summary>
    public Matrix4X3()
    {
        M00 = 1;
        M01 = 0;
        M02 = 0;
        M03 = 0;
        M10 = 0;
        M11 = 1;
        M12 = 0;
        M13 = 0;
        M20 = 0;
        M21 = 0;
        M22 = 1;
        M23 = 0;
    }


    /// <summary>
    ///     Construct a matrix that represents a translation and uniform scale.
    /// </summary>
    public Matrix4X3(Vector3 translation, float uniformScale)
    {
        M00 = uniformScale;
        M01 = 0;
        M02 = 0;
        M03 = translation.X;
        M10 = 0;
        M11 = uniformScale;
        M12 = 0;
        M13 = translation.Y;
        M20 = 0;
        M21 = 0;
        M22 = uniformScale;
        M23 = translation.Z;
    }


    public override string ToString()
    {
        return $"Matrix4X3\n{M00}, {M01}, {M02}, {M03}\n{M10}, {M11}, {M12}, {M13}\n{M20}, {M21}, {M22}, {M23}";
    }


    #region Fields

    public float M00;
    public float M01;
    public float M02;
    public float M03;

    public float M10;
    public float M11;
    public float M12;
    public float M13;

    public float M20;
    public float M21;
    public float M22;
    public float M23;

    #endregion
}