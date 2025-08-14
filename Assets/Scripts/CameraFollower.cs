using UnityEngine;

namespace CornEngine
{
    public class CameraFollower : MonoBehaviour
    {
        public CharacterController Target;
        public Vector3 Offset = new Vector3(0f,1f,-3f);
        public float SmoothTime = 0.3f;

        private Vector3 velocity = Vector3.zero;

        protected float _currentZoom;
        protected Vector3 _currentVelocity;
        private Quaternion _currentRotVelocity;

        protected float _offsetZ;
        protected Vector3 _lookAheadPos;
        protected Vector3 _lastTargetPosition;

        [HideInInspector] public float _addZ = 0f;

        protected Vector3 _lookDirectionModifier = new Vector3(0,0,0);
        private Quaternion _lastRotation=Quaternion.Euler(Vector3.zero);

        private Camera cam;

        public float CameraSpeed;

            private Vector3 dir;
            private Vector3 last_dir;
            private Vector3 OriginalPos;
            private Quaternion OriginalRot;
            private Vector3 NewPos;
            private Quaternion NewRot;

            public float MinimumZoom = 5f;
            [Range (1, 20)]
            public float MaximumZoom = 10f;
            public float ZoomSpeed = 0.4f;

        private CharacterController TargetChar;

        private void Start()
        {
            _currentZoom=MinimumZoom;
            cam=GetComponent<Camera>();
            TargetChar=Target.GetComponent<CharacterController>();
            Zoom();
        }

        void LateUpdate(){
            if(MinimumZoom<1){
                MinimumZoom=1;
            }
            sideFollow();
         }

        void Zoom()
        {
            if (Target == null){return;}

            float characterSpeed = Mathf.Abs(Target.speed.x);
            float currentVelocity=0f;

            _currentZoom=Mathf.SmoothDamp(_currentZoom,(characterSpeed/10)*(MaximumZoom-MinimumZoom)+MinimumZoom,ref currentVelocity,ZoomSpeed);

            cam.orthographicSize=_currentZoom;
        }

        void sideFollow(){
            if (Target == null){return;}

            Vector3 aheadTargetPos=Vector3.zero;
            Vector3 _offsetZD=new Vector3(Offset.x,Offset.y, 0);
            float _offsetZA=Offset.z+_addZ;

            if(TargetChar.isFacingRight){
                dir=-Target.pathForward;
                if(TargetChar.pathForward!=Vector3.zero){
                    dir=Quaternion.Euler(0, 90, 0) * Target.pathForward;
                }
            }else{
                dir=Target.pathForward;
                if(Target.pathForward!=Vector3.zero){
                    dir=Quaternion.Euler(0, -90, 0) * Target.pathForward;
                }
            }

            if(dir==-last_dir){return;}

            aheadTargetPos = _lastTargetPosition + _lookAheadPos  + dir * + _offsetZ + _lookDirectionModifier;
            transform.position = Vector3.SmoothDamp(transform.position,(aheadTargetPos - dir * + _offsetZA) + _offsetZD, ref _currentVelocity, CameraSpeed);
            transform.LookAt (Target.transform.position  + _offsetZD);
            NewRot = transform.rotation;

            if(DeltaMath.RoundQuat(OriginalRot,100f)!=DeltaMath.RoundQuat(NewRot,100f)){
                transform.rotation = DeltaMath.QuarternationSmoothDamp(OriginalRot, NewRot, ref _currentRotVelocity, CameraSpeed);
           }

            last_dir=dir;
            _lastRotation = Target.transform.rotation;
            _lastTargetPosition = Target.transform.position;

            OriginalPos = transform.position;
            OriginalRot = transform.rotation;
        }

    }
}
