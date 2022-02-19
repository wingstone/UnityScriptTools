using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculateDirectionalCurvature : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        mf.sharedMesh = need_curvatures(mf.sharedMesh);
    }

    void GetDirCurv(Mesh mesh)
    {

    }

    int NEXT_MOD3(int i)
    {
        return ((i) < 2 ? (i) + 1 : (i) - 2);
    }
    int PREV_MOD3(int i)
    {
        return ((i) > 0 ? (i) - 1 : (i) + 2);
    }

    float[] pointareas;
    float[,] cornerareas;

    // Compute per-vertex point areas
    void need_pointareas(Mesh mesh)
    {
        Debug.Log("Computing point areas... ");

        int nf = mesh.triangles.Length / 3;
        int nv = mesh.vertexCount;
        pointareas = new float[nv];
        cornerareas = new float[nf, 3];

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        int[,] faces = new int[triangles.Length / 3, 3];
        for (int i = 0; i < triangles.Length / 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                faces[i, j] = triangles[i * 3 + j];
            }
        }

        // Vector3[] e = new Vector3[3];
        for (int i = 0; i < nf; i++)
        {
            // Edges
            Vector3[] e = new Vector3[3] { vertices[faces[i, 2]] - vertices[faces[i, 1]],
                     vertices[faces[i, 0]] - vertices[faces[i, 2]],
                     vertices[faces[i, 1]] - vertices[faces[i, 0]] };

            // Compute corner weights
            float area = 0.5f * Vector3.Cross(e[0], e[1]).magnitude;
            float[] l2 = { e[0].sqrMagnitude, e[1].sqrMagnitude, e[2].sqrMagnitude };

            // Barycentric weights of circumcenter
            float[] bcw = { l2[0] * (l2[1] + l2[2] - l2[0]),
                         l2[1] * (l2[2] + l2[0] - l2[1]),
                         l2[2] * (l2[0] + l2[1] - l2[2]) };
            if (bcw[0] <= 0.0f)
            {
                cornerareas[i, 1] = -0.25f * l2[2] * area /
                                    Vector3.Dot(e[0], e[2]);
                cornerareas[i, 2] = -0.25f * l2[1] * area /
                                    Vector3.Dot(e[0], e[1]);
                cornerareas[i, 0] = area - cornerareas[i, 1] -
                                    cornerareas[i, 2];
            }
            else if (bcw[1] <= 0.0f)
            {
                cornerareas[i, 2] = -0.25f * l2[0] * area /
                                    Vector3.Dot(e[1], e[0]);
                cornerareas[i, 0] = -0.25f * l2[2] * area /
                                            Vector3.Dot(e[1], e[2]);
                cornerareas[i, 1] = area - cornerareas[i, 2] -
                                            cornerareas[i, 0];
            }
            else if (bcw[2] <= 0.0f)
            {
                cornerareas[i, 0] = -0.25f * l2[1] * area /
                                    Vector3.Dot(e[2], e[1]);
                cornerareas[i, 1] = -0.25f * l2[0] * area /
                                    Vector3.Dot(e[2], e[0]);
                cornerareas[i, 2] = area - cornerareas[i, 0] -
                                    cornerareas[i, 1];
            }
            else
            {
                float scale = 0.5f * area / (bcw[0] + bcw[1] + bcw[2]);
                for (int j = 0; j < 3; j++)
                    cornerareas[i, j] = scale * (bcw[NEXT_MOD3(j)] +
                                                 bcw[PREV_MOD3(j)]);
            }

            pointareas[faces[i, 0]] += cornerareas[i, 0];

            pointareas[faces[i, 1]] += cornerareas[i, 1];

            pointareas[faces[i, 2]] += cornerareas[i, 2];
        }

        Debug.Log("Done.\n");
    }

    float sqr(float a) { return a * a; }

    bool ldltdc(ref float[,] A, ref float[] rdiag, int N)
    {
        // Special case for small N
        if (N < 1)
        {
            return false;
        }
        else if (N <= 3)
        {
            float d0 = A[0, 0];
            rdiag[0] = 1 / d0;
            if (N == 1)
                return (d0 != 0);
            A[1, 0] = A[0, 1];
            float l10 = rdiag[0] * A[1, 0];
            float d1 = A[1, 1] - l10 * A[1, 0];
            rdiag[1] = 1 / d1;
            if (N == 2)
                return (d0 != 0 && d1 != 0);
            float d2 = A[2, 2] - rdiag[0] * sqr(A[2, 0]) - rdiag[1] * sqr(A[2, 1]);
            rdiag[2] = 1 / d2;
            A[2, 0] = A[0, 2];
            A[2, 1] = A[1, 2] - l10 * A[2, 0];
            return (d0 != 0 && d1 != 0 && d2 != 0);
        }

        float[] v = new float[N - 1];
        for (int i = 0; i < N; i++)
        {
            for (int k = 0; k < i; k++)
                v[k] = A[i, k] * rdiag[k];
            for (int j = i; j < N; j++)
            {
                float sum = A[i, j];
                for (int k = 0; k < i; k++)
                    sum -= v[k] * A[j, k];
                if (i == j)
                {
                    if ((sum == 0))
                        return false;
                    rdiag[i] = 1 / sum;
                }
                else
                {
                    A[j, i] = sum;
                }
            }
        }

        return true;
    }

    // Solve Ax=b after ldltdc.  x is allowed to be the same as b.
    static void ldltsl(float[,] A, float[] rdiag, float[] b, ref float[] x, int N)
    {
        for (int i = 0; i < N; i++)
        {
            float sum = b[i];
            for (int k = 0; k < i; k++)
                sum -= A[i, k] * x[k];
            x[i] = sum * rdiag[i];
        }
        for (int i = N - 1; i >= 0; i--)
        {
            float sum = 0;
            for (int k = i + 1; k < N; k++)
                sum += A[k, i] * x[k];
            x[i] -= sum * rdiag[i];
        }
    }

    // Rotate a coordinate system to be perpendicular to the given normal
    static void rot_coord_sys(Vector3 old_u, Vector3 old_v,
                               Vector3 new_norm,
                              ref Vector3 new_u, ref Vector3 new_v)
    {
        new_u = old_u;
        new_v = old_v;
        Vector3 old_norm = Vector3.Cross(old_u, old_v);
        float ndot = Vector3.Dot(old_norm, new_norm);
        if ((ndot <= -1.0f))
        {
            new_u = -new_u;
            new_v = -new_v;
            return;
        }

        // Perpendicular to old_norm and in the plane of old_norm and new_norm
        Vector3 perp_old = new_norm - ndot * old_norm;

        // Perpendicular to new_norm and in the plane of old_norm and new_norm
        // Vector3 perp_new = ndot * new_norm - old_norm;

        // perp_old - perp_new, with normalization ants folded in
        Vector3 dperp = 1.0f / (1 + ndot) * (old_norm + new_norm);

        // Subtracts component along perp_old, and adds the same amount along
        // perp_new.  Leaves unchanged the component perpendicular to the
        // plane containing old_norm and new_norm.
        new_u -= dperp * Vector3.Dot(new_u, perp_old);
        new_v -= dperp * Vector3.Dot(new_v, perp_old);
    }

    // Reproject a curvature tensor from the basis spanned by old_u and old_v
    // (which are assumed to be unit-length and perpendicular) to the
    // new_u, new_v basis.
    void proj_curv(Vector3 old_u, Vector3 old_v,
               float old_ku, float old_kuv, float old_kv,
                Vector3 new_u, Vector3 new_v,
               ref float new_ku, ref float new_kuv, ref float new_kv)
    {
        Vector3 r_new_u = Vector3.zero;
        Vector3 r_new_v = Vector3.zero;
        rot_coord_sys(new_u, new_v, Vector3.Cross(old_u, old_v), ref r_new_u, ref r_new_v);

        float u1 = Vector3.Dot(r_new_u, old_u);
        float v1 = Vector3.Dot(r_new_u, old_v);
        float u2 = Vector3.Dot(r_new_v, old_u);
        float v2 = Vector3.Dot(r_new_v, old_v);
        new_ku = old_ku * u1 * u1 + old_kuv * (2.0f * u1 * v1) + old_kv * v1 * v1;
        new_kuv = old_ku * u1 * u2 + old_kuv * (u1 * v2 + u2 * v1) + old_kv * v1 * v2;
        new_kv = old_ku * u2 * u2 + old_kuv * (2.0f * u2 * v2) + old_kv * v2 * v2;
    }



    // Given a curvature tensor, find principal directions and curvatures
    // Makes sure that pdir1 and pdir2 are perpendicular to normal
    void diagonalize_curv(Vector3 old_u, Vector3 old_v,
                      float ku, float kuv, float kv,
                       Vector3 new_norm,
                       ref Vector3 pdir1, ref Vector3 pdir2, ref float k1, ref float k2)
    {
        Vector3 r_old_u = Vector3.zero;
        Vector3 r_old_v = Vector3.zero;
        rot_coord_sys(old_u, old_v, new_norm, ref r_old_u, ref r_old_v);

        float c = 1, s = 0, tt = 0;
        if ((kuv != 0.0f))
        {
            // Jacobi rotation to diagonalize
            float h = 0.5f * (kv - ku) / kuv;
            tt = (h < 0.0f) ?
                    1.0f / (h - Mathf.Sqrt(1.0f + h * h)) :
                    1.0f / (h + Mathf.Sqrt(1.0f + h * h));
            c = 1.0f / Mathf.Sqrt(1.0f + tt * tt);
            s = tt * c;
        }

        k1 = ku - tt * kuv;
        k2 = kv + tt * kuv;

        if (Mathf.Abs(k1) >= Mathf.Abs(k2))
        {
            pdir1 = c * r_old_u - s * r_old_v;
        }
        else
        {
            float tmp = k1;
            k1 = tmp;
            k2 = tmp;
            pdir1 = s * r_old_u + c * r_old_v;
        }
        pdir2 = Vector3.Cross(new_norm, pdir1);
    }


    // Compute principal curvatures and directions.
    Mesh need_curvatures(Mesh mesh)
    {
        if (mesh.normals == null || mesh.normals.Length == 0)
            return mesh;

        need_pointareas(mesh);

        Debug.Log("Computing curvatures... ");

        int nv = mesh.vertexCount;
        int nf = mesh.triangles.Length / 3;

        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        int[] triangles = mesh.triangles;
        int[,] faces = new int[triangles.Length / 3, 3];
        for (int i = 0; i < triangles.Length / 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                faces[i, j] = triangles[i * 3 + j];
            }
        }

        float[] curv1 = new float[nv];
        float[] curv2 = new float[nv];
        Vector3[] pdir1 = new Vector3[nv];
        Vector3[] pdir2 = new Vector3[nv];
        List<float> curv12 = new List<float>(new float[nv]);

        // Set up an initial coordinate system per vertex
        for (int i = 0; i < nf; i++)
        {
            pdir1[faces[i, 0]] = vertices[faces[i, 1]] -
                                 vertices[faces[i, 0]];
            pdir1[faces[i, 1]] = vertices[faces[i, 2]] -
                                 vertices[faces[i, 1]];
            pdir1[faces[i, 2]] = vertices[faces[i, 0]] -
                                 vertices[faces[i, 2]];
        }

        for (int i = 0; i < nv; i++)
        {
            pdir1[i] = Vector3.Cross(pdir1[i], normals[i]);
            pdir1[i].Normalize();
            pdir2[i] = Vector3.Cross(normals[i], pdir1[i]);
        }

        // Compute curvature per-face
        // Vector3[] e;
        for (int i = 0; i < nf; i++)
        {
            // Edges
            Vector3[] e = new Vector3[] { vertices[faces[i, 2]] - vertices[faces[i, 1]],
                     vertices[faces[i, 0]] - vertices[faces[i, 2]],
                     vertices[faces[i, 1]] - vertices[faces[i, 0]] };

            // N-T-B coordinate system per face
            Vector3 t = e[0];
            t.Normalize();
            Vector3 n = Vector3.Cross(e[0], e[1]);
            Vector3 b = Vector3.Cross(n, t);
            b.Normalize();

            // Estimate curvature based on variation of normals
            // along edges
            float[] m = { 0, 0, 0 };
            float[,] w = { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
            for (int j = 0; j < 3; j++)
            {
                float u = Vector3.Dot(e[j], t);
                float v = Vector3.Dot(e[j], b);
                w[0, 0] += u * u;
                w[0, 1] += u * v;
                w[2, 2] += v * v;
                // The below are computed once at the end of the loop
                // w[1, 1] += v*v + u*u;
                // w[1, 2] += u*v;
                Vector3 dn = normals[faces[i, PREV_MOD3(j)]] -
                         normals[faces[i, NEXT_MOD3(j)]];
                float dnu = Vector3.Dot(dn, t);
                float dnv = Vector3.Dot(dn, b);
                m[0] += dnu * u;
                m[1] += dnu * v + dnv * u;
                m[2] += dnv * v;
            }
            w[1, 1] = w[0, 0] + w[2, 2];
            w[1, 2] = w[0, 1];

            // Least squares solution
            float[] diag = new float[3];
            if (!ldltdc(ref w, ref diag, 3))
            {
                //Debug.Log("ldltdc failed!\n");
                continue;
            }
            ldltsl(w, diag, m, ref m, 3);

            // Push it back out to the vertices
            for (int j = 0; j < 3; j++)
            {
                int vj = faces[i, j];
                float c1 = 0;
                float c12 = 0;
                float c2 = 0;
                proj_curv(t, b, m[0], m[1], m[2],
                          pdir1[vj], pdir2[vj], ref c1, ref c12, ref c2);
                float wt = cornerareas[i, j] / pointareas[vj];

                curv1[vj] += wt * c1;

                curv12[vj] += wt * c12;

                curv2[vj] += wt * c2;
            }
        }

        // Compute principal directions and curvatures at each vertex

        for (int i = 0; i < nv; i++)
        {
            diagonalize_curv(pdir1[i], pdir2[i],
                             curv1[i], curv12[i], curv2[i],
                             normals[i], ref pdir1[i], ref pdir2[i],
                            ref curv1[i], ref curv2[i]);
        }

        Mesh newMesh = new Mesh();
        newMesh.vertices = mesh.vertices;
        newMesh.triangles = mesh.triangles;
        newMesh.normals = mesh.normals;
        Color[] colors = new Color[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            colors[i].r = (curv1[i] * 0.5f + curv2[i] * 0.5f)*0.1f;
            colors[i].g = (curv1[i] * 0.5f + curv2[i] * 0.5f)*0.1f;
            colors[i].b = (curv1[i] * 0.5f + curv2[i] * 0.5f)*0.1f;
        }
        newMesh.colors = colors;

        Debug.Log("Done.\n");

        return newMesh;
    }
}
