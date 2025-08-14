using UnityEngine;
using System.Collections.Generic;
using BansheeGz.BGSpline.Curve;

using UnityEngine.UI;

namespace CornEngine
{

    public class KeepOnPath : MonoBehaviour {
        private Vector3 cPoint=Vector3.zero;
        private Vector3 fPoint=Vector3.zero;
        private Vector3 vDir=Vector3.zero;

        public enum ControllerType
        {
            TwoD,
            TwoFiveD,
            ThreeD
        }

        [Header("Components")]

          public BGCurve curve;

          private BGCurveBaseMath _math;

          [HideInInspector] public Collider  _collider;
          [HideInInspector] public Rigidbody _rigidbody;
          [HideInInspector] public List<Collider>  _extraColliders;
          private LODGroup _lodgroup;
          [HideInInspector] public Mesh _mesh;
          private Bounds _bounds;
          private RigidbodyConstraints _rigidbodyConstraints;
          private CollisionDetectionMode collisionDetectionMode;

        [Header("Type")]

          public ControllerType controllerType;
          private ControllerType lastControllerType;

        [Header("Abilities")]

          public bool isPushable = false;
          public bool isGrabbable = false;
          public bool isSnappable = false;
          public bool isLockable = true;

          [HideInInspector] public bool isOffPath=false;
          [HideInInspector] public bool isPushedOffPath = false;

        [Header("Options")]

          [HideInInspector] public Vector3 pushDirection = Vector3.zero;

          public bool isBound=true;
          private bool wasBound=true;
          public bool turnWithPath=false;

        [Header("Properties")]

          private Quaternion initialRotation;
          private Vector3 modelRotation = Vector3.zero;
          public Vector3 CenterOfMass;

          [HideInInspector] public bool isKinematic = false;
          [HideInInspector] public bool isTrigger = false;
          [HideInInspector] public bool useGravity = false;
          [HideInInspector] public bool detectCollisions = false;
          [HideInInspector] public bool keepOnPath = false;

          [HideInInspector] public float mass;
          [HideInInspector] public float drag;
          [HideInInspector] public float angular_drag;

          [HideInInspector] public int layer;
          [HideInInspector] public Transform parent;
          [HideInInspector] public PhysicMaterial physicMaterial;
          [HideInInspector] public List<PhysicMaterial>  _extraPhysicMaterials;
          [HideInInspector] public RigidbodyInterpolation interpolation;
          [HideInInspector] public RigidbodyConstraints constraints;

        [Header("State")]

          [ReadOnly] public bool isCarried = false;
          [ReadOnly] public bool isStacked = false;
          [ReadOnly] public bool isGrounded = false;
          [ReadOnly] public bool isLocked = false;
          [ReadOnly] public bool isPushed = false;
          [ReadOnly] public bool isAwake;
          [ReadOnly] public bool isInWater = false;
          [ReadOnly] public bool isFacing = false;
          [ReadOnly] public bool isBlocking = false;

          [ReadOnly] public bool isStatic=false;
          [ReadOnly] public bool isUnderStatic=false;
          [ReadOnly] public bool isPathStatic=false;

          [HideInInspector] public bool isEnteringWater = false;

          public Vector3 _constantforce;
          public Vector3 movementSpeed;

          public float PathOffset = 0f;
          private Vector3 zOffDir = Vector3.zero;
          private Vector3 zOff = Vector3.zero;
          public Vector3 forward = Vector3.zero;
          private Vector3 oldPosition=Vector3.zero;

          private bool _kinematic;

          [HideInInspector] public Animator _animator;
          [HideInInspector] public Vector4 speed;

      	void Start () {
             Setup();

        // INITIAL BINDING
            if(isBound){
              Move();
              LookForward(vDir,true);
              if(controllerType==ControllerType.TwoD){BindToPath2D();}
              else if(controllerType==ControllerType.TwoFiveD){BindToPath25D();}
            }

            SetValues();
      	}

        void Setup(){
              initialRotation = transform.localRotation;
              modelRotation = transform.rotation.eulerAngles;
              if(curve!=null){
                  _math = new BGCurveBaseMath(curve, new BGCurveBaseMath.Config(BGCurveBaseMath.Fields.PositionAndTangent));
              }else{
                  CornTools.PrintMessage("A path is required!");
              }
              if(GetComponent<Collider>()!=null){
                  _collider=GetComponent<Collider>();
                  List<Collider> _allColliders=new List<Collider>();
                  _extraColliders=new List<Collider>();
                  _allColliders.AddRange(GetComponents<Collider>());
                  bool hasCarry=false;
                  foreach(Collider col in _allColliders){
                      if(col.sharedMaterial!=null&&col.sharedMaterial.name=="Carry"){_collider=col; hasCarry=true;}
                      if(!hasCarry&&!col.isTrigger){ _collider=col; hasCarry=true;}
                      if(!col.isTrigger){_extraColliders.Add(col);}
                  }
                  if(_extraColliders.Count>1){  _extraColliders.Remove(_collider); _extraPhysicMaterials=new List<PhysicMaterial>();}
                  else{_extraColliders.Clear(); _extraColliders=null;}
              }else{
                  CornTools.PrintMessage("A collider is required!");
              }
              if(GetComponent<Rigidbody>()!=null){
                  _rigidbody=GetComponent<Rigidbody>();
                  _rigidbodyConstraints=_rigidbody.constraints;
              }else{
                  CornTools.PrintMessage("A rigid body is required!");
              };
              if(GetComponent<LODGroup>()!=null){
                  _lodgroup=GetComponent<LODGroup>();
                 _mesh = CornTools.GetCurrentLODMesh(_lodgroup);
              }else if(GetComponent<MeshFilter>()!=null){
                 _mesh = GetComponent<MeshFilter>().mesh;
              }
              if(GetComponent<Animator>()!=null){
                  _animator=GetComponent<Animator>();
              }
        }

        void FixedUpdate()
        {
            isFacing=CornTools.CheckFacingPath(_rigidbody,vDir);
            isStatic=CornTools.CheckStatic(_rigidbody,0.01f);
            isPathStatic=CornTools.CheckPathStatic(_rigidbody,oldPosition,0.01f);

            if(!isLocked&&!isStacked){
              isBlocking=false;
            }
            isGrounded=false;
            if(_rigidbody==null){return;}
            if(isOffPath){
                if(isStatic){
                  isGrounded=CheckGrounded();
                  if(isGrounded){
                      ResetPathOffset();
                  }
                }
                return;
            }
            _kinematic=_rigidbody.isKinematic;
            controllerType=lastControllerType;

        // LOCK
            if(isPushable&&isLocked&&!isStacked){
              if(isPushed&&!isStacked){
                 CornTools.Unlock(_rigidbody, out isLocked, _rigidbodyConstraints);
              }else if(isLockable&&!isLocked){
                 CornTools.Lock(_rigidbody, out isLocked);
              }
            }
            if(isLocked){CornTools.ClearVelocity(_rigidbody);}
            if(isPathStatic){CornTools.ClearPathVelocity(_rigidbody);}

        // STACK
            if(isStatic||isStacked){
                isGrounded=CheckGrounded();
                if(!isCarried&&isGrounded){
                    if(_rigidbody.isKinematic){
                        isUnderStatic=CheckStacked();
                    }
                    if(!isPushed&&(isLockable||isStacked)&&!isLocked){
                      CornTools.Lock(_rigidbody, out isLocked);
                    }
                }
                if(!isGrounded){
                    if(isLocked){
                      CornTools.Unlock(_rigidbody, out isLocked, _rigidbodyConstraints);
                    }
                    isUnderStatic=false;
                }
                return;
            }

        // WAKE
            _rigidbody.centerOfMass=CenterOfMass;
            _rigidbody.WakeUp();
             isAwake = !_rigidbody.IsSleeping();

        // MOVE
            if(isLocked||isPathStatic){return;}
            Move();

        // BIND
            if(controllerType==ControllerType.TwoFiveD&&!isFacing){
              ResetPathOffset(true);
              return;
            }
            if(!isPushedOffPath){
              if(controllerType==ControllerType.TwoD){
                  BindToPath2D();
              }else if(controllerType==ControllerType.TwoFiveD){
                if(isFacing){
                  BindToPath25D();
                }
              }
            }else{
              ResetPathOffset(true);
            }

        // TURN
            if(isFacing&&turnWithPath){
              LookForward(vDir);
            }
            oldPosition=_rigidbody.position;
        }

        public void SetValues(){
            wasBound=isBound;
            if(_rigidbody!=null){
                isKinematic=_rigidbody.isKinematic;
                useGravity=_rigidbody.useGravity;
                interpolation=_rigidbody.interpolation;
                collisionDetectionMode=_rigidbody.collisionDetectionMode;
                constraints=_rigidbody.constraints;
                mass=_rigidbody.mass;
                drag=_rigidbody.drag;
                angular_drag=_rigidbody.angularDrag;
            }
            if(_collider!=null){
                physicMaterial=_collider.sharedMaterial;
            }
            if(_extraColliders!=null){
                _extraPhysicMaterials.Clear();
                for(int ec=0;ec<_extraColliders.Count;ec++){
                    _extraPhysicMaterials.Add(_extraColliders[ec].sharedMaterial);
                }
            }
            layer=gameObject.layer;
            parent=transform.parent;
        }

/*** PATH ***/

        void Move(){
            if(_math==null||_rigidbody==null||_collider==null){return;}
            if(isStatic||isPathStatic){return;}

          // CALCULATE OFF PATH OFFSET
            if(isPushedOffPath){return;}
            zOff=CalculateZOffset();
            fPoint=PathMath.FwdFromCPoint(_math, PathMath.GetFlatPos(_rigidbody.position,cPoint.y),  out cPoint, zOff);
            vDir=PathMath.GetFlatPos(fPoint,0).normalized;

          // REDIRECT VELOCITY
            if(!isPushedOffPath){
                Vector3 vProject = Vector3.Project(_rigidbody.velocity, -vDir);
                _rigidbody.velocity = PathMath.GetFlatPos(vProject,_rigidbody.velocity.y);
            }
            forward=fPoint;
        }

        public void ResetPathOffset(bool skipCalc=false){
            isOffPath=false;
            PathOffset=CalculatePathOffset();
            zOff=CalculateZOffset();
            if(!skipCalc){
              fPoint=PathMath.FwdFromCPoint(_math, PathMath.GetFlatPos(_rigidbody.position,cPoint.y),  out cPoint, zOff);
              _rigidbody.velocity=Vector3.zero;
              _rigidbody.angularVelocity=Vector3.zero;
            }
        }

        float CalculatePathOffset(bool skipCalc=false){
            if(!skipCalc){
              PathMath.FwdFromCPoint(_math, PathMath.GetFlatPos(_rigidbody.position,cPoint.y),  out cPoint);
            }
            Vector3 rPos=PathMath.GetFlatPos(_rigidbody.position,cPoint.y);
            Vector3 cPos=PathMath.GetFlatPos(cPoint,cPoint.y);
            Vector3 rDir = (rPos - cPos).normalized;
            float angleCheck=CornTools.GetDirAngle(forward,rDir,Vector3.up);
            if(angleCheck>0f){
                return -(Vector3.Distance(cPoint,rPos));
            }else{
                return (Vector3.Distance(cPoint,rPos));
            }
        }

        public Vector3 CalculateZOffset(){
            Vector3 oldDir=zOffDir;
            Vector3 newDir=Quaternion.Euler(0,-90,0)*fPoint;
            float d = Vector3.Dot(oldDir,newDir);
            zOffDir = Vector3.Lerp(oldDir, newDir, 0.5f);
            Vector3  zOffPt=zOffDir.normalized*PathOffset;
            return zOffPt;
        }

        void OffPathMove(){
            Vector3 vProject = Vector3.Project(_rigidbody.velocity, -pushDirection);
            _rigidbody.velocity = PathMath.GetFlatPos(vProject,_rigidbody.velocity.y);
        }

        void LookForward(Vector3 dir,bool force=false){
            Vector3 checkDirA=Quaternion.Euler(0,90,0)*-dir;
            Vector3 vel=_rigidbody.velocity;
            if(isFacing&&vel.magnitude>0.1f||force){
              Quaternion clRot = CornTools.RotateToClosestSide(transform,checkDirA);
              Quaternion targetRotation = clRot;
              if(force){
                _rigidbody.rotation = clRot;
              }else{
                _rigidbody.rotation = Quaternion.RotateTowards(_rigidbody.rotation, targetRotation, 1f);
              }
            }
        }

        void BindToPath2D(){
            if(_rigidbody==null||_collider==null){return;}
            Vector3 lPoint = PathMath.GetFlatPos(cPoint,_rigidbody.position.y);
            //_rigidbody.MovePosition(lPoint);
            if(isPushed&&!isPushedOffPath){
              _rigidbody.MovePosition(Vector3.Lerp(_rigidbody.position,lPoint,Time.fixedDeltaTime*3f));
            }else{
              _rigidbody.MovePosition(lPoint);
            }
        }

        void BindToPath25D(){
            if(_rigidbody==null||_collider==null){return;}
            Vector3 lPoint=new Vector3(cPoint.x,transform.position.y,cPoint.z);
        }


/*** STACKING ***/

        public bool CheckStacked(bool test=false){
            if(isGrabbable&&isCarried){return false;}
            if(!test&&isStatic&&isUnderStatic){return true;}
            Vector3 _first_origin=transform.position;
            RaycastHit _hit;
            Color _raycolor = Color.red;
            bool isHit = false;
            GameObject _hitObj = null;
            Collider _hitCol = null;
            Rigidbody _hitRig = null;
            Vector3 _origin = Vector3.zero;
            Vector3 _dir= Vector3.up;
            float _len=0f;
            Vector3 globalmax= transform.TransformPoint(_bounds.max);
            float distance = (globalmax - _origin).magnitude;
            _origin = transform.position;
            if(_mesh!=null){
                _len = _mesh.bounds.extents.y+0.01f;
            }
            if(Physics.Raycast(_origin, _dir, out _hit, _len, -1))
            {
                if(_hit.transform.gameObject!=gameObject){
                    if(!test){
                        _hitObj = _hit.transform.gameObject;
                        _hitRig = _hit.rigidbody;
                        _hitCol = _hit.collider;
                        if(_hitRig.isKinematic){
                            isHit=true;
                        }
                    }
                }
            }
            return isHit;
        }

/*** PHYSICS ***/

        public void SetConstraints(RigidbodyConstraints constraints){
            _rigidbodyConstraints=constraints;
        }

        private Vector3 CalculateMovementVelocity()
        {
            if(_rigidbody==null){return Vector3.zero;}
            movementSpeed=new Vector3(0, _constantforce.y*Time.fixedDeltaTime,0);
            return movementSpeed;
        }

        public bool CheckGrounded(bool test=false){
            if(isGrabbable&&isCarried){return false;}
            if(!test&&isStatic&&isGrounded){return true;}
            Vector3 _first_origin=transform.position;
            RaycastHit _hit;
            Color _raycolor = Color.red;
            bool isHit = false;
            GameObject _hitObj = null;
            Collider _hitCol = null;
            Rigidbody _hitRig = null;
            Vector3 _origin = Vector3.zero;
            Vector3 _dir= -Vector3.up;
            float _len=0f;
            Vector3 globalmax= transform.TransformPoint(_bounds.max);
            float distance = (globalmax - _origin).magnitude;
            _origin=transform.position;
            if(_mesh!=null){
                _len=_mesh.bounds.extents.y+0.01f;
            }
            if(Physics.Raycast(_origin, _dir, out _hit, _len, -1))
            {
                if(_hit.transform.gameObject!=gameObject){
                    if(!test){
                        _hitObj=_hit.transform.gameObject;
                        _hitRig=_hit.rigidbody;
                        _hitCol=_hit.collider;
                        if((_hitRig!=null&&_hitRig.isKinematic)||(_hitObj.tag=="Floor"||_hitObj.tag=="Stack")){
                            isHit=true;
                        }
                    }
                }
            }
            return isHit;
        }

        public void UpdatePhysicsValues(){
            if(_rigidbody==null){return;}
            mass=_rigidbody.mass;
            drag=_rigidbody.drag;
            angular_drag=_rigidbody.angularDrag;
        }

        public void ClearPhysics(bool no_y=false){
            if(_rigidbody==null){return;}
            if(no_y){
                _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
            }else{
                _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, _rigidbody.velocity.y, _rigidbody.velocity.z);
            }
            _rigidbody.angularVelocity = Vector3.zero;
        }

        public void IgnorePhysics(Collider collider, bool ignore = true){
            if(_collider!=null){
                Physics.IgnoreCollision(collider, _collider, ignore);
                Physics.IgnoreCollision(_collider, collider, ignore);
            }
            if(_extraColliders!=null){
                foreach(Collider _extraCollider in _extraColliders){
                    Physics.IgnoreCollision(collider, _extraCollider,  ignore);
                    Physics.IgnoreCollision(_extraCollider, collider,  ignore);
                    if(_collider!=null){
                        Physics.IgnoreCollision(_extraCollider, _collider, ignore);
                    }
                }
            }
        }

        void OnCollisionEnter(Collision collision)
        {
          if(_rigidbody==null){return;}
          if(collision.gameObject.tag=="Floor"||collision.gameObject.tag=="Obstacle"){return;}

          float thresh = 33f;

            isPushed=false;

          // CHECK IF VELOCITY IS ALIGNED WITH PATH
            bool velCheck=CornTools.CheckFacingAngle(PathMath.GetFlatPos(_rigidbody.velocity.normalized),vDir.normalized,thresh);
            bool velCheckReverse=CornTools.CheckFacingAngle(PathMath.GetFlatPos(_rigidbody.velocity.normalized),-vDir.normalized,thresh);
            if(velCheck||velCheckReverse){isPushed=true;return;}

          // CHECK IF PUSHED OFF PATH
            foreach (ContactPoint contact in collision.contacts)
            {
              bool angleCheck=CornTools.CheckFacingAngle(contact.normal,vDir.normalized,thresh);
              bool angleCheckReverse=CornTools.CheckFacingAngle(contact.normal,-vDir.normalized,thresh);
              if(angleCheck||angleCheckReverse){
                Debug.DrawRay(contact.point, contact.normal*10f, Color.green);
                isPushedOffPath=false;
              }else{
                Debug.DrawRay(contact.point, contact.normal*10f, Color.red);
                isPushedOffPath=true;
                break;
              }
            }
        }

        void OnCollisionExit(Collision collision)
        {
            if(_rigidbody==null){return;}
            if(collision.gameObject.tag=="Floor"||collision.gameObject.tag=="Obstacle"){return;}

            // RESET PUSH
              isPushed=false;
              isPushedOffPath=false;
        }
    }
}
