using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace PathCreation
{
	[CreateAssetMenu(menuName = "SerializedVertexPath")]
	public class SerializedVertexPath : ScriptableObject
	{
		#region VertexPath Fields Copy
		
		[SerializeField]
		private PathSpace space;
		[SerializeField]
		private bool isClosedLoop;
		[SerializeField]
		private Vector3[] localPoints;
		[SerializeField]
		private Vector3[] localTangents;
		[SerializeField]
		private Vector3[] localNormals;

		/// Percentage along the path at each vertex (0 being start of path, and 1 being the end)
		[SerializeField]
		private float[] times;
		/// Total distance between the vertices of the polyline
		[SerializeField]
		private float length;
		/// Total distance from the first vertex up to each vertex in the polyline
		[SerializeField]
		private float[] cumulativeLengthAtEachVertex;
		/// Bounding box of the path
		[SerializeField]
		private Bounds bounds;
		/// Equal to (0,0,-1) for 2D paths, and (0,1,0) for XZ paths
		[SerializeField]
		private Vector3 up;

		//-----
		//Transform reduced to its components
		[SerializeField]
		private Vector3 transformPosition;
		[SerializeField]
		private Quaternion transformRotation;
		[SerializeField]
		private Vector3 transformLossyScale;

		#endregion


		public void CopyVertexPathData(VertexPath vertexPath)
		{
			space = vertexPath.space;
			isClosedLoop = vertexPath.isClosedLoop;
			localPoints = CopyArray(vertexPath.localPoints);
			localTangents = CopyArray(vertexPath.localTangents);
			localNormals = CopyArray(vertexPath.localNormals);

			times = CopyArray(vertexPath.times);

			length = vertexPath.length;

			cumulativeLengthAtEachVertex = CopyArray(vertexPath.cumulativeLengthAtEachVertex);

			bounds = vertexPath.bounds;

			up = vertexPath.up;

			transformPosition = vertexPath.LinkedTransform.position;
			transformRotation = vertexPath.LinkedTransform.rotation;
			transformLossyScale = vertexPath.LinkedTransform.lossyScale;
		}


		private T[] CopyArray<T>(T[] existingArray)
		{
			T[] newArray = new T[existingArray.Length];
			existingArray.CopyTo(newArray, 0);
			return newArray;
		}
	}
}
