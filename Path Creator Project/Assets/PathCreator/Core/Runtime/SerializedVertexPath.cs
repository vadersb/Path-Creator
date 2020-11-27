using System.Collections;
using System.Collections.Generic;
using PathCreation.Utility;
using UnityEngine;



namespace PathCreation
{
	[CreateAssetMenu(menuName = "SerializedVertexPath")]
	public class SerializedVertexPath : ScriptableObject
	{
		public event System.Action pathUpdated;
		
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

		public int NumPoints => localPoints.Length;

		public float Length => length;

		public PathSpace Space => space;

		public bool IsClosedLoop => isClosedLoop;

		public Bounds Bounds => bounds;

		public Vector3 Up => up;


		public Vector3 TransformPosition => transformPosition;
		public Quaternion TransformRotation => transformRotation;
		public Vector3 TransformLossyScale => transformLossyScale;


		public void CopyVertexPathData(VertexPath vertexPath, bool useLocalTransform)
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

			if (useLocalTransform)
			{
				transformPosition = vertexPath.LinkedTransform.localPosition;
				transformRotation = vertexPath.LinkedTransform.localRotation;
				transformLossyScale = vertexPath.LinkedTransform.localScale;
			}
			else
			{
				transformPosition = vertexPath.LinkedTransform.position;
				transformRotation = vertexPath.LinkedTransform.rotation;
				transformLossyScale = vertexPath.LinkedTransform.lossyScale;
			}
			
			pathUpdated?.Invoke();
		}



		
		//PUBLIC METHODS ADAPTED FROM VERTEX PATH
		#region Public methods and accessors
		
		public Vector3 GetTangent (int index) {
			return MathUtility.TransformDirection (localTangents[index], transformRotation, space);
		}

		public Vector3 GetNormal (int index) {
			return MathUtility.TransformDirection (localNormals[index], transformRotation, space);
		}
		
		public Vector3 GetPoint (int index) {
			return MathUtility.TransformPoint (localPoints[index], transformPosition, transformRotation, transformLossyScale, space);
		}
		
		/// Gets point on path based on distance travelled.
		public Vector3 GetPointAtDistance (float dst, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Loop) {
			float t = dst / length;
			return GetPointAtTime (t, endOfPathInstruction);
		}
		
		/// Gets forward direction on path based on distance travelled.
		public Vector3 GetDirectionAtDistance (float dst, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Loop) {
			float t = dst / length;
			return GetDirection (t, endOfPathInstruction);
		}
		
		/// Gets normal vector on path based on distance travelled.
		public Vector3 GetNormalAtDistance (float dst, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Loop) {
			float t = dst / length;
			return GetNormal (t, endOfPathInstruction);
		}
		
		/// Gets a rotation that will orient an object in the direction of the path at this point, with local up point along the path's normal
		public Quaternion GetRotationAtDistance (float dst, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Loop) {
			float t = dst / length;
			return GetRotation (t, endOfPathInstruction);
		}
		
		
		/// Gets point on path based on 'time' (where 0 is start, and 1 is end of path).
		public Vector3 GetPointAtTime (float t, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Loop) {
			var data = CalculatePercentOnPathData (t, endOfPathInstruction);
			return Vector3.Lerp (GetPoint (data.previousIndex), GetPoint (data.nextIndex), data.percentBetweenIndices);
		}
		
		/// Gets forward direction on path based on 'time' (where 0 is start, and 1 is end of path).
		public Vector3 GetDirection (float t, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Loop) {
			var data = CalculatePercentOnPathData (t, endOfPathInstruction);
			Vector3 dir = Vector3.Lerp (localTangents[data.previousIndex], localTangents[data.nextIndex], data.percentBetweenIndices);
			return MathUtility.TransformDirection (dir, transformRotation, space);
		}
		
		/// Gets normal vector on path based on 'time' (where 0 is start, and 1 is end of path).
		public Vector3 GetNormal (float t, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Loop) {
			var data = CalculatePercentOnPathData (t, endOfPathInstruction);
			Vector3 normal = Vector3.Lerp (localNormals[data.previousIndex], localNormals[data.nextIndex], data.percentBetweenIndices);
			return MathUtility.TransformDirection (normal, transformRotation, space);
		}
		
		/// Gets a rotation that will orient an object in the direction of the path at this point, with local up point along the path's normal
		public Quaternion GetRotation (float t, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Loop) {
			var data = CalculatePercentOnPathData (t, endOfPathInstruction);
			Vector3 direction = Vector3.Lerp (localTangents[data.previousIndex], localTangents[data.nextIndex], data.percentBetweenIndices);
			Vector3 normal = Vector3.Lerp (localNormals[data.previousIndex], localNormals[data.nextIndex], data.percentBetweenIndices);
			return Quaternion.LookRotation (MathUtility.TransformDirection (direction, transformRotation, space), MathUtility.TransformDirection (normal, transformRotation, space));
		}
		
		
		/// Finds the closest point on the path from any point in the world
		public Vector3 GetClosestPointOnPath (Vector3 worldPoint) {
			// Transform the provided worldPoint into VertexPath local-space.
			// This allows to do math on the localPoint's, thus avoiding the need to
			// transform each local vertexpath point into world space via GetPoint.
			Vector3 localPoint = MathUtility.InverseTransformPoint(worldPoint, transformPosition, transformRotation, transformLossyScale, space);

			VertexPath.TimeOnPathData data = CalculateClosestPointOnPathData (localPoint);
			Vector3 localResult = Vector3.Lerp (localPoints[data.previousIndex], localPoints[data.nextIndex], data.percentBetweenIndices);

			// Transform local result into world space
			return MathUtility.TransformPoint(localResult, transformPosition, transformRotation, transformLossyScale, space);
		}
		
		/// Finds the 'time' (0=start of path, 1=end of path) along the path that is closest to the given point
		public float GetClosestTimeOnPath (Vector3 worldPoint) {
			Vector3 localPoint = MathUtility.InverseTransformPoint(worldPoint, transformPosition, transformRotation, transformLossyScale, space);
			VertexPath.TimeOnPathData data = CalculateClosestPointOnPathData (localPoint);
			return Mathf.Lerp (times[data.previousIndex], times[data.nextIndex], data.percentBetweenIndices);
		}
		
		/// Finds the distance along the path that is closest to the given point
		public float GetClosestDistanceAlongPath (Vector3 worldPoint) {
			Vector3 localPoint = MathUtility.InverseTransformPoint(worldPoint, transformPosition, transformRotation, transformLossyScale, space);
			VertexPath.TimeOnPathData data = CalculateClosestPointOnPathData(localPoint);
			return Mathf.Lerp (cumulativeLengthAtEachVertex[data.previousIndex], cumulativeLengthAtEachVertex[data.nextIndex], data.percentBetweenIndices);
		}
		
		#endregion
		
		//TODO query methods that skip the transform part for those paths that use origin transform
		
		//INTERNAL METHODS COPIED FROM VERTEX PATH AS IS
		#region Internal methods 

        /// For a given value 't' between 0 and 1, calculate the indices of the two vertices before and after t.
        /// Also calculate how far t is between those two vertices as a percentage between 0 and 1.
        VertexPath.TimeOnPathData CalculatePercentOnPathData (float t, EndOfPathInstruction endOfPathInstruction) {
            // Constrain t based on the end of path instruction
            switch (endOfPathInstruction) {
                case EndOfPathInstruction.Loop:
                    // If t is negative, make it the equivalent value between 0 and 1
                    if (t < 0) {
                        t += Mathf.CeilToInt (Mathf.Abs (t));
                    }
                    t %= 1;
                    break;
                case EndOfPathInstruction.Reverse:
                    t = Mathf.PingPong (t, 1);
                    break;
                case EndOfPathInstruction.Stop:
                    t = Mathf.Clamp01 (t);
                    break;
            }

            int prevIndex = 0;
            int nextIndex = NumPoints - 1;
            int i = Mathf.RoundToInt (t * (NumPoints - 1)); // starting guess

            // Starts by looking at middle vertex and determines if t lies to the left or to the right of that vertex.
            // Continues dividing in half until closest surrounding vertices have been found.
            while (true) {
                // t lies to left
                if (t <= times[i]) {
                    nextIndex = i;
                }
                // t lies to right
                else {
                    prevIndex = i;
                }
                i = (nextIndex + prevIndex) / 2;

                if (nextIndex - prevIndex <= 1) {
                    break;
                }
            }

            float abPercent = Mathf.InverseLerp (times[prevIndex], times[nextIndex], t);
            return new VertexPath.TimeOnPathData (prevIndex, nextIndex, abPercent);
        }

        /// Calculate time data for closest point on the path from given world point
        VertexPath.TimeOnPathData CalculateClosestPointOnPathData (Vector3 localPoint) {
            float minSqrDst = float.MaxValue;
            Vector3 closestPoint = Vector3.zero;
            int closestSegmentIndexA = 0;
            int closestSegmentIndexB = 0;

            for (int i = 0; i < localPoints.Length; i++) {
                int nextI = i + 1;
                if (nextI >= localPoints.Length) {
                    if (isClosedLoop) {
                        nextI %= localPoints.Length;
                    } else {
                        break;
                    }
                }

                Vector3 closestPointOnSegment = MathUtility.ClosestPointOnLineSegment (localPoint, localPoints[i], localPoints[nextI]);
                float sqrDst = (localPoint - closestPointOnSegment).sqrMagnitude;
                if (sqrDst < minSqrDst) {
                    minSqrDst = sqrDst;
                    closestPoint = closestPointOnSegment;
                    closestSegmentIndexA = i;
                    closestSegmentIndexB = nextI;
                }

            }
            float closestSegmentLength = (localPoints[closestSegmentIndexA] - localPoints[closestSegmentIndexB]).magnitude;
            float t = (closestPoint - localPoints[closestSegmentIndexA]).magnitude / closestSegmentLength;
            return new VertexPath.TimeOnPathData (closestSegmentIndexA, closestSegmentIndexB, t);
        }

        #endregion

		
		
		private T[] CopyArray<T>(T[] existingArray)
		{
			T[] newArray = new T[existingArray.Length];
			existingArray.CopyTo(newArray, 0);
			return newArray;
		}
		
	}
}
