using System;
using System.Collections;
using System.Collections.Generic;
using PathCreation;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace PathCreation
{

    [ExecuteAlways]
    public class SerializedVertexPathAutoWriter : MonoBehaviour
    {
        public SerializedVertexPath serializedVertexPath;

        private PathCreator pathCreator;


        public void Awake()
        {
            #if UNITY_EDITOR
            pathCreator = GetComponent<PathCreator>();

            if (pathCreator == null)
            {
                Debug.LogWarning("SerializedVertexPathAutoWriter can't find PathCreator script");
                return;
            }
            #endif
        }

        public void Start()
        {
            #if UNITY_EDITOR
            pathCreator.pathUpdated += OnPathUpdated;
            #endif
        }

        #if UNITY_EDITOR
        private void OnPathUpdated()
        {
            if (pathCreator != null && serializedVertexPath != null)
            {
                serializedVertexPath.CopyVertexPathData(pathCreator.path);
                EditorUtility.SetDirty(serializedVertexPath);
            }
        }
        #endif
    }
}
