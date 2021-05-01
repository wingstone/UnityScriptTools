using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
#endif

//https://forum.unity.com/threads/onscenegui-not-working-when-selecting-gameobject-with-selection-activeobject.902387/
namespace BoneTool.Script.Runtime
{
    [ExecuteInEditMode]
    public class BoneVisualiser : MonoBehaviour
    {
        public Transform RootNode;
        public float BoneGizmosSize = 0.01f;
        public Color BoneColor = Color.white;
        public bool HideRoot;

        private Transform _preRootNode;
        private Transform[] _childNodes;
        private BoneTransform[] _previousTransforms;

        public Transform[] GetChildNodes()
        {
            return _childNodes;
        }

#if UNITY_EDITOR

        private void OnScene(SceneView sceneview)
        {
            var shouldDraw = true;

            //Checking if the current game object is inside of a prefab stage
            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                shouldDraw = PrefabStageUtility.GetPrefabStage(gameObject) == PrefabStageUtility.GetCurrentPrefabStage();

            if (shouldDraw)
            {
                if (_childNodes == null || _childNodes.Length == 0 || _previousTransforms == null || _previousTransforms.Length == 0)
                    PopulateChildren();

                Handles.color = BoneColor;

                foreach (var node in _childNodes)
                {
                    if (!node.transform.parent) continue;
                    if (HideRoot && node == _preRootNode) continue;

                    var start = node.transform.parent.position;
                    var end = node.transform.position;

                    if (Handles.Button(node.transform.position, Quaternion.identity, BoneGizmosSize, BoneGizmosSize, Handles.SphereHandleCap))
                    {
                        Selection.activeGameObject = node.gameObject;
                    }

                    if (HideRoot && node.parent == _preRootNode) continue;

                    if (node.transform.parent.childCount == 1)
                        Handles.DrawAAPolyLine(5f, start, end);
                    else
                        Handles.DrawDottedLine(start, end, 0.5f);
                }
            }
        }

        private void OnEnable()
        {
            SceneView.onSceneGUIDelegate += OnScene;
        }

        private void OnDisable()
        {
            SceneView.onSceneGUIDelegate -= OnScene;
        }

#endif

        public void PopulateChildren()
        {
            if (RootNode == null)
                RootNode = transform;

            _preRootNode = RootNode;
            _childNodes = RootNode.GetComponentsInChildren<Transform>();
            _previousTransforms = new BoneTransform[_childNodes.Length];
            for (var i = 0; i < _childNodes.Length; i++)
            {
                var childNode = _childNodes[i];
                _previousTransforms[i] = new BoneTransform(childNode, childNode.localPosition);
            }
        }

        [Serializable]
        private struct BoneTransform
        {
            public Transform Target;
            public Vector3 LocalPosition;

            public BoneTransform(Transform target, Vector3 localPosition)
            {
                Target = target;
                LocalPosition = localPosition;
            }

            public void SetLocalPosition(Vector3 position)
            {
                LocalPosition = position;
            }
        }
    }
}