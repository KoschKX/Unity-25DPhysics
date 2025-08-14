using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BansheeGz.BGSpline.Curve;
using BansheeGz.BGSpline.Components;
using UnityEngine.UI;

namespace CornEngine
{

  public class TwoFiveD : ControlType {

    public override Vector3 CalculateDirection()
    {
      Vector3 _velocity = Vector3.zero;
      controller.fPoint=PathMath.FwdFromCPoint(controller._math, transform.position,  out controller.cPoint);
      float vInput=controller.GetVerticalMovementInput();
      float hInput=controller.GetHorizontalMovementInput();
      Vector3 vOffDir=(Quaternion.Euler(0,-90,0)*controller.fPoint);
      Vector3 vOff=vOffDir*vInput;
      if(camfollow!=null&&camfollow.Offset.z>0f){
         hInput=-hInput;
      }
      if (hInput > 0f)
      {
          _velocity=controller.fPoint * hInput;
          controller.pathForward=new Vector3(_velocity.x,0,_velocity.z).normalized;
          _velocity=(controller.fPoint * hInput)+vOff;
          controller.forward=new Vector3(_velocity.x,0,_velocity.z).normalized;
          controller.isFacingRight=true;

          controller.lastForward=controller.forward;
      } else if (hInput < 0f)
      {
          _velocity=controller.fPoint * hInput;
          controller.pathForward=new Vector3(_velocity.x,0,_velocity.z).normalized;
          _velocity=(controller.fPoint * hInput)+vOff;
          controller.forward=new Vector3(_velocity.x,0,_velocity.z).normalized;
          controller.isFacingRight=false;
          controller.lastForward=controller.forward;
      }else{
          _velocity=vOff;
          if(_velocity.magnitude > 0f){
              controller.forward=new Vector3(_velocity.x,0,_velocity.z).normalized;
          }else if(controller.LookTowardsPath){
              controller.forward=controller.pathForward;
          }
      }
      if(Mathf.Abs(vInput)>0f){
          controller.LookForward(transform,controller.forward*360);
          controller.lastForward=controller.forward;
      }else{
          if(controller.LookTowardsPath){
              controller.LookForward(transform,controller.pathForward*360);
          }else{
              controller.LookForward(transform,controller.lastForward*360);
          }
      }
      if(_velocity.magnitude > 1f)
          _velocity.Normalize();
          Vector3 rPos=PathMath.GetFlatPos(_rigidbody.position);
          Vector3 cPos=PathMath.GetFlatPos(controller.cPoint);
          Vector3 rDir = (rPos - cPos).normalized;
          float angleCheck=CornTools.GetDirAngle(controller.forward,rDir,Vector3.up);
          if(!controller.isFacingRight){angleCheck=-angleCheck;}
          if(angleCheck>0f){
              controller.PathOffset=-(Vector3.Distance(controller.cPoint,rPos));
          }else{
              controller.PathOffset=(Vector3.Distance(controller.cPoint,rPos));
          }
      return _velocity;
    }
    public override void BindToPath(bool force=false){
      if(_rigidbody==null){return;}
      if (_rigidbody.velocity.magnitude < 0.1f){return;}
      //Vector3 dir = new Vector3(controller.pathForward.x,0,controller.pathForward.z).normalized;
    }
  }
}
