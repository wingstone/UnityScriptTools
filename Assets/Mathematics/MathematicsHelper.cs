using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathematicsHelper
{
    // https://pbr-book.org/3ed-2018/Monte_Carlo_Integration/2D_Sampling_with_Multidimensional_Transformations
    #region Sampling

    // return point in radius 1 Disk
    public static void UniformSampleInDiskWithReject(int num, out Vector2[] points)
    {
        points = new Vector2[num];

        for (int i = 0; i < num; i++)
        {
            float x = Random.value * 2 - 1;
            float y = Random.value * 2 - 1;
            while (x * x + y * y > 1)
            {
                x = Random.value * 2 - 1;
                y = Random.value * 2 - 1;
            }

            points[i] = new Vector2(x, y);
        }
    }

    // return point in radius 1 Disk
    public static void UniformSampleInDisk(int num, out Vector2[] points)
    {
        points = new Vector2[num];

        for (int i = 0; i < num; i++)
        {
            float theta = Random.value * Mathf.PI * 2;
            float r = Mathf.Sqrt(Random.value);

            points[i] = r * (new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)));
        }
    }

    // return point in radius 1 Circle
    public static void UniformSampleInCircle(int num, out Vector2[] points)
    {
        points = new Vector2[num];

        for (int i = 0; i < num; i++)
        {
            float theta = Random.value * Mathf.PI * 2;

            points[i] = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
        }
    }

    // return point in radius 1 Hemisphere Surface
    public static void UniformSampleInHemisphereSurface(int num, out Vector3[] points)
    {
        points = new Vector3[num];

        for (int i = 0; i < num; i++)
        {
            float theta = Random.value * Mathf.PI * 2;
            float cosPhi = Random.value;

            points[i] = new Vector3(Mathf.Cos(theta) * (Mathf.Sqrt(1 - cosPhi * cosPhi)), Mathf.Sin(theta) * (Mathf.Sqrt(1 - cosPhi * cosPhi)), cosPhi);
        }
    }

    // return point in radius 1 Hemisphere Solid
    public static void UniformSampleInHemisphereSolid(int num, out Vector3[] points)
    {
        points = new Vector3[num];

        for (int i = 0; i < num; i++)
        {
            float theta = Random.value * Mathf.PI * 2;
            float cosPhi = Random.value;
            float r = Mathf.Sqrt(Random.value);

            points[i] = r * (new Vector3(Mathf.Cos(theta) * (Mathf.Sqrt(1 - cosPhi * cosPhi)), Mathf.Sin(theta) * (Mathf.Sqrt(1 - cosPhi * cosPhi)), cosPhi));
        }
    }

    // return point in radius 1 Sphere Surface
    public static void UniformSampleInSphereSurface(int num, out Vector3[] points)
    {
        points = new Vector3[num];

        for (int i = 0; i < num; i++)
        {
            float theta = Random.value * Mathf.PI * 2;
            float cosPhi = Random.value * 2 - 1;

            points[i] = new Vector3(Mathf.Cos(theta) * (Mathf.Sqrt(1 - cosPhi * cosPhi)), Mathf.Sin(theta) * (Mathf.Sqrt(1 - cosPhi * cosPhi)), cosPhi);
        }
    }

    // return point in radius 1 Sphere Solid
    public static void UniformSampleInSphereSolid(int num, out Vector3[] points)
    {
        points = new Vector3[num];

        for (int i = 0; i < num; i++)
        {
            float theta = Random.value * Mathf.PI * 2;
            float cosPhi = Random.value * 2 - 1;
            float r = Mathf.Sqrt(Random.value);

            points[i] = r * (new Vector3(Mathf.Cos(theta) * (Mathf.Sqrt(1 - cosPhi * cosPhi)), Mathf.Sin(theta) * (Mathf.Sqrt(1 - cosPhi * cosPhi)), cosPhi));
        }
    }

    // return point in raiangle
    public static void UniformSampleInTriangle(int num, Vector3 p1, Vector3 p2, Vector3 p3, out Vector3[] points)
    {
        points = new Vector3[num];

        for (int i = 0; i < num; i++)
        {
            float r1 = Random.value;
            float r2 = Random.value;

            float s1 = Mathf.Sqrt(r1);

            points[i] = p1 * (1f - s1) + p2 * (1f - r2) * s1 + p3 * r2 * s1;
        }
    }

    #endregion
}
