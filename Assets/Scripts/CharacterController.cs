using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BansheeGz.BGSpline.Curve;
using BansheeGz.BGSpline.Components;
using UnityEngine.UI;

namespace CornEngine
{
  public enum ControllerType
  {
      TwoD,
      TwoFiveD
  }

  [AddComponentMenu("CornEngine/CharacterController")]
  [RequireComponent(typeof(Rigidbody))]
  [RequireComponent(typeof(Collider))]
  public class CharacterController : MonoBehaviour{

    TwoD twoD;
    TwoFiveD twoFiveD;

    [Header("Components")]

      public BGCurve _path;
      [HideInInspector] public BGCurveBaseMath _math;
      [HideInInspector] public BGCcBounds pathBounds;
      [HideInInspector] public Vector3 zOffDir = Vector3.zero;
      [HideInInspector] public Vector3 zOff = Vector3.zero;
      [HideInInspector] public Rigidbody _rigidbody;
      [HideInInspector] public Collider _collider;
      [HideInInspector] public Camera cam;

    [Header("State")]

      public bool isGrounded;
      public bool isPushing;

      public float slideGravity = 5f;
      public float slopeLimit = 80f;

      public Vector4 speed = new Vector4(0,0,0,0);

      public float PathOffset = 0f;

      [HideInInspector] public Vector3 cPoint=Vector3.zero;
      [HideInInspector] public Vector3 fPoint=Vector3.zero;

      [HideInInspector] public Vector3 forward = Vector3.zero;
      [HideInInspector] public Vector3 pathForward = Vector3.zero;
      [HideInInspector] public Vector3 lastForward = Vector3.zero;
      [HideInInspector] public Quaternion lastRotation = Quaternion.identity;


    [Header("Type")]

      public ControllerType controllerType;
      [HideInInspector] public ControllerType lastControllerType;
      public bool LookTowardsPath=false;
      [HideInInspector] public bool isFacingRight;

      public string horizontalInputAxis = "Horizontal";
      public string verticalInputAxis = "Vertical";

      public float movementSpeed = 7f;
      public float yVelocity=0;
      public float yPosition=0;

    [Header("Animation")]

      [HideInInspector] public Animator animator;
      public static readonly int AnimGrounded = Animator.StringToHash("Grounded");
      public static readonly int AnimIdle = Animator.StringToHash("Idle");
      public static readonly int AnimRunning = Animator.StringToHash("Running");
      public static readonly int AnimJumping = Animator.StringToHash("Jumping");

    RaycastHit hit;


/*** MAIN ***/

      void Start () {

          Setup();
          FacePath();

          if(controllerType==ControllerType.TwoD){
            twoD.BindToPath(true);
          }else if(controllerType==ControllerType.TwoFiveD){
            twoFiveD.BindToPath(true);
          }
      }

      void Update(){
        if (Input.GetKeyDown(KeyCode.Tab)){
          if(controllerType==ControllerType.TwoD){
            controllerType=ControllerType.TwoFiveD;
            twoFiveD.BindToPath(true);
          }else{
            controllerType=ControllerType.TwoD;
            twoD.BindToPath(true);
          }
        }
         HandleAnimator();
      }
      void FixedUpdate()
      {
         Move();
      }
      void Setup(){
          if(_path==null){Debug.Log("A path is required!");return;}
          _rigidbody=GetComponent<Rigidbody>();
          _collider=GetComponent<Collider>();
          twoD=new TwoD();twoD.Init(this);
          twoFiveD=new TwoFiveD();twoFiveD.Init(this);
          ChangePath(_path);
          if(animator==null&&GetComponent<Animator>()!=null){
            animator=GetComponent<Animator>();
          }
          if(animator==null&&GetComponentInChildren<Animator>()!=null){
            animator=GetComponentInChildren<Animator>();
          }
          if(animator==null){
            Debug.Log("An Animator is required!");
          }
      }

/*** PATH ***/

      public void Move(){
        yVelocity=_rigidbody.velocity.y;
        yPosition=_rigidbody.position.y;
        zOff=CalculateZOffset();
        if(controllerType==ControllerType.TwoD){
          _rigidbody.velocity = twoD.CalculateDirection()*movementSpeed;
          if(isPushing){
            twoD.BindToPath(true);
          }else{
            twoD.BindToPath();
          }
        }else if(controllerType==ControllerType.TwoFiveD){
          _rigidbody.velocity =  twoFiveD.CalculateDirection()*movementSpeed;
          if(isPushing){
            twoFiveD.BindToPath(true);
          }else{
            twoFiveD.BindToPath();
          }
        }
        _rigidbody.velocity = new Vector3(_rigidbody.velocity.x,yVelocity,_rigidbody.velocity.z);
        transform.position= new Vector3(transform.position.x,yPosition,transform.position.z);
        float w=Vector3.Dot(_rigidbody.velocity,forward*360)*0.0025f;
        if(Mathf.Abs(_rigidbody.velocity.x)<0.1f&&Mathf.Abs(_rigidbody.velocity.z)<0.1f){
            w=0;
        }
        speed=new Vector4(_rigidbody.velocity.x,_rigidbody.velocity.y,_rigidbody.velocity.z,w);
        isGrounded=CheckGrounded();
        //Debug.Log(CheckSlope(45));
      }
      public Vector3 CalculateZOffset(){
          Vector3 oldDir=zOffDir;
          Vector3 newDir=new Vector3(fPoint.x,0,fPoint.z);
          newDir=Quaternion.Euler(0,-90,0)*newDir;
          float d = Vector3.Dot(oldDir,newDir);
          zOffDir=Vector3.Lerp(oldDir, newDir, 0.5f);
          Vector3  zOffPt=zOffDir.normalized*PathOffset;
          return zOffPt;
      }

      public bool IsTurning(){
        bool turning=false;
        if(_rigidbody!=null){
            float angleDiff=Mathf.Abs(_rigidbody.rotation.eulerAngles.y - lastRotation.eulerAngles.y);
            if (angleDiff>1f){
                turning=true;
            }
            lastRotation=_rigidbody.rotation;
        }
        return turning;
      }

      public void FacePath()
      {
          fPoint=PathMath.FwdFromCPoint(_math, transform.position,  out cPoint);
          Vector3 velocity=Vector3.zero;
          if(isFacingRight){
              velocity=fPoint * -0.1f;
          }else{
              velocity=fPoint * 0.1f;
          }
          forward=new Vector3(velocity.x,0,velocity.z).normalized;
          pathForward=forward;
          LookForward(transform,forward*360);
          isFacingRight=true;
      }
      public void LookForward(Transform obj,Vector3 lookTarget, bool anim=false){
          if(anim){
              Vector3 targetDir = lookTarget - obj.position;
              Vector3 forward = obj.forward;
              Vector3 localTarget = transform.InverseTransformPoint(lookTarget);
              float angle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
              Vector3 eulerAngleVelocity = new Vector3 (0, angle, 0);
              Quaternion deltaRotation= Quaternion.Euler(eulerAngleVelocity * (Time.deltaTime*movementSpeed));
              _rigidbody.MoveRotation(_rigidbody.rotation * deltaRotation);
          }else{
              obj.LookAt(lookTarget);
              float yRot = obj.eulerAngles.y;
              obj.eulerAngles=new Vector3(0,yRot,0);
          }
      }
      public void ChangePath(BGCurve newpath){
        _path=newpath;
        _math = new BGCurveBaseMath(newpath, new BGCurveBaseMath.Config(BGCurveBaseMath.Fields.PositionAndTangent));
        PathMath.RecalculatePath(_math,true);
        if(_path.GetComponent<BGCcBounds>()!=null){
            pathBounds=_path.GetComponent<BGCcBounds>();
        }
      }

/*** PHYSICS ***/

      public bool CheckGrounded(float rayDistance=1.0f){
        var grounded=false;
        Debug.DrawRay(transform.position, -Vector3.up*rayDistance, Color.green);
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, rayDistance))
        {
            grounded=true;
        }
        return grounded;
      }
      public bool CheckSlope(float slideLimit=10f, float rayDistance=5f){
        var sliding=false;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, rayDistance))
        {
            sliding |= (Vector3.Angle(hit.normal, Vector3.up) > slideLimit && hit.collider.tag != "NoSlide");
        }
        return sliding;
      }
      void OnCollisionEnter(Collision collision)
      {
        if(collision.gameObject.tag!="Pushable"){return;}
          isPushing=true;
      }
      void OnCollisionExit(Collision collision)
      {
        if(collision.gameObject.tag!="Pushable"){return;}
          isPushing=false;
      }

/*** INPUT ***/

      public float GetHorizontalMovementInput()
      {
          return Input.GetAxis(horizontalInputAxis);
      }
      public float GetVerticalMovementInput()
      {
          return Input.GetAxis(verticalInputAxis);
      }

/*** ANIMATION ***/

      public void HandleAnimator(){
          animator.SetBool(AnimGrounded, isGrounded);
          if(isGrounded){
            if(speed.w==0){
                animator.SetBool(AnimIdle, true);
                animator.SetBool(AnimRunning, false);
            }else{
                animator.SetBool(AnimIdle, false);
                animator.SetBool(AnimRunning, true);
            }
            animator.SetBool(AnimJumping, false);
          }else{
            animator.SetBool(AnimIdle, false);
            animator.SetBool(AnimRunning, false);
            animator.SetBool(AnimJumping, true);
          }
      }
      /*
      public void HandleAnimator(){
          animator.SetBool("Grounded", isGrounded);
          if(isGrounded){
            if(speed.w==0){
              animator.SetBool("Idle", true);
              animator.SetBool("Running", false);
            }else{
              animator.SetBool("Idle", false);
              animator.SetBool("Running", true);
            }
            animator.SetBool("Jumping", false);
          }else{
            animator.SetBool("Idle", false);
            animator.SetBool("Running", false);
            animator.SetBool("Jumping", true);
          }
      }
      */

    }
}
