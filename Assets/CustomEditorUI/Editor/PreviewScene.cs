using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
public class PreviewScene : IDisposable
{
    private readonly Scene m_Scene;
    private readonly List<GameObject> m_GameObjects = new List<GameObject>();
    private readonly Camera m_Camera;

    public PreviewScene(string sceneName)
    {
        m_Scene = EditorSceneManager.NewPreviewScene();
        if (!m_Scene.IsValid())
            throw new InvalidOperationException("Preview scene could not be created");

        m_Scene.name = sceneName;

        var camGO = EditorUtility.CreateGameObjectWithHideFlags("Preview Scene Camera", HideFlags.HideAndDontSave, typeof(Camera));
        AddGameObject(camGO);
        m_Camera = camGO.GetComponent<Camera>();
        camera.cameraType = CameraType.Preview;
        camera.enabled = false;
        camera.clearFlags = CameraClearFlags.Depth;
        camera.fieldOfView = 15;
        camera.farClipPlane = 10.0f;
        camera.nearClipPlane = 2.0f;

        // Explicitly use forward rendering for all previews
        // (deferred fails when generating some static previews at editor launch; and we never want
        // vertex lit previews if that is chosen in the player settings)
        camera.renderingPath = RenderingPath.Forward;
        camera.useOcclusionCulling = false;
        camera.scene = m_Scene;
    }

    public Camera camera
    {
        get { return m_Camera; }
    }

    public Scene scene
    {
        get { return m_Scene; }
    }

    public void AddGameObject(GameObject go)
    {
        if (m_GameObjects.Contains(go))
            return;

        SceneManager.MoveGameObjectToScene(go, m_Scene);
        m_GameObjects.Add(go);
    }

    public void AddManagedGO(GameObject go)
    {
        SceneManager.MoveGameObjectToScene(go, m_Scene);
    }

    public void Dispose()
    {
        EditorSceneManager.ClosePreviewScene(m_Scene);

        foreach (var go in m_GameObjects)
            Object.DestroyImmediate(go);

        m_GameObjects.Clear();
    }
}
