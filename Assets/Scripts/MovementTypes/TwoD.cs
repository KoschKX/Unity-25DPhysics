using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BansheeGz.BGSpline.Curve;
using BansheeGz.BGSpline.Components;
using UnityEngine.UI;

namespace CornEngine
{

  public class TwoD : ControlType {

    private Vector3 vel=Vector3.zero;
    private Vector3 fwd=Vector3.zero;

    public override Vector3 CalculateDirection()
    {
        Vector3 velocity = Vector3.zero;
        controller.PathOffset=0f;
        controller.fPoint=PathMath.FwdFromCPoint(controller._math, PathMath.GetFlatPos(_rigidbody.position,controller.cPoint.y),  out controller.cPoint);
        Vector3 zOffDir=(Quaternion.Euler(0,-90,0)*controller.fPoint);
        Vector3 zOff=zOffDir*controller.PathOffset;
        float hInput=controller.GetHorizontalMovementInput();
        if(camfollow!=null&&camfollow.Offset.z>0f){
           hInput=-hInput;
        }
        if (hInput > 0f)
        {
            velocity=controller.fPoint * hInput;
            fwd.x=velocity.x;fwd.y=0;fwd.z=velocity.z;
            controller.forward=fwd.normalized;
            controller.pathForward=controller.forward;
            controller.LookForward(transform,controller.forward*360);
            controller.isFacingRight=true;
        } else if (hInput < 0f)
        {
            velocity=controller.fPoint * hInput;
            fwd.x=velocity.x;fwd.y=0;fwd.z=velocity.z;
            controller.forward=fwd.normalized;
            controller.pathForward=controller.forward;
            controller.LookForward(transform,controller.forward*360);
            controller.isFacingRight=false;
        }
        if(velocity.magnitude > 1f)
            velocity.Normalize();
        return velocity;
    }

    public override void BindToPath(bool force=false){
        if(_rigidbody==null){return;}
        if (_rigidbody.velocity.magnitude < 0.1f){return;}
        controller.fPoint=PathMath.FwdFromCPoint(controller._math, PathMath.GetFlatPos(_rigidbody.position,controller.cPoint.y),  out controller.cPoint);
        Vector3 lPoint= new Vector3(controller.cPoint.x,_rigidbody.position.y,controller.cPoint.z);
        Vector3 dir = PathMath.GetFlatPos(controller.forward).normalized;
        LookForward(transform,dir*360);
        if(force){
          _rigidbody.MovePosition(lPoint);
        }else{
          _rigidbody.MovePosition(Vector3.Lerp(_rigidbody.position,lPoint,Time.fixedDeltaTime*3f));
        }
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
    }

  }
}
