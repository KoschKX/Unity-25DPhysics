using System;
using UnityEngine;
using System.Collections.Generic;
using BansheeGz.BGSpline.Curve;
using System.Linq;

namespace BansheeGz.BGSpline.Components
{
    /// <summary> Sweep a line or 2d spline along another 2d spline.</summary>
    [HelpURL("http://www.bansheegz.com/BGCurve/Cc/BGCcSweep2D")]
    [DisallowMultipleComponent]
    [
        CcDescriptor(
            Description = "Create Bounds.",
            Name = "Bounds",
            Icon = "BGCcSweep2d123")
    ]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [AddComponentMenu("BansheeGz/BGCurve/Components/BGCcSweep2D")]
    [ExecuteInEditMode]
    public class BGCcBounds : BGCcSplitterPolyline
    {
        //===============================================================================================
        //                                                    Enums
        //===============================================================================================
        /// <summary>Profile mode </summary>
        public enum ProfileModeEnum
        {
            /// <summary>Sweep straight line</summary>
            Line = 0,

            /// <summary>Sweep 2d spline</summary>
            Spline = 1,
        }

        //===============================================================================================
        //                                                    Fields (Persistent)
        //===============================================================================================
        [SerializeField] [Tooltip("Profile mode.\r\n " +
                                  "StraightLine -use straight line as cross section;\r\n " +
                                  "Spline - use 2d spline as cross section;")] private ProfileModeEnum profileMode = ProfileModeEnum.Line;




        [SerializeField] [Tooltip("Line width for StraightLine profile mode")] public float pathWidth = 1;
        [SerializeField] [Tooltip("Line width for StraightLine profile mode")] public float pathHeight = 1;



        [SerializeField] [Tooltip("U coordinate for line start")] private float uCoordinateStart;

        [SerializeField] [Tooltip("U coordinate for line end")] private float uCoordinateEnd = 1;

        [SerializeField] [Tooltip("Profile spline for Spline profile mode")] private BGCcSplitterPolyline profileSpline;

//        [SerializeField] [Tooltip("Custom field, providing U coordinate")] private BGCurvePointField uCoordinateField;

        [SerializeField] [Tooltip("V coordinate multiplier")] private float vCoordinateScale = 1;

        [SerializeField] [Tooltip("Swap U with V coordinate")] private bool swapUV;

        [SerializeField] [Tooltip("Swap mesh normals direction")] private bool swapNormals;

        [SerializeField] [Tooltip("Recalculate mesh normals")] private bool recalculateNormals;

        /// <summary>Mode for profile (cross section) </summary>
        public ProfileModeEnum ProfileMode
        {
            get { return profileMode; }
            set { ParamChanged(ref profileMode, value); }
        }

        /// <summary>Line width for StraightLine profile mode</summary>
        public float PathWidth
        {
            get { return pathWidth; }
            set { ParamChanged(ref pathWidth, value); }
        }

        /// <summary>U coordinate for line start</summary>
        public float UCoordinateStart
        {
            get { return uCoordinateStart; }
            set { ParamChanged(ref uCoordinateStart, value); }
        }

        /// <summary>U coordinate for line end</summary>
        public float UCoordinateEnd
        {
            get { return uCoordinateEnd; }
            set { ParamChanged(ref uCoordinateEnd, value); }
        }

        /// <summary>Profile spline for Spline profile mode</summary>
        public BGCcSplitterPolyline ProfileSpline
        {
            get { return profileSpline; }
            set
            {
                ParamChanged(ref profileSpline, value);
//                if () uCoordinateField = null;
            }
        }

        /// <summary>Swap U and V coordinates</summary>
        public bool SwapUv
        {
            get { return swapUV; }
            set { ParamChanged(ref swapUV, value); }
        }

        /// <summary>Swap normal direction 180 degrees</summary>
        public bool SwapNormals
        {
            get { return swapNormals; }
            set { ParamChanged(ref swapNormals, value); }
        }

        public bool RecalculateNormals
        {
            get { return recalculateNormals; }
            set { ParamChanged(ref recalculateNormals, value); }
        }



/*
        /// <summary>Custom field, providing U coordinate</summary>
        public BGCurvePointField UCoordinateField
        {
            get { return uCoordinateField; }
            set { ParamChanged(ref uCoordinateField, value); }
        }
*/

        /// <summary>V coordinate scale</summary>
        public float VCoordinateScale
        {
            get { return vCoordinateScale; }
            set { ParamChanged(ref vCoordinateScale, value); }
        }

        //===============================================================================================
        //                                                    Editor stuff
        //===============================================================================================
        public override string Error
        {
            get
            {
                return ChoseMessage(base.Error, () =>
                {
                    //if (!Curve.Mode2DOn) return "Curve should be in 2D mode";

                    if (profileMode == ProfileModeEnum.Spline)
                    {
                        if (profileSpline == null) return "Profile spline is not set.";
                        if (profileSpline.Curve.Mode2D != BGCurve.Mode2DEnum.XY) return "Profile spline should be in XY 2D mode.";
                        profileSpline.InvalidateData();
                        if (profileSpline.PointsCount < 2) return "Profile spline should have at least 2 points.";
                    }

                    var profilePointsCount = profileMode == ProfileModeEnum.Line ? 2 : profileSpline.PointsCount;
                    if (PointsCount*profilePointsCount > 65534) return "Vertex count per mesh limit is exceeded ( > 65534)";
                    return null;
                });
            }
        }

        //===============================================================================================
        //                                                    Fields (Not persistent)
        //===============================================================================================
        private static readonly List<PositionWithU> crossSectionList = new List<PositionWithU>();

        [NonSerialized] private MeshFilter meshFilter;
        [NonSerialized] private MeshRenderer meshRenderer;
        [NonSerialized] private Mesh[] meshes;
        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<Vector2> uvs = new List<Vector2>();
        private readonly List<int> triangles = new List<int>();

        [NonSerialized] private MeshCollider meshColliderTop;
        [NonSerialized] private MeshCollider meshColliderBottom;
        [NonSerialized] private MeshCollider meshColliderFront;
        [NonSerialized] private MeshCollider meshColliderBack;
        [NonSerialized] private MeshCollider meshColliderLeft;
        [NonSerialized] private MeshCollider meshColliderRight;

        public MeshFilter MeshFilter
        {
            get
            {
                //do not replace with ??
                if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
                return meshFilter;
            }
        }

        public MeshRenderer MeshRenderer
        {
            get
            {
                //do not replace with ??
                if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
                return meshRenderer;
            }
        }

        public MeshCollider MeshColliderTop
        {
            get
            {
                //do not replace with ??
                Component[] meshColliders=GetComponents(typeof(MeshCollider));
                if(meshColliders.Length<1){gameObject.AddComponent<MeshCollider>();}
                if (meshColliderTop == null) meshColliderTop = (MeshCollider)meshColliders[0];
                return meshColliderTop;
            }
        }
        public MeshCollider MeshColliderBottom
        {
            get
            {
                //do not replace with ??
                Component[] meshColliders=GetComponents(typeof(MeshCollider));
                if(meshColliders.Length<2){gameObject.AddComponent<MeshCollider>();}
                if (meshColliderBottom == null) meshColliderBottom = (MeshCollider)meshColliders[1];
                return meshColliderBottom;
            }
        }
        public MeshCollider MeshColliderLeft
        {
            get
            {
                //do not replace with ??
                Component[] meshColliders=GetComponents(typeof(MeshCollider));
                if(meshColliders.Length<3){gameObject.AddComponent<MeshCollider>();}
                if (meshColliderLeft == null) meshColliderLeft = (MeshCollider)meshColliders[2];
                return meshColliderLeft;
            }
        }
        public MeshCollider MeshColliderRight
        {
            get
            {
                //do not replace with ??
                Component[] meshColliders=GetComponents(typeof(MeshCollider));
                if(meshColliders.Length<4){gameObject.AddComponent<MeshCollider>();}
                if (meshColliderRight == null) meshColliderRight = (MeshCollider)meshColliders[3];
                return meshColliderRight;
            }
        }
        public MeshCollider MeshColliderBack
        {
            get
            {
                //do not replace with ??
                Component[] meshColliders=GetComponents(typeof(MeshCollider));
                if(meshColliders.Length<5){gameObject.AddComponent<MeshCollider>();}
                if (meshColliderBack == null) meshColliderBack = (MeshCollider)meshColliders[4];
                return meshColliderBack;
            }
        }
        public MeshCollider MeshColliderFront
        {
            get
            {
                //do not replace with ??
                Component[] meshColliders=GetComponents(typeof(MeshCollider));
                if(meshColliders.Length<6){gameObject.AddComponent<MeshCollider>();}
                if (meshColliderFront == null) meshColliderFront = (MeshCollider)meshColliders[5];
                return meshColliderFront;
            }
        }

        //===============================================================================================
        //                                                    Unity Callbacks
        //===============================================================================================

        public override void Start()
        {
            useLocal = true;
            base.Start();
            if (MeshFilter.sharedMesh == null) UpdateUI();
        }

        //mathChangedEvent

        //===============================================================================================
        //                                                    Public Functions
        //===============================================================================================
        public void UpdateUI(bool _reset=false)
        {

            //Debug.Log(BGEditorUtility.ChangeCheck());
            _reset=true;
            //Debug.Log("Updating Level Bounds...");

            /* COMPONENTS */

                MeshFilter meshFilter = MeshFilter;
                MeshRenderer meshRenderer =  MeshRenderer;

                MeshCollider meshColliderBottom=MeshColliderBottom;
                MeshCollider meshColliderTop=MeshColliderTop;
                MeshCollider meshColliderLeft=MeshColliderLeft;
                MeshCollider meshColliderRight=MeshColliderRight;
                MeshCollider meshColliderBack=MeshColliderBack;
                MeshCollider meshColliderFront=MeshColliderFront;

            /* MESHES */

                Mesh mesh = MeshFilter.sharedMesh;

                Mesh bottomMesh = meshColliderBottom.sharedMesh;
                Mesh topMesh = meshColliderTop.sharedMesh;
                Mesh leftMesh = meshColliderLeft.sharedMesh;
                Mesh rightMesh = meshColliderRight.sharedMesh;
                Mesh backMesh = meshColliderBack.sharedMesh;
                Mesh frontMesh = meshColliderFront.sharedMesh;

                if(_reset){
                    mesh = null;
                    bottomMesh = null;
                    topMesh = null;
                    leftMesh = null;
                    rightMesh = null;
                    backMesh = null;
                    frontMesh = null;
                }

                if (bottomMesh == null){bottomMesh = new Mesh();bottomMesh.name="pathBoundsBottom";}
                if (topMesh == null){topMesh = new Mesh();topMesh.name="pathlBoundsTop";}
                if (leftMesh == null){leftMesh = new Mesh();leftMesh.name="pathBoundsLeft";}
                if (rightMesh == null){rightMesh = new Mesh();rightMesh.name="pathBoundsRight";}
                if (backMesh == null){backMesh = new Mesh();backMesh.name="pathBoundsBack";}
                if (frontMesh == null){frontMesh = new Mesh();frontMesh.name="pathBoundsFront";}

                GeneratePathWall(bottomMesh,0);
                GeneratePathWall(topMesh,1);
                GeneratePathWall(leftMesh,2);
                GeneratePathWall(rightMesh,3);
                GeneratePathWall(backMesh,4);
                GeneratePathWall(frontMesh,5);

                meshes=new Mesh[6];
                meshes[0] = meshColliderTop.sharedMesh =  topMesh;
                meshes[1] = meshColliderBottom.sharedMesh =bottomMesh;
                meshes[2] = meshColliderLeft.sharedMesh = leftMesh;
                meshes[3] = meshColliderRight.sharedMesh = rightMesh;
                meshes[4] = meshColliderFront.sharedMesh = backMesh;
                meshes[5] = meshColliderBack.sharedMesh = frontMesh;

                Matrix4x4 transformMatrix = new Matrix4x4();
                transformMatrix.SetTRS(transform.InverseTransformPoint(transform.position),transform.localRotation,transform.localScale);

                CombineInstance[] combine = new CombineInstance[meshes.Length];
                int m = 0;
                while (m < meshes.Length)
                {
                    combine[m].mesh = meshes[m];
                    combine[m].mesh.triangles = meshes[m].triangles;
                    combine[m].transform = transformMatrix;
                    m++;
                }
                if (mesh == null){mesh = new Mesh();mesh.name="pathBounds";}
                mesh.CombineMeshes(combine,true);
                mesh.triangles=mesh.triangles.Reverse().ToArray();
                meshFilter.mesh = WeldVertices(mesh, 0.01f, recalculateNormals, true);
                meshFilter.mesh = mesh;
        }

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

        public Mesh GeneratePathWall(Mesh _mesh, int _dir)
        {
            Matrix4x4 transformMatrix = new Matrix4x4();

            if(_mesh==null){return null;}

            if (Error != null) return null;

            if (!UseLocal)
            {
                useLocal = true;
                dataValid = false;
            }
            var positions = Positions;
            if (positions.Count < 2) return null;

            //prepare
            crossSectionList.Clear();
            triangles.Clear();
            //uvs.Clear();
            vertices.Clear();

            //------------- cross section points
            if (profileMode == ProfileModeEnum.Line)
            {
                if(_dir==0||_dir==1){
                    if(_dir==0){
                        crossSectionList.Add(new PositionWithU {Position = (Vector3.left*pathWidth*.5f)+new Vector3(0,-pathHeight,0), U = uCoordinateStart});
                        crossSectionList.Add(new PositionWithU {Position = (Vector3.right*pathWidth*.5f)+new Vector3(0,-pathHeight,0), U = uCoordinateEnd});
                    }else if(_dir==1){
                        crossSectionList.Add(new PositionWithU {Position = (Vector3.right*pathWidth*.5f)+new Vector3(0,pathHeight,0), U = uCoordinateEnd});
                        crossSectionList.Add(new PositionWithU {Position = (Vector3.left*pathWidth*.5f)+new Vector3(0,pathHeight,0), U = uCoordinateStart});
                    }
                }else if(_dir==2||_dir==3){
                    if(_dir==2){
                        crossSectionList.Add(new PositionWithU {Position = (Vector3.left*pathWidth*.5f)+new Vector3(0,pathHeight,0), U = uCoordinateEnd});
                        crossSectionList.Add(new PositionWithU {Position = (Vector3.left*pathWidth*.5f)+new Vector3(0,-pathHeight,0), U = uCoordinateStart});
                    }else if(_dir==3){
                        crossSectionList.Add(new PositionWithU {Position = (Vector3.right*pathWidth*.5f)+new Vector3(0,-pathHeight,0), U = uCoordinateStart});
                        crossSectionList.Add(new PositionWithU {Position = (Vector3.right*pathWidth*.5f)+new Vector3(0,pathHeight,0), U = uCoordinateEnd});
                    }
                }else if(_dir==4||_dir==5){
                    crossSectionList.Add(new PositionWithU {Position = (Vector3.left*pathWidth*.5f)+new Vector3(0,-pathHeight,0), U = uCoordinateStart});
                    crossSectionList.Add(new PositionWithU {Position = (Vector3.right*pathWidth*.5f)+new Vector3(0,-pathHeight,0), U = uCoordinateEnd});
                }
            }
            else
            {
                var points = profileSpline.Positions;
                for (var i = 0; i < points.Count; i++){
                    crossSectionList.Add(
                        new PositionWithU {Position = points[i]}
                    );
                }
            }
            var crossSectionCount = crossSectionList.Count;
            var crossSectionDistance = .0f;

            for (var i = 0; i < crossSectionCount - 1; i++) crossSectionDistance += Vector3.Distance(crossSectionList[i].Position, crossSectionList[i + 1].Position);

            //------------- calculate U coord for profile spline
            if (profileMode == ProfileModeEnum.Spline)
            {
                var distance = 0f;
                for (var i = 0; i < crossSectionCount - 1; i++)
                {
                    crossSectionList[i] = new PositionWithU {Position = crossSectionList[i].Position, U = uCoordinateStart + (distance/crossSectionDistance)*(uCoordinateEnd - uCoordinateStart)};
                    distance += Vector3.Distance(crossSectionList[i].Position, crossSectionList[i + 1].Position);
                }
                crossSectionList[crossSectionList.Count - 1] = new PositionWithU {Position = crossSectionList[crossSectionList.Count - 1].Position, U = uCoordinateEnd};
            }

            //------------- normal
            Vector3 normal;
            switch (Curve.Mode2D)
            {
                case BGCurve.Mode2DEnum.XY:
                    normal = swapNormals ? Vector3.back : Vector3.forward;
                    break;
                case BGCurve.Mode2DEnum.XZ:
                    normal = swapNormals ? Vector3.down : Vector3.up;
                    break;
                case BGCurve.Mode2DEnum.YZ:
                    normal = swapNormals ? Vector3.left : Vector3.right;
                    break;
                default:
                    normal = swapNormals ? Vector3.down : Vector3.up;
                    break;
                    //throw new ArgumentOutOfRangeException("Curve.Mode2D");
            }

                //------------- build mesh
                //first row
                var closed = Curve.Closed;
                Vector3 firstTangent;
                if (closed)
                {
                    var first = positions[1] - positions[0];
                    var firstDistance = first.magnitude;
                    var last = positions[positions.Count - 1] - positions[positions.Count - 2];
                    var lastDistance = last.magnitude;

                    var distanceRatio = firstDistance/lastDistance;
                    firstTangent = first.normalized + last.normalized*distanceRatio;
                }
                else firstTangent = positions[1] - positions[0];

                var previousForward = firstTangent;
                var previousForwardNormalized = previousForward.normalized;
                var previousForwardDistance = (positions[1] - positions[0]).magnitude;
                var matrix = Matrix4x4.TRS(positions[0], Quaternion.LookRotation(previousForward, normal), Vector3.one);
                for (var i = 0; i < crossSectionCount; i++)
                {
                    var positionWithU = crossSectionList[i];
                    vertices.Add(matrix.MultiplyPoint(positionWithU.Position));
                    //uvs.Add(swapUV ? new Vector2(0, positionWithU.U) : new Vector2(positionWithU.U, 0));
                }

            if(!closed&&(_dir==4||_dir==5)){

                var currentDistance = previousForwardDistance;
                var positionsCount = positions.Count;

                var position=0;
                if(_dir==4){
                    position = positions.Count - 1;

                    previousForward = positions[position] - positions[position-1];
                    previousForwardNormalized = previousForward.normalized;
                    previousForwardDistance = previousForward.magnitude;

                }else if(_dir==5){
                    position = 0;
                }

                var pos = positions[position];

                var lastPoint = position == positionsCount - 1;

                var forward = lastPoint ? previousForward : positions[position + 1] - pos;
                var forwardNormalized = forward.normalized;
                var forwardDistance = forward.magnitude;

                var distanceRatio = forwardDistance/previousForwardDistance;
                var tangent = forwardNormalized + previousForwardNormalized*distanceRatio;

                matrix = Matrix4x4.TRS(pos, Quaternion.LookRotation(tangent, normal), Vector3.one);
                var v = currentDistance/crossSectionDistance*vCoordinateScale;

                //vertices + uvs
                for (var j = 0; j < crossSectionCount; j++)
                {
                    var positionWithU = crossSectionList[j];

                    if(_dir==5){
                        vertices.Add(matrix.MultiplyPoint(positionWithU.Position));
                        vertices.Add(matrix.MultiplyPoint(positionWithU.Position+new Vector3(0,pathHeight*2,0)));
                    }else if(_dir==4){
                        vertices.Add(matrix.MultiplyPoint(positionWithU.Position+new Vector3(0,pathHeight*2,0)));
                        vertices.Add(matrix.MultiplyPoint(positionWithU.Position));
                    }
                    //uvs.Add(swapUV ? new Vector2(v, positionWithU.U) : new Vector2(positionWithU.U, v));
                    //uvs.Add(swapUV ? new Vector2(v, positionWithU.U) : new Vector2(positionWithU.U, v));
                }

                //tris
                var firstRowStart = vertices.Count - crossSectionCount*2;
                var secondRowStart = vertices.Count - crossSectionCount;
                for (var j = 0; j < crossSectionCount - 1; j++)
                {
                    triangles.Add(firstRowStart + j);
                    triangles.Add(secondRowStart + j);
                    triangles.Add(firstRowStart + j + 1);

                    triangles.Add(firstRowStart + j + 1);
                    triangles.Add(secondRowStart + j);
                    triangles.Add(secondRowStart + j + 1);
                }

                //update vars
                currentDistance += forwardDistance;


            }else{
                //iterate points
                var currentDistance = previousForwardDistance;
                var positionsCount = positions.Count;
                for (var i = 1; i < positionsCount; i++)
                {
                    var pos = positions[i];

                    var lastPoint = i == positionsCount - 1;

                    var forward = lastPoint ? previousForward : positions[i + 1] - pos;
                    var forwardNormalized = forward.normalized;
                    var forwardDistance = forward.magnitude;

                    var distanceRatio = forwardDistance/previousForwardDistance;
                    var tangent = forwardNormalized + previousForwardNormalized*distanceRatio;

                    //hack for closed curve
                    if (lastPoint && closed) tangent = firstTangent;

                    matrix = Matrix4x4.TRS(pos, Quaternion.LookRotation(tangent, normal), Vector3.one);
                    var v = currentDistance/crossSectionDistance*vCoordinateScale;

                    //vertices + uvs
                    for (var j = 0; j < crossSectionCount; j++)
                    {
                        var positionWithU = crossSectionList[j];
                        vertices.Add(matrix.MultiplyPoint(positionWithU.Position));
                        //uvs.Add(swapUV ? new Vector2(v, positionWithU.U) : new Vector2(positionWithU.U, v));
                    }

                    //tris
                    var firstRowStart = vertices.Count - crossSectionCount*2;
                    var secondRowStart = vertices.Count - crossSectionCount;
                    for (var j = 0; j < crossSectionCount - 1; j++)
                    {
                        triangles.Add(firstRowStart + j);
                        triangles.Add(secondRowStart + j);
                        triangles.Add(firstRowStart + j + 1);

                        triangles.Add(firstRowStart + j + 1);
                        triangles.Add(secondRowStart + j);
                        triangles.Add(secondRowStart + j + 1);
                    }

                    //update vars
                    currentDistance += forwardDistance;
                    previousForward = forward;
                    previousForwardNormalized = forwardNormalized;
                    previousForwardDistance = forwardDistance;
                }
            }
                       //set values
            _mesh.Clear();
            _mesh.SetVertices(vertices);
            //_mesh.SetUVs(0, uvs);
            _mesh.SetTriangles(triangles, 0);
            _mesh.RecalculateNormals();

            return _mesh;
        }

        public static Mesh FlipMesh(Mesh mesh)
        {
            Mesh m = mesh;
            if (m)
            {
                int[] tris = m.triangles;
                for (int i = 0; i< tris.Length-1; i+= 3)
                {
                    int t = tris[i];
                    tris[i] = tris[i+1];
                    tris[i+1] = t;
                }
             m.triangles = tris;
            }
         return m;
        }

         public static Mesh WeldVertices(Mesh aMesh, float aMaxDelta = 0.01f, bool recalcNormals = false, bool optimize = true) {
             var verts = aMesh.vertices;
             var normals = aMesh.normals;
             var uvs = aMesh.uv;
             Dictionary<Vector3, int> duplicateHashTable = new Dictionary<Vector3, int>();
             List<int> newVerts = new List<int>();
             int[] map = new int[verts.Length];

             //create mapping and find duplicates, dictionaries are like hashtables, mean fast
             for (int i = 0; i < verts.Length; i++) {
                 if (!duplicateHashTable.ContainsKey(verts[i])) {
                     duplicateHashTable.Add(verts[i], newVerts.Count);
                     map[i] = newVerts.Count;
                     newVerts.Add(i);
                 }
                 else {
                     map[i] = duplicateHashTable[verts[i]];
                 }
             }

             // create new vertices
             var verts2 = new Vector3[newVerts.Count];
             var normals2 = new Vector3[newVerts.Count];
             //var uvs2 = new Vector2[newVerts.Count];
             for (int i = 0; i < newVerts.Count; i++) {
                 int a = newVerts[i];
                 verts2[i] = verts[a];
                 normals2[i] = normals[a];
                 //uvs2[i] = uvs[a];
             }
             // map the triangle to the new vertices
             var tris = aMesh.triangles;
             for (int i = 0; i < tris.Length; i++) {
                 tris[i] = map[tris[i]];
             }
             aMesh.triangles = tris;
             aMesh.vertices = verts2;
             aMesh.normals = normals2;
             //aMesh.uv = uvs2;

             aMesh.RecalculateBounds();
             //aMesh.RecalculateTangents();

             if(recalcNormals){
                aMesh.RecalculateNormals();
             }

             if(optimize){
                aMesh.Optimize();
                aMesh.OptimizeIndexBuffers();
                aMesh.OptimizeReorderVertexBuffer();
             }

             return aMesh;
         }

        //===============================================================================================
        //                                                    Private Functions
        //===============================================================================================
        // curve's changed
        protected override void UpdateRequested(object sender, EventArgs e)
        {
            base.UpdateRequested(sender, e);
            UpdateUI();
        }


        //===============================================================================================
        //                                                    Private classes
        //===============================================================================================
        private struct PositionWithU
        {
            public Vector3 Position;
            public float U;
        }
    }
}
