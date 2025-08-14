using UnityEngine;
using System.Collections;

namespace CornEngine
{
	//This is a static helper class that offers various methods for calculating and modifying vectors (as well as float values);
	public static class DeltaMath {

		public static Vector3 RoundVec(Vector3 vec, float amt){
            float x = vec.x;
            float y = vec.y;
            float z = vec.z;
            return new Vector3(Mathf.Round(x * amt)/ amt,Mathf.Round(y * amt)/ amt,Mathf.Round(z * amt)/ amt);
        }
        public static Quaternion RoundQuat(Quaternion qau, float amt){
            float x = qau.x;
            float y = qau.y;
            float z = qau.z;
            float w = qau.w;

            return new Quaternion(Mathf.Round(x * amt)/ amt,Mathf.Round(y * amt)/ amt,Mathf.Round(z * amt)/ amt,w);
        }


        public static Quaternion QuarternationSmoothDamp(Quaternion rot, Quaternion target, ref Quaternion deriv, float time) {
            // account for double-cover
            var Dot = Quaternion.Dot(rot, target);
            var Multi = Dot > 0f ? 1f : -1f;
            target.x *= Multi;
            target.y *= Multi;
            target.z *= Multi;
            target.w *= Multi;
            // smooth damp (nlerp approx)
            var Result = new Vector4(
                Mathf.SmoothDamp(rot.x, target.x, ref deriv.x, time*Time.deltaTime),
                Mathf.SmoothDamp(rot.y, target.y, ref deriv.y, time*Time.deltaTime),
                Mathf.SmoothDamp(rot.z, target.z, ref deriv.z, time*Time.deltaTime),
                Mathf.SmoothDamp(rot.w, target.w, ref deriv.w, time*Time.deltaTime)
            ).normalized;
            // compute deriv
            var dtInv = 1f / Time.deltaTime;
            deriv.x = (Result.x - rot.x) * dtInv;
            deriv.y = (Result.y - rot.y) * dtInv;
            deriv.z = (Result.z - rot.z) * dtInv;
            deriv.w = (Result.w - rot.w) * dtInv;
            return new Quaternion(Result.x, Result.y, Result.z, Result.w);
        }

	}
}
