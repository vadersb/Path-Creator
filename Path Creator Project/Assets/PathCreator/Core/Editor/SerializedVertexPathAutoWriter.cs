using System;
using System.Collections;
using System.Collections.Generic;
using PathCreation;
using UnityEditor;
using UnityEngine;


namespace PathCreationEditor
{

    [ExecuteAlways]
    public class SerializedVertexPathAutoWriter : MonoBehaviour
    {
        public SerializedVertexPath serializedVertexPath;

        private PathCreator pathCreator;


        public void Awake()
        {
            pathCreator = GetComponent<PathCreator>();

            if (pathCreator == null)
            {
                Debug.LogWarning("SerializedVertexPathAutoWriter can't find PathCreator script");
                return;
            }
        }

        public void Start()
        {
            pathCreator.pathUpdated += OnPathUpdated;
        }


        private void OnPathUpdated()
        {
            if (pathCreator != null && serializedVertexPath != null)
            {
                serializedVertexPath.CopyVertexPathData(pathCreator.path);
                EditorUtility.SetDirty(serializedVertexPath);
            }
        }
    }
}
