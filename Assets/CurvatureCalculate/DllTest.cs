using UnityEngine;
using System.Runtime.InteropServices;

public class DllTest : MonoBehaviour
{
    [DllImport("Assets/CurvatureCalculate/Plugins/TrimeshDll.dll", EntryPoint = "test")]
    public static extern int test(int n, int[] a);

    // Start is called before the first frame update
    void Start()
    {
        int[] n = new int[5] { 1, 1, 1, 1, 1 };
        int z = 0;
        z = test(5, n);
        Debug.Log("test dll: " + z);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
