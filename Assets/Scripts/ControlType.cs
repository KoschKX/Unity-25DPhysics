using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BansheeGz.BGSpline.Curve;
using BansheeGz.BGSpline.Components;
using UnityEngine.UI;

namespace CornEngine
{
    public class ControlType
    {

      protected CharacterController controller;

      protected BGCurve path;
      protected BGCurveBaseMath math;
      protected BGCcBounds pathBounds;

      private Vector3 zOffDir = Vector3.zero;
      private Vector3 zOff = Vector3.zero;

      protected Camera camera;
      protected CameraFollower camfollow;

      protected Rigidbody _rigidbody;
      protected Collider _collider;

      protected Transform transform;

      public void Init(CharacterController c){

          controller = c;

          path = controller._path;
          math = controller._math;
          pathBounds = controller.pathBounds;

          camera = controller.cam;

          _rigidbody = controller._rigidbody;
          _collider = controller._collider;

          transform = controller.transform;
      }

      public Vector3 CalculatePathOffset(){
          Vector3 oldDir=zOffDir;
          Vector3 newDir=new Vector3(controller.fPoint.x,0,controller.fPoint.z);
          newDir=Quaternion.Euler(0,-90,0)*newDir;
          float d = Vector3.Dot(oldDir,newDir);
          zOffDir=Vector3.Lerp(oldDir, newDir, 0.5f);
          Vector3  zOffPt=zOffDir.normalized*controller.PathOffset;
          return zOffPt;
      }

      public bool IsTurning(){
        bool turning=false;
        if(_rigidbody!=null){
            float angleDiff=Mathf.Abs(_rigidbody.rotation.eulerAngles.y - controller.lastRotation.eulerAngles.y);
            if (angleDiff>1f){
                turning=true;
            }
            controller.lastRotation=_rigidbody.rotation;
        }
        return turning;
      }

      public void FacePath(bool isRight=true)
      {
          controller.fPoint=PathMath.FwdFromCPoint(math, transform.position,  out controller.cPoint);
          Vector3 velocity=Vector3.zero;
          if(isRight){
              velocity=controller.fPoint * 0.1f;
          }else{
              velocity=controller.fPoint * -0.1f;
          }
          controller.forward=new Vector3(velocity.x,0,velocity.z).normalized;
          controller.pathForward=controller.forward;
          LookForward(transform,controller.forward*360);
          controller.isFacingRight=true;
      }

      public void LookForward(Transform obj,Vector3 lookTarget, bool anim=false){
          if(anim){
              Vector3 targetDir = lookTarget - obj.position;
              Vector3 forward = obj.forward;
              Vector3 localTarget = transform.InverseTransformPoint(lookTarget);
              float angle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
              Vector3 eulerAngleVelocity = new Vector3 (0, angle, 0);
              Quaternion deltaRotation= Quaternion.Euler(eulerAngleVelocity * (Time.deltaTime*controller.movementSpeed));
              controller._rigidbody.MoveRotation(controller._rigidbody.rotation * deltaRotation);
          }else{
              obj.LookAt(lookTarget);
              float yRot = obj.eulerAngles.y;
              obj.eulerAngles=new Vector3(0,yRot,0);
          }
      }

      public float GetHorizontalMovementInput(bool forceRaw=false)
      {
          return Input.GetAxis(controller.horizontalInputAxis);
      }
      public float GetVerticalMovementInput(bool forceRaw=false)
      {
          return Input.GetAxis(controller.verticalInputAxis);
      }


      /* OVERRIDES */
         public virtual Vector3 CalculateDirection(){
            return Vector3.zero;
          }

         public virtual void BindToPath(bool force=false){
            return;
         }

    }

}
