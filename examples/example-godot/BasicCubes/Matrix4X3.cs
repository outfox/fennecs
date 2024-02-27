using System.Numerics;
using System.Runtime.InteropServices;

namespace examples.godot.BasicCubes;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct Matrix4X3
{
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


    public Matrix4X3(Vector3 origin)
    {
        M00 = 1;
        M01 = 0;
        M02 = 0;
        M03 = origin.X;
        M10 = 0;
        M11 = 1;
        M12 = 0;
        M13 = origin.Y;
        M20 = 0;
        M21 = 0;
        M22 = 1;
        M23 = origin.Z;
    }


    public Matrix4X3(Vector3 bX, Vector3 bY, Vector3 bZ, Vector3 origin)
    {
        M00 = bX.X;
        M01 = bX.Y;
        M02 = bX.Z;
        M03 = origin.X;
        M10 = bY.X;
        M11 = bY.Y;
        M12 = bY.Z;
        M13 = origin.Y;
        M20 = bZ.X;
        M21 = bZ.Y;
        M22 = bZ.Z;
        M23 = origin.Z;
    }


    public override string ToString()
    {
        return $"Matrix4X3({M00}, {M01}, {M02}, {M03}, {M10}, {M11}, {M12}, {M13}, {M20}, {M21}, {M22}, {M23})";
    }

}