using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BansheeGz.BGSpline.Curve;
using BansheeGz.BGSpline.Components;

namespace CornEngine
{
	//This is a static helper class that offers various methods for calculating and modifying vectors (as well as float values);
	public static class PathMath {

        public static Vector3 tempVecA=Vector3.zero;
        public static Vector3 tempVecB=Vector3.zero;

				public static Vector3 FwdFromCPoint(BGCurveBaseMath math, Vector3 outsidePoint, out Vector3 curvePoint, Vector3? offset = null)
        {
            if (offset == null){offset = Vector3.zero;}else{offset=(Vector3)offset;}
            Vector3 forward =TanFromCPoint(math, outsidePoint, out curvePoint, offset);
            forward.y = 0;
            return forward.normalized;
        }

        private static Vector3 TanFromCPoint(BGCurveBaseMath math, Vector3 outsidePoint, out Vector3 curvePoint, Vector3? offset = null)
        {
            if (offset == null){offset = Vector3.zero;}else{offset=(Vector3)offset;}
            float curvePointDistance;
            curvePoint = math.CalcPositionByClosestPoint(outsidePoint-(Vector3)offset, out curvePointDistance)+(Vector3)offset;
            Vector3 tangent = math.CalcTangentByDistance(curvePointDistance);
            return tangent;
        }

        public static Vector3 GetCoordinate(BGCurveBaseMath math, float pct){
            return math.CalcByDistanceRatio(BGCurveBaseMath.Field.Position, pct);
        }

				public static Vector3 GetFlatPos(Vector3 pos, float y=0){
					return (Vector3.right*pos.x)+(Vector3.up*y)+(Vector3.forward*pos.z);
				}
				
        public static float GetLength(BGCurveBaseMath math){
            return math.GetDistance();
        }

        public static Vector3 GetCoint(BGCurveBaseMath math, Vector3 pt){
            float curvePointDistance;
            return math.CalcPositionByClosestPoint(pt, out curvePointDistance);
        }

        public static float GetCPct(BGCurve path, BGCurveBaseMath math, Vector3 pt){
            float curvePointDistance;
            pt=math.CalcPositionByClosestPoint(pt, out curvePointDistance);
            float distance = Vector3.Distance(path[0].PositionLocal, pt);
            distance=Mathf.Clamp01(distance / math.GetDistance());
            return distance;
        }

        public static void RecalculatePath(BGCurveBaseMath math, bool force){
            math.Recalculate(force);
        }


	}
}
