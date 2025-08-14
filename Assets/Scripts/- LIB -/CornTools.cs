using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
	using UnityEditor;
  using UnityEditor.SceneManagement;
#endif

namespace CornEngine
{
    public class CornTools : MonoBehaviour
    {


/* MESSAGES */

	        public static void PrintMessage(string message,GameObject obj=null){
	            if(obj==null){
	               Debug.Log(message);
	            }else{
	               Debug.Log("["+obj.name+"]"+" "+message);
	            }
	         }

/*** MATH ***/

					public static float CalcEulerSafe(float x)
					{
							 if (x >= -90 && x <= 90)
									 return x;
							 x = x % 180;
							 if (x > 0)
									 x -= 180;
							 else
							 	 	 x += 180;
							 return x;
					}

					public static bool CheckInfinity(Vector3 vec){
							 if(Mathf.Abs(vec.x)==Mathf.Infinity
								 ||Mathf.Abs(vec.y)==Mathf.Infinity
								 ||Mathf.Abs(vec.z)==Mathf.Infinity
							 ){
									 return true;
							 }else{
									 return false;
							 }
					 }

					 public static float normalize(float value, float min, float max) {
						     float normalized = (value - min) / (max - min);
						     return normalized;
					 }

/*** VECTOR ***/

							static Vector3 ProjectRotationOntoVector(Quaternion rotation, Vector3 vector)
							{
									 Vector3 perpendicularVector = Vector3.Cross(vector, Vector3.forward);
									 Vector3 rotatedPerpendicularVector = rotation * perpendicularVector;
									 Vector3 projection = Vector3.Project(rotatedPerpendicularVector, vector);
									 return projection;
							}

						 public  static Vector3 CenterOfVectors( Vector3[] vectors )
						 {
										Vector3 sum = Vector3.zero;
										if( vectors == null || vectors.Length == 0 )
										{
												return sum;
										}

										foreach( Vector3 vec in vectors )
										{
												sum += vec;
										}
										return sum/vectors.Length;
						 }

						 public static Vector3 Vector3Abs(Vector3 v){
							 	 return (Vector3.right*(Mathf.Abs(v.x)))+(Vector3.up*(Mathf.Abs(v.y)))+(Vector3.forward*(Mathf.Abs(v.z)));
						 }

						public static Vector3 ConvertWorldToLocalVector(Transform trn,Vector3 v)
						{
									Vector3 c;
									c = trn.right * v.x + trn.up * v.y;
									return c;
						}

						public static Vector3 ConvertToLocalSpace(Transform trn, Vector3 direction) {
									return trn.InverseTransformDirection(direction);
						}

						public static Vector3 SetSelectedAxis(Vector3 original, Vector3 desired, Vector3 selectedAxis)
						{
							for(int i = 0; i < 3; i++)
							{
									if(selectedAxis[i] != 0)
									{
											selectedAxis[i] = desired[i];
									}else{
											selectedAxis[i] = original[i];
									}
							}
							return selectedAxis;
						}

						public static Vector3 Mul(Vector3 v, Vector3 a)
						{
								 return new Vector3(v.x * a.x, v.y * a.y, v.z * a.z);
						}

						public static Vector3 GetDir(Vector3 a, Vector3 b){
								 return (a-b).normalized;
						}




/*** QUATERNION ***/

						public static bool CheckFacingPosition(Vector3 direction, Vector3 vectorOfOrigin, Vector3 vectorOfInterest, float thresh){
								if(Vector3.Angle(direction, vectorOfOrigin - vectorOfInterest) < thresh){
									return true;
								}else{
									return false;
								}
						}

						public static bool CheckFacingAngle(Vector3 vector, Vector3 vectorOfInterest, float thresh){
								float AngleDiff =  Vector3.Angle(vectorOfInterest, vector);
								if(AngleDiff<thresh){
									return true;
								}else{
									return false;
								}
						}

						private static Vector3[] faces = new Vector3[6] ;
						public static Quaternion RotateToClosestSide(Transform trn, Vector3 dir)
						{
								faces[0]=trn.forward;faces[1]=-trn.forward;faces[2]=trn.right;faces[3]=-trn.right;faces[4]=trn.up;faces[5]=-trn.up;
								int closestFace = 0;
								float maxDot = Vector3.Dot(faces[0], dir);
								for (int i = 1; i < faces.Length; i++)
								{
										float dot = Vector3.Dot(faces[i], dir);
										if (dot > maxDot)
										{
												maxDot = dot;
												closestFace = i;
										}
								}
								Quaternion rotation = Quaternion.FromToRotation(faces[closestFace], dir);
								return rotation * trn.rotation;
						 }

						 public static Quaternion RotateToClosestSide(Transform trn, Vector3 dir, bool lockX, bool lockY, bool lockZ, Quaternion initialRotation)
						 {
								 Quaternion currentRotation = trn.rotation;
								 faces[0]=trn.forward;faces[1]=-trn.forward;faces[2]=trn.right;faces[3]=-trn.right;faces[4]=trn.up;faces[5]=-trn.up;
								 int closestFace = 0;
								 float maxDot = Vector3.Dot(faces[0], dir);
								 for (int i = 1; i < faces.Length; i++)
								 {
										 float dot = Vector3.Dot(faces[i], dir);
										 if (dot > maxDot)
										 {
												 maxDot = dot;
												 closestFace = i;
										 }
								 }
								 Quaternion rotation = Quaternion.FromToRotation(faces[closestFace], dir);
								 Quaternion finalRotation = rotation * currentRotation;
								 finalRotation=LockRotation(currentRotation,initialRotation, finalRotation,lockX, lockY, lockZ);
								 return finalRotation;
						 }

						 public static Quaternion LockRotation(Quaternion currentRotation, Quaternion initialRotation, Quaternion finalRotation, bool lockX, bool lockY, bool lockZ){
									 if (!lockX) {
											 finalRotation.eulerAngles = (Vector3.right*currentRotation.eulerAngles.x)+(Vector3.up*finalRotation.eulerAngles.y)+(Vector3.forward*finalRotation.eulerAngles.z);
									 }else{
											 finalRotation.eulerAngles = (Vector3.right*initialRotation.eulerAngles.x)+(Vector3.up*finalRotation.eulerAngles.y)+(Vector3.forward*finalRotation.eulerAngles.z);
									 }
									 if (!lockY) {
											 finalRotation.eulerAngles = (Vector3.right*finalRotation.eulerAngles.x)+(Vector3.up*currentRotation.eulerAngles.y)+(Vector3.forward*finalRotation.eulerAngles.z);
									 }
									 if (!lockZ) {
											 finalRotation.eulerAngles = (Vector3.right*finalRotation.eulerAngles.x)+(Vector3.up*finalRotation.eulerAngles.y)+(Vector3.forward*currentRotation.eulerAngles.z);
									 }else if (!lockZ) {
											 finalRotation.eulerAngles = (Vector3.right*finalRotation.eulerAngles.x)+(Vector3.up*finalRotation.eulerAngles.y)+(Vector3.forward*initialRotation.eulerAngles.z);
									 }
									 return finalRotation;
						 }

						public static Quaternion GetXAxisRotation(Quaternion quaternion)
						{
									float a = Mathf.Sqrt((quaternion.w * quaternion.w) + (quaternion.x * quaternion.x));
									return new Quaternion(x: quaternion.x, y: 0, z: 0, w: quaternion.w / a).normalized;
						}

						public static Quaternion GetYAxisRotation(Quaternion quaternion)
						{
									float a = Mathf.Sqrt((quaternion.w * quaternion.w) + (quaternion.y * quaternion.y));
									return new Quaternion (x: 0, y: quaternion.y, z: 0, w: quaternion.w / a).normalized;
						}
						public static Quaternion GetZAxisRotation(Quaternion quaternion)
						{
									float a = Mathf.Sqrt((quaternion.w * quaternion.w) + (quaternion.z * quaternion.z));
									return new Quaternion(x: 0, y: 0, z: quaternion.z, w: quaternion.w / a).normalized;
						}
						public static void UndoRotationX(Transform trn, Quaternion initialRotation)
						{
									trn.rotation = Quaternion.Euler(initialRotation.eulerAngles.x, trn.rotation.eulerAngles.y, trn.rotation.eulerAngles.z);
						}

						public static void UndoRotationY(Transform trn, Quaternion initialRotation)
						{
									trn.rotation = Quaternion.Euler(trn.rotation.eulerAngles.x, initialRotation.eulerAngles.y, trn.rotation.eulerAngles.z);
						}

						public static void UndoRotationZ(Transform trn, Quaternion initialRotation)
						{
									trn.rotation = Quaternion.Euler(trn.rotation.eulerAngles.x, trn.rotation.eulerAngles.y, initialRotation.eulerAngles.z);
						}

						public static float ComputeXAngle(Quaternion q)
						{
									float sinr_cosp = 2 * (q.w * q.x + q.y * q.z);
									float cosr_cosp = 1 - 2 * (q.x * q.x + q.y * q.y);
									return (Mathf.Atan2(sinr_cosp, cosr_cosp))* Mathf.Rad2Deg;
						}

						public static float ComputeYAngle(Quaternion q)
						{
									float sinp = 2 * (q.w * q.y - q.z * q.x);
									if (Mathf.Abs(sinp) >= 1)
											return (Mathf.PI / 2 * Mathf.Sign(sinp))* Mathf.Rad2Deg; // use 90 degrees if out of range
									else
											return (Mathf.Asin(sinp))* Mathf.Rad2Deg;
						}

						public static float ComputeZAngle(Quaternion q)
						{
									float siny_cosp = 2 * (q.w * q.z + q.x * q.y);
									float cosy_cosp = 1 - 2 * (q.y * q.y + q.z * q.z);
									return (Mathf.Atan2(siny_cosp, cosy_cosp)* Mathf.Rad2Deg);
						}

						public static float CalculateWComponent(Quaternion s){
									float sumOfSquares = s.x * s.x + s.y * s.y + s.z * s.z;
									float w = Mathf.Sqrt(Mathf.Max(0.0f, 1.0f - sumOfSquares));
									return w;
						}

						public static Quaternion ShortestRotation(Quaternion a, Quaternion b)
						{
							if (Quaternion.Dot(a, b) < 0)
							{
									return a * Quaternion.Inverse(MultiplyQuarternion(b, -1));
							}
							else return a * Quaternion.Inverse(b);
						}

						public static Quaternion MultiplyQuarternion(Quaternion input, float scalar)
						{
								 return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
						}

						public static Quaternion QuaternionDiff(Quaternion to, Quaternion from)
						{
								 return to * Quaternion.Inverse(from);
						}

						public static Quaternion QuaternionAdd(Quaternion start, Quaternion diff)
						{
								 return diff * start;
						}

/*** ANGLE ***/

						public static Vector3 ComputeAngles(Quaternion q)
						{
									return new Vector3(ComputeXAngle(q), ComputeYAngle(q), ComputeZAngle(q));
						}

						public static float NormalizeAngle(float angle) {
								 while (angle > 360)
										 angle -= 360;
								 while (angle < 0)
										 angle += 360;
								 return angle;
						}

						public static float GetAngleFromDir(Vector3 v) {
								 v.Normalize();
								 float ang = Mathf.Asin(v.x) * Mathf.Rad2Deg;
								 if (v.y < 0){
									 ang = 180 - ang;
								 }
								 else
								 if (v.x < 0){
									 ang = 360 + ang;
								 }
								 return ang;
						}

						public static float GetDirAngle(Vector3 fwd, Vector3 targetDir, Vector3 up) {
								Vector3 perp = Vector3.Cross(fwd, targetDir);
								float dir = Vector3.Dot(perp, up);
								if (dir > 0f) {
										return 1f;
								} else if (dir < 0f) {
										return -1f;
								} else {
										return 0f;
								}
						}

						public static float ClampAngle(float angle, float from, float to)
						{
								 if (angle < 0f) angle = 360 + angle;
								 if (angle > 180f) return Mathf.Max(angle, 360+from);
								 return Mathf.Min(angle, to);
						}

						public static float ClampAngle2 (float angle, float min, float max)
						{
								 angle = angle % 360;
								 if ((angle >= -360F) && (angle <= 360F)) {
										 if (angle < -360F) {
												 angle += 360F;
										 }
										 if (angle > 360F) {
												 angle -= 360F;
										 }
								 }
								 return Mathf.Clamp (angle, min, max);
						}

						public static float ClampAngle3(float angle, float min, float max) {
								 angle = NormalizeAngle(angle);
								 if (angle > 180) {
										 angle -= 360;
								 } else if (angle < -180) {
										 angle += 360;
								 }
								 min = NormalizeAngle(min);
								 if (min > 180) {
										 min -= 360;
								 } else if (min < -180) {
										 min += 360;
								 }
								 max = NormalizeAngle(max);
								 if (max > 180) {
										 max -= 360;
								 } else if (max < -180) {
										 max += 360;
								 }
								 return Mathf.Clamp(angle, min, max);
						 }

/*** PHYSICS ***/

					public static Vector3 RemoveCrossVelocity(Transform trn, Vector3 v)
					{
							 Vector3 crossVelocity = Vector3.Project(v, Vector3.Cross(trn.right, trn.up));
							 return v -= crossVelocity;
					}

					public static Vector3 AngularvelocityToImpulse(Rigidbody rb, Vector3 vel, Vector3 position)
					{
							 Vector3 R = position - rb.worldCenterOfMass;
							 Vector3 Q = MultiplyByInertiaTensor(rb, vel);
							 if (Mathf.Abs(Vector3.Dot(Q, R)) > 1e-5)
									return new Vector3();
							 return 0.5f * Vector3.Cross(Q, R) / R.sqrMagnitude;
					}

					public static Vector3 MultiplyByInertiaTensor(Rigidbody rb, Vector3 v)
					{
							 return rb.rotation * Mul(Quaternion.Inverse(rb.rotation) * v, rb.inertiaTensor);
					}

					 public static bool CheckStatic(Rigidbody rb, float thresh){
							 return (Mathf.Abs(rb.velocity.magnitude)<thresh&&Mathf.Abs(rb.angularVelocity.magnitude)<thresh);
					 }

					 public static void Lock(Rigidbody rb, out bool isLocked){
	             rb.constraints=RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY  | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY  | RigidbodyConstraints.FreezeRotationZ;
	             rb.isKinematic=true;
							 isLocked=true;
	         }

					 public static void Unlock(Rigidbody rb, out bool isLocked,RigidbodyConstraints cstr=RigidbodyConstraints.None){
	             rb.isKinematic=false;
	             rb.constraints=cstr;
							 isLocked=false;
	         }

/*** PATH ***/

					 public static bool CheckPathStatic(Rigidbody rb, Vector3 oldPos, float thresh){
							 Vector3 rPos=PathMath.GetFlatPos(rb.position);
							 Vector3 rOPos=PathMath.GetFlatPos(oldPos);
							 if(rPos==rOPos){return true;}
							 float rThr=thresh;
							 if(thresh<=0.1){rThr=thresh*10;}
							 if(thresh<=0.01){rThr=thresh*100;}
							 if(thresh<=0.001){rThr=thresh*1000;}
							 rPos=(Vector3.right*Mathf.Round(rPos.x*rThr))+(Vector3.up*Mathf.Round(rPos.y*rThr))+(Vector3.forward*Mathf.Round(rPos.z*rThr));
							 rOPos=(Vector3.right*Mathf.Round(rOPos.x*rThr))+(Vector3.up*Mathf.Round(rOPos.y*rThr))+(Vector3.forward*Mathf.Round(rOPos.z*rThr));
							 return (rPos==rOPos&&Mathf.Abs(rb.velocity.x)<thresh&&Mathf.Abs(rb.velocity.z)<thresh);
					 }

					 public static void ClearVelocity(Rigidbody rb){
							 rb.velocity=Vector3.zero;rb.angularVelocity=Vector3.zero;
					 }

					 public static void ClearPathVelocity(Rigidbody rb){
							 rb.velocity=(Vector3.right*0)+(Vector3.up*rb.velocity.y)+(Vector3.forward*0);
					  	 rb.angularVelocity=(Vector3.right*0)+(Vector3.up*rb.angularVelocity.y)+(Vector3.forward*0);
					 }

					 public static bool CheckFacingPath(Rigidbody rb, Vector3 vdir){
							 Vector3 vel=rb.velocity;vel.y=0;
							 if(vel.magnitude>0.1f&&(CheckFacingAngle(vel, vdir, 25f)||CheckFacingAngle(vel, -vdir, 25f))){
									return true;
							 }
							 return false;
					 }


/*** CAMERA ***/

           public static float CalculateOrthographicCam(float distance,float fov)
           {
             return distance * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
           }

           public static float CalculateOrthographicWidth(float width){
             return width * Screen.height / Screen.width * 0.5f;
           }

           public static float CalculateOrthographicSize(Vector3 size)
           {
               float screenRatio = Screen.width / Screen.height;
               float targetRatio = size.x / size.z;

               if (screenRatio >= targetRatio)
               {
                   return size.z / 2;
               }
               else
               {
                   float differenceInSize = targetRatio / screenRatio;
                   return size.z / 2 * differenceInSize;
               }
           }

					 private static Plane[] planes = {
					 		new Plane(Vector3.zero,Vector3.zero,Vector3.zero),
					 		new Plane(Vector3.zero,Vector3.zero,Vector3.zero),
					 		new Plane(Vector3.zero,Vector3.zero,Vector3.zero),
					 		new Plane(Vector3.zero,Vector3.zero,Vector3.zero),
					 		new Plane(Vector3.zero,Vector3.zero,Vector3.zero),
					 		new Plane(Vector3.zero,Vector3.zero,Vector3.zero)
					 };

					 public static Plane[] CalculateFrustum(Camera cam)
					 {
						  if(cam==null){return null;}
						  Vector3 origin = cam.transform.position;
						 	Vector3 direction = cam.transform.forward;
							float fovRadians = cam.fieldOfView*Mathf.Deg2Rad;
							float viewRatio = 1.0f;
							float distance = cam.farClipPlane;
					 		Vector3 nearCenter = origin + direction * 0.3f;
					 		Vector3 farCenter  = origin + direction * distance;
					 		Vector3 camRight   = Vector3.Cross(direction,Vector3.up) * -1;
					 		Vector3 camUp      = Vector3.Cross(direction,camRight);
					 		float nearHeight = 2 * Mathf.Tan(fovRadians / 2) * 0.3f;
					 		float farHeight  = 2 * Mathf.Tan(fovRadians / 2) * distance;
					 		float nearWidth  = nearHeight * viewRatio;
					 		float farWidth   = farHeight * viewRatio;
					 		Vector3 farTopLeft  = farCenter + camUp*(farHeight*0.5f) - camRight*(farWidth*0.5f);
					 		Vector3 farBottomLeft  = farCenter - camUp*(farHeight*0.5f) - camRight*(farWidth*0.5f);
					 		Vector3 farBottomRight = farCenter - camUp*(farHeight*0.5f) + camRight*(farWidth*0.5f);
					 		Vector3 nearTopLeft  = nearCenter + camUp*(nearHeight*0.5f) - camRight*(nearWidth*0.5f);
					 		Vector3 nearTopRight = nearCenter + camUp*(nearHeight*0.5f) + camRight*(nearWidth*0.5f);
					 		Vector3 nearBottomRight = nearCenter - camUp*(nearHeight*0.5f) + camRight*(nearWidth*0.5f);
					 		planes[0].Set3Points(nearTopLeft,farTopLeft,farBottomLeft);
					 		planes[1].Set3Points(nearTopRight,nearBottomRight,farBottomRight);
					 		planes[2].Set3Points(farBottomLeft,farBottomRight,nearBottomRight);
					 		planes[3].Set3Points(farTopLeft,nearTopLeft,nearTopRight);
					 		planes[3].Set3Points(nearBottomRight,nearTopRight,nearTopLeft);
					 		planes[3].Set3Points(farBottomRight,farBottomLeft,farTopLeft);
					 		return planes;
					}

/*** LAYERS ***/

          public static bool IsInLayerMask(GameObject obj, LayerMask layerMask)
          {
              return ((layerMask.value & (1 << obj.layer)) > 0);
          }
          public static void ClearCameraCullingMask(Camera camera){
              camera.cullingMask=~0;
          }

          public static void ToggleCameraCullingMask(Camera camera, LayerMask layer, bool on){
              if(on){
                	camera.cullingMask |= 1 << layer;
              }else{
                	camera.cullingMask &=  ~(1 << layer);
							}
          }

          public static void ToggleLightCullingMask(Light light, LayerMask layer , bool on){
              if(on){
                	light.cullingMask |= 1 << layer;
              }else{
               		light.cullingMask &=  ~(1 << layer);
              }
          }

          public static void addLayer(string layerName)
          {
              UnityEngine.Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
              if ((asset != null) && (asset.Length > 0))
              {
                  SerializedObject serializedObject = new SerializedObject(asset[0]);
                  SerializedProperty layers = serializedObject.FindProperty("layers");

                  for (int i = 0; i < layers.arraySize; ++i)
                  {
                      if (layers.GetArrayElementAtIndex(i).stringValue == layerName)
                      {
                          return;
                      }
                  }
                  for (int i = 0; i < layers.arraySize; i++)
                  {
                      if (layers.GetArrayElementAtIndex(i).stringValue == "")
                      {
                          layers.GetArrayElementAtIndex(i).stringValue = layerName;
                          serializedObject.ApplyModifiedProperties();
                          serializedObject.Update();
                          if (layers.GetArrayElementAtIndex(i).stringValue == layerName)
                          {
                              return;
                          }
                      }
                  }
              }
          }

					public static void SetGameLayerRecursive(GameObject _go, int _layer)
	        {
	            _go.layer = _layer;
	            foreach (Transform child in _go.transform)
	            {
	                child.gameObject.layer = _layer;
	                Transform _HasChildren = child.GetComponentInChildren<Transform>();
	                if (_HasChildren != null)
	                    SetGameLayerRecursive(child.gameObject, _layer);
	            }
	        }

/*** TAGS ***/

          public static void addTag(string tag)
           {
	             UnityEngine.Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
	             if ((asset != null) && (asset.Length > 0))
	             {
	                SerializedObject so = new SerializedObject(asset[0]);
	                SerializedProperty tags = so.FindProperty("tags");
	                for (int i = 0; i < tags.arraySize; ++i)
	                {
	                    if (tags.GetArrayElementAtIndex(i).stringValue == tag)
	                    {
	                        return;
	                    }
	                }
	                tags.InsertArrayElementAtIndex(0);
	                tags.GetArrayElementAtIndex(0).stringValue = tag;
	                so.ApplyModifiedProperties();
	                so.Update();
	            }
          }

/*** BOUNDS ***/

         public static Bounds TransformBounds( Transform _transform, Bounds _localBounds, bool inverse=false)
         {
	             Vector3 center;
	             Vector3 axisX = Vector3.zero;
	             Vector3 axisY = Vector3.zero;
	             Vector3 axisZ = Vector3.zero;
	             Vector3 extents = _localBounds.extents;
	             if(inverse){
	                center= _transform.InverseTransformPoint(_localBounds.center);
	                axisX = _transform.InverseTransformVector(extents.x, 0, 0);
	                axisY = _transform.InverseTransformVector(0, extents.y, 0);
	                axisZ = _transform.InverseTransformVector(0, 0, extents.z);
	             }else{
	                center= _transform.TransformPoint(_localBounds.center);
	                axisX = _transform.TransformVector(extents.x, 0, 0);
	                axisY = _transform.TransformVector(0, extents.y, 0);
	                axisZ = _transform.TransformVector(0, 0, extents.z);

	             }
	             extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
	             extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
	             extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);
	             return new Bounds { center = center, extents = extents };
          }

          public static Vector3 calculateTerrainCentroid(Terrain[] terrains)
       		{
	       			if(terrains==null||terrains.Length==0){return Vector3.zero;}
	       			Vector3 centroid = Vector3.zero;
	       				foreach(Terrain t in terrains)
	       				{
	       						if(t==null||t.terrainData==null){continue;}
	       						centroid += t.transform.position+t.transform.localPosition;
	       				}
	       				centroid /= (terrains.Length+1);
	       			return centroid;
       		}

          public static Bounds CalculateTerrainBounds(Terrain[] terrains){
      				if(terrains==null||terrains.Length==0){return new Bounds();}
      				Vector3 center = calculateTerrainCentroid(terrains);
      				Bounds bounds = new Bounds(center, Vector3.zero);
      				foreach (Terrain t in terrains)
      				{
      						if(t==null||t.terrainData==null){continue;}
      						Bounds n_bounds = new Bounds(center, Vector3.zero);
      						n_bounds.center=t.transform.position+(t.terrainData.bounds.extents);
      						n_bounds.extents=t.terrainData.bounds.extents;
									bounds.Encapsulate(n_bounds);
      				}
      				return bounds;
      		}

/*** TEXTURE ***/

          public static Texture2D GetSplatTexture(Terrain terrain, int idx) {
              if(terrain==null){return null;}
              return terrain.terrainData.GetAlphamapTexture(idx);
          }

          public static Texture2D ChangeFormat(Texture2D oldTexture, TextureFormat newFormat)
          {
              Texture2D newTex = new Texture2D(2, 2, newFormat, false);
              newTex.SetPixels(oldTexture.GetPixels());
              newTex.Apply();
              return newTex;
          }

          public static Texture2D ConvertToTransparent(Texture2D tex,float tolerance){
              Color[] colors = tex.GetPixels();
              for(int c = 0; c< colors.Length; c++){
                   if((colors[c].r + colors[c].g + colors[c].b) < tolerance){
                       colors[c].a = 0;
                   }
              }
              Texture2D transTex = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, false);
              transTex.SetPixels(colors);
              transTex.Apply();
              return transTex;
          }

          public static Texture2D ScaleTexture(Texture2D source,int targetWidth,int targetHeight) {
   		        Texture2D result=new Texture2D(targetWidth,targetHeight,source.format,true);
   		        Color[] rpixels=result.GetPixels(0);
   		        float incX=((float)1/source.width)*((float)source.width/targetWidth);
   		        float incY=((float)1/source.height)*((float)source.height/targetHeight);
   		        for(int px=0; px<rpixels.Length; px++) {
   		                rpixels[px] = source.GetPixelBilinear(incX*((float)px%targetWidth),
   		                                  incY*((float)Mathf.Floor(px/targetWidth)));
   		        }
   		        result.SetPixels(rpixels,0);
   		        result.Apply();
   		        return result;
     			}

          public static Texture2D SaveRenderTextureToFile(RenderTexture rendertexture,  bool debug=false)
          {
              RenderTexture.active = rendertexture;
              Texture2D tex = new Texture2D(rendertexture.width, rendertexture.height, TextureFormat.RGB24, false, true);
              tex.ReadPixels(new Rect(0, 0, rendertexture.width, rendertexture.height), 0, 0);
              tex.Apply();
              RenderTexture.active = null;

              byte[] bytes;
              bytes = tex.EncodeToPNG();

              string name = SceneManager.GetActiveScene().name;
              string dir = SceneManager.GetActiveScene().path.Replace(name+".unity","") + "/Colormaps/";
              string path = dir+name+"_colormap.png";

              if(!Directory.Exists(dir)){System.IO.Directory.CreateDirectory(dir);}
              //System.IO.File.Create(path);
              System.IO.File.WriteAllBytes(path, bytes);
              AssetDatabase.ImportAsset(path);

              if(debug){Debug.Log("Saved Texture to " + path);}

              return (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
           }

/*** TERRAIN ***/

					public static void ExportHeightmap(Terrain terrain, int resolution,  bool debug=false) {
							if(terrain.terrainData==null){return;}
							string name = SceneManager.GetActiveScene().name;
							string path = SceneManager.GetActiveScene().path.Replace(name+".unity","") + "Colormaps/"+name+"_heightmap.png";
							UnityEngine.TerrainData terraindata = terrain.terrainData;
							if (terraindata == null) {return;}
							byte[] myBytes;
							int idx = 0;
							var duplicateHeightMap = new Texture2D(terraindata.heightmapResolution, terraindata.heightmapResolution, TextureFormat.ARGB32, false);
							float[,] rawHeights =  terraindata.GetHeights(0, 0, terraindata.heightmapResolution, terraindata.heightmapResolution);
							for (int y=0; y < duplicateHeightMap.height; ++y)
							{
									for (int x=0; x < duplicateHeightMap.width; ++x)
									{
											Color color = new Vector4(rawHeights[x,y], rawHeights[x,y], rawHeights[x,y], 1.0f);
											duplicateHeightMap.SetPixel (x, y, color);
											idx++;
									}
							}
							duplicateHeightMap.Apply();
							duplicateHeightMap=CornTools.ScaleTexture(duplicateHeightMap,resolution,resolution);

							myBytes = duplicateHeightMap.EncodeToPNG();
							File.WriteAllBytes(path, myBytes);

							if(debug){Debug.Log("Saved Texture to " + path);}
					}


/*** ANIMATION ***/

				 public static bool AnimatorHasParameter(string paramName, Animator animator)
				 {
							foreach (AnimatorControllerParameter param in animator.parameters)
							{
								 if (param.name == paramName)
										return true;
							}
							return false;
				 }

 /*** EDITOR ***/

         public static bool IsUnityEditorFocused() {
            	return EditorWindow.mouseOverWindow != null;
         }

/*** IO ***/

          public string GetScriptPath(ScriptableObject self){
	            #if UNITY_EDITOR
	            	MonoScript ms = MonoScript.FromScriptableObject(self);
	            	string scriptFilePath = AssetDatabase.GetAssetPath(ms);
	            	FileInfo fi = new FileInfo(scriptFilePath);
	            	string scriptFolder = scriptFilePath.Replace(fi.Name,"");
	                 	return scriptFolder;
							#else
								return "";
	            #endif

          }

/*** UI ***/

					private static PointerEventData UIPointerEventData;
					private static List<RaycastResult> UIRaycastResults;
					public static bool IsPointerOverUIElement()
					{
							if(UIPointerEventData==null){UIPointerEventData=new PointerEventData(EventSystem.current);}
							UIPointerEventData.position = Input.mousePosition;
							if(UIRaycastResults==null){UIRaycastResults = new List<RaycastResult>();}
							UIRaycastResults.Clear();
							EventSystem.current.RaycastAll(UIPointerEventData, UIRaycastResults);
							return UIRaycastResults.Count > 0;
					}

					public static int GetDropdownIndexByName(Dropdown dropDown, string name)
					{
							return dropDown.options.FindIndex((i) => { return i.text.Equals(name); });
					}

/*** MESH ***/

				public static Mesh TransformMesh(Mesh mesh, Matrix4x4 matrix)
		    {
			        Vector3[] vertices = mesh.vertices;
			        for (int i = 0; i < vertices.Length; i++)
			        {
			            vertices[i] = matrix.MultiplyPoint(vertices[i]);
			        }
			        mesh.vertices=vertices;
			        return mesh;
		    }

				public static Mesh scaleMesh(Mesh mesh,float mesh_scale){
						 Vector3[] baseVertices = mesh.vertices;
						 Vector3[] vertices = new Vector3[baseVertices.Length];
						 for (int i=0;i<vertices.Length;i++)
						 {
							Vector3 vertex = baseVertices[i];
							vertex.x = vertex.x * mesh_scale;
							vertex.y = vertex.y * mesh_scale;
							vertex.z = vertex.z * mesh_scale;
								vertices[i] = vertex;
						 }
						 mesh.vertices = vertices;
						 mesh.RecalculateNormals();
						 mesh.RecalculateBounds();
						 return mesh;
				}

				public static Mesh CombineMesh(List<Mesh> meshes)
				{
						int vertexCount = 0;
						int uvCount = 0;
						int triangleCount = 0;
						for (int i = 0; i < meshes.Count; i++)
						{
								if (meshes[i] == null)
								{
										continue;
								}
								if (meshes[i].boneWeights == null || meshes[i].boneWeights.Length == 0)
								{
										meshes[i].boneWeights = new BoneWeight[meshes[i].vertices.Length];
								}
								vertexCount += meshes[i].vertices.Length;
								uvCount += meshes[i].uv.Length;
								triangleCount += meshes[i].triangles.Length;
						}
						var verts = new Vector3[vertexCount];
						var normals = new Vector3[vertexCount];
						var uvs = new Vector2[uvCount];
						var triangles = new int[triangleCount];
						var weights = new BoneWeight[vertexCount];
						var cols = new Color[vertexCount];
						int vertexOffset = 0;
						int uvOffset = 0;
						int triangleOffset = 0;
						for (int i = 0; i < meshes.Count; i++)
						{
								Array.Copy(meshes[i].vertices, 0, verts, vertexOffset, meshes[i].vertices.Length);
								Array.Copy(meshes[i].normals, 0, normals, vertexOffset, meshes[i].normals.Length);
								Array.Copy(meshes[i].uv, 0, uvs, uvOffset,meshes[i].uv.Length);
								Array.Copy(meshes[i].colors, 0, cols, vertexOffset, meshes[i].vertices.Length);
								for (int j = 0; j < meshes[i].triangles.Length; j++)
								{
										triangles[triangleOffset + j] = meshes[i].triangles[j] + vertexOffset;
								}
								Array.Copy(meshes[i].boneWeights, 0, weights, vertexOffset,meshes[i].vertices.Length);
								vertexOffset += meshes[i].vertices.Length;
								uvOffset += meshes[i].uv.Length;
								triangleOffset += meshes[i].triangles.Length;
						}
						Mesh mesh = new Mesh();
						mesh.Clear();
						mesh.vertices = verts;
						mesh.normals = normals;
						mesh.colors = cols;
						mesh.uv = uvs;
						mesh.triangles = triangles;
						mesh.boneWeights = weights;
						return mesh;
				}

				public static Mesh GetCurrentLODMesh(LODGroup lodGroup){
						if(lodGroup!=null){
								Transform lodTransform = lodGroup.transform;
								foreach (Transform lod in lodTransform)
								{
									var renderer = lod.GetComponent<Renderer>();
									if (renderer != null && renderer.isVisible)
									{
											if(lod.GetComponent<MeshFilter>()!=null){
												 MeshFilter _mfilter=lod.GetComponent<MeshFilter>();
												 return _mfilter.sharedMesh;
											}
									}
								}
						}
						return null;
				}

				public static Mesh RemoveVerticesFromMesh(Mesh oldMesh, Vector3[] delVerts){
		        Mesh mesh = Instantiate(oldMesh);
		        Vector3[] vertices = mesh.vertices;
		        List<Vector3> verticesToKeep = new List<Vector3>();
		        int[] triangles = mesh.triangles;
		        List<int> trianglesToKeep = new List<int>();
		        Dictionary<int, int> indexMap = new Dictionary<int, int>();
		        for (int i = 0; i < vertices.Length; i++)
		        {
		            bool keepVertex = true;
		            for(int v=0; v<delVerts.Length;v++){
		              if(vertices[i]==delVerts[v]){ keepVertex=false; break;}
		            }
		            if (keepVertex) { verticesToKeep.Add(vertices[i]); indexMap[i] = verticesToKeep.Count - 1; }
		        }
		        for (int i = 0; i < triangles.Length; i++)
		        {
		            if (indexMap.ContainsKey(triangles[i])) { trianglesToKeep.Add(indexMap[triangles[i]]); }
		        }
		        mesh.triangles = trianglesToKeep.ToArray();
		        mesh.vertices = verticesToKeep.ToArray();
		        mesh.RecalculateNormals();
		        Destroy(oldMesh);
		        return mesh;
			  }

				public static Vector3[] getVertsFromTriangle(Mesh mesh, int triangle){
						var allVertices = mesh.vertices;
						if(allVertices.Length==0){return new Vector3[3];}
						Vector3 p1=allVertices[triangle * 3 + 0];
						Vector3 p2=allVertices[triangle * 3 + 1];
						Vector3 p3=allVertices[triangle * 3 + 2];
						return new Vector3[] {p1,p2,p3};
				}
				public struct Edge
		    {
		        public int v1;
		        public int v2;
		        public int triangleIndex;
		        public Edge(int aV1, int aV2, int aIndex)
		        {
		            v1 = aV1;
		            v2 = aV2;
		            triangleIndex = aIndex;
		        }
		    }

		    public static List<Edge> GetEdges(int[] aIndices)
		     {
		         List<Edge> result = new List<Edge>();
		         for (int i = 0; i < aIndices.Length; i += 3)
		         {
		             int v1 = aIndices[i];
		             int v2 = aIndices[i + 1];
		             int v3 = aIndices[i + 2];
		             result.Add(new Edge(v1, v2, i));
		             result.Add(new Edge(v2, v3, i));
		             result.Add(new Edge(v3, v1, i));
		         }
		         return result;
		     }

		     public static List<Edge> FindBoundary(List<Edge> aEdges)
		     {
		         List<Edge> result = new List<Edge>(aEdges);
		         for (int i = result.Count-1; i > 0; i--)
		         {
		             for (int n = i - 1; n >= 0; n--)
		             {
		                 if (result[i].v1 == result[n].v2 && result[i].v2 == result[n].v1)
		                 {
		                     result.RemoveAt(i);
		                     result.RemoveAt(n);
		                     i--;
		                     break;
		                 }
		             }
		         }
		         return result;
		     }

		     public static List<Edge> SortEdges(List<Edge> aEdges)
		     {
		         List<Edge> result = new List<Edge>(aEdges);
		         for (int i = 0; i < result.Count-2; i++)
		         {
		             Edge E = result[i];
		             for(int n = i+1; n < result.Count; n++)
		             {
		                 Edge a = result[n];
		                 if (E.v2 == a.v1)
		                 {
		                     if (n == i+1)
		                         break;
		                     result[n] = result[i + 1];
		                     result[i + 1] = a;
		                     break;
		                 }
		             }
		         }
		         return result;
		     }

/*** OBJECTS ***/

				public static Transform FindChildByRecursion(Transform aParent, string aName)
        {
             if (aParent == null) return null;
             var result = aParent.Find(aName);
             if (result != null)
                 return result;
             foreach (Transform child in aParent)
             {
                 result = CornTools.FindChildByRecursion(child,aName);
                 if (result != null)
                     return result;
             }
             return null;
        }

        public static GameObject GetChildWithName(GameObject obj, string name) {
             Transform trans = obj.transform;
             Transform childTrans = trans. Find(name);
             if (childTrans != null) {
                 return childTrans.gameObject;
             } else {
                 return null;
             }
        }

        public static void DestroyAll(string tag)
        {
          GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
          for(int i=0; i< objs.Length; i++)
          {
            DestroyImmediate(objs[i]);
          }
        }

/*** CONVERSION ***/

				public static byte[] ToByteArray(float[,] nmbs)
				{
						int k = 0; byte[] nmbsBytes = new byte[nmbs.GetLength(0) * nmbs.GetLength(1) * 4];
						for (int i = 0; i < nmbs.GetLength(0); i++)
						{
							for (int j = 0; j < nmbs.GetLength(1); j++)
							{
									byte[] array = System.BitConverter.GetBytes(nmbs[i, j]);
									for (int m = 0; m < array.Length; m++)
									{
											nmbsBytes[k++] = array[m];
									}
							}
						}
						return nmbsBytes;
				}

				public Color32 IntToColor32(uint aCol)
				{
						Color32 c=new Color32(0, 0, 0, 0);
						c.b = (byte)((aCol) & 0xFF);
						c.g = (byte)((aCol>>8) & 0xFF);
						c.r = (byte)((aCol>>16) & 0xFF);
						c.a = (byte)((aCol>>24) & 0xFF);
						return c;
				}

	}
}
