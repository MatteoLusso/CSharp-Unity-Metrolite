using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineGenerator : MonoBehaviour
{

    public GameObject prefab_StartLine;

    public bool debug_ContinuouslyRegenerateLine = false;
    //public LineRenderer reference_Line;
    public bool segment_LimitAngle;
    public float segment_AngleValue;
    public bool segment_Constant;
    public float segment_ContantSize = 5.0f;
    public LineRenderer curve_Quadratic;
    public int curve_BezierPointsNumber = 50;
    public List<Transform> curve_ControlPoints;
    public bool mesh_ParabolicCurves = false;

    public float tunnel_width;
    private GameObject tunnel_Pavement;

    public Material texture;

    public List<Vector3> floor_LeftPoints;
    public List<Vector3> floor_RightPoints;
    public Vector3[] curve_Points;
    public Vector3[] mesh_Vertices;
    public int[] mesh_Edges;

    private Vector2[] mesh_UVs;
    private Mesh mesh_Pavement;
    public GameObject tunnel_End;

    public Vector3[] GetCurvePoints()
    {
        return curve_Points;
    }

    void Start()
    {
        if( curve_Quadratic == null )
        {
            curve_Quadratic = new LineRenderer();
        }
        tunnel_Pavement = new GameObject( "Tunnel Base" );

        GameObject tunnel_End = Instantiate( new GameObject( "Tunnel End" ), tunnel_Pavement.transform );
        tunnel_Pavement.AddComponent<MeshFilter>();
        tunnel_Pavement.AddComponent<MeshRenderer>();

        if( !debug_ContinuouslyRegenerateLine ) {
            GenerateLine();
        }
    }

    void Update()
    {
        if( debug_ContinuouslyRegenerateLine )
        {
            GenerateLine();
        }
        
    }

    private void GenerateLine() {
        if( curve_BezierPointsNumber < 2 )
        {
            curve_BezierPointsNumber = 2;
        }
        if( segment_ContantSize < 0.1f )
        {
            segment_ContantSize = 0.1f;
        }

        DrawBezierCurve();

        ConstantSegmentsCurve();
        LimitCurveAngle();

        CalculateLinearMeshVertex();
        GenerateLinearMesh( VertexMatrix_2xM( floor_RightPoints, floor_LeftPoints ) );
        
        tunnel_Pavement.GetComponent<MeshFilter>().sharedMesh = mesh_Pavement;
        tunnel_Pavement.GetComponent<MeshRenderer>().material = texture;
    }

    private void LimitCurveAngle()
    {
        if( segment_LimitAngle )
        {
            List<Vector3> newCurve = new List<Vector3>();
            newCurve.Add(curve_Points[ 0 ]);
            newCurve.Add(curve_Points[ 1 ]);

            int i = 1;

            float L1, L2;

            float alpha = segment_AngleValue;
            float beta;
            float gamma;

            Vector3 prevDir;
            Vector3 nextDir;

            while( i < curve_Points.Length - 1 )
            {
 
                prevDir = newCurve[ i ] - newCurve[ i - 1 ];
                    
                gamma = Vector3.SignedAngle( Vector3.right, prevDir, Vector3.forward );

                L1 = Vector3.SignedAngle( Vector3.right, Quaternion.Euler( 0.0f, 0.0f, alpha ) * prevDir, Vector3.forward );
                L2 = Vector3.SignedAngle( Vector3.right, Quaternion.Euler( 0.0f, 0.0f, -alpha ) * prevDir, Vector3.forward );

                //L1 = gamma + alpha;
                //L2 = gamma - alpha;

                nextDir = curve_Points[ i + 1 ] - newCurve[ i ];

                beta = Vector3.SignedAngle( Vector3.right, nextDir, Vector3.forward );

                Debug.DrawRay( newCurve[ i ], Quaternion.Euler( 0.0f, 0.0f, 0.0f ) * nextDir.normalized * 2, Color.magenta, 0.1f );

                Debug.Log( "Punto " + i + " | gamma = " + gamma + " | L1 = " + L1 + " | L2 = " + L2 + " | beta = " + beta );

                if( beta <= L1 && beta >= L2 )
                {
                    newCurve.Add( newCurve[i] + nextDir.normalized * ( curve_Points[ i + 1 ] - curve_Points[ i ] ).magnitude );
                    Debug.Log( "Nessuna modifica, L2 = " + L2 + " <= beta = " + beta + " <= L1 = " + L1 );

                    Debug.DrawRay( newCurve[ i ], Quaternion.Euler( 0.0f, 0.0f, alpha ) * prevDir.normalized * 2, Color.green, 0.1f );
                    Debug.DrawRay(newCurve[ i ], Quaternion.Euler( 0.0f, 0.0f, -alpha ) * prevDir.normalized * 2, Color.green, 0.1f );
                }
                else if(beta > L1)
                {
                    newCurve.Add( newCurve[ i ] + ( Quaternion.Euler( 0.0f, 0.0f, -( beta - L1 ) ) * nextDir.normalized * ( curve_Points[ i + 1 ] - curve_Points[ i ] ).magnitude ) );
                    Debug.Log( "beta = " + beta + " > L1 = " + L1 );

                    Debug.Log( "Nuovo angolo = " + Vector3.SignedAngle( Vector3.right, ( Quaternion.Euler( 0.0f, 0.0f, -( beta - L1 ) ) * nextDir ), Vector3.forward ) + " = " + L1 + " = L1" );

                    Debug.DrawRay( newCurve[ i ], Quaternion.Euler( 0.0f, 0.0f, alpha ) * prevDir.normalized * 2, Color.red, 0.1f );
                    Debug.DrawRay( newCurve[ i ], Quaternion.Euler( 0.0f, 0.0f, -alpha ) * prevDir.normalized * 2, Color.green, 0.1f );
                }
                else if( beta < L2 )
                {
                    newCurve.Add( newCurve[ i ] + ( Quaternion.Euler( 0.0f, 0.0f, -( beta - L2 ) ) * nextDir.normalized * ( curve_Points[ i + 1 ] - curve_Points[ i ] ).magnitude ) );
                    Debug.Log( "beta = " + beta + " < L2 = " + L2 );

                    Debug.Log( "Nuovo angolo = " + Vector3.SignedAngle( Vector3.right, ( Quaternion.Euler( 0.0f, 0.0f, -( beta - L2 ) ) * nextDir ), Vector3.forward ) + " = " + L2 + " = L2" );

                    Debug.DrawRay( newCurve[ i ], Quaternion.Euler( 0.0f, 0.0f, alpha ) * prevDir.normalized * 2, Color.green, 0.1f );
                    Debug.DrawRay( newCurve[ i ], Quaternion.Euler( 0.0f, 0.0f, -alpha ) * prevDir.normalized * 2, Color.red, 0.1f );
                }

                i++;
            }

            Vector3 finalDir = newCurve[ newCurve.Count - 1 ] - newCurve[ newCurve.Count - 2 ];
            Debug.DrawRay( newCurve[ newCurve.Count - 1 ], Quaternion.Euler( 0.0f, 0.0f, -alpha ) * finalDir.normalized * 10, Color.yellow, 0.1f );


            //newCurve.Add(curve_Points[curve_Points.Length - 1]);

            curve_Points = new Vector3[ newCurve.Count ];
            
            int h = 0;

            foreach( Vector3 point in newCurve )
            {
                curve_Points[ h ] = point;
                h++;
            }

            curve_Quadratic.positionCount = curve_Points.Length;
            curve_Quadratic.SetPositions( curve_Points );
        }
    }

    private float CurveLenght()
    {
        float distance = 0.0f;

        for( int i = 0; i < curve_Points.Length - 1; i++ )
        {
            distance = Vector3.Distance( curve_Points[ i ], curve_Points[ i + 1 ] );
        }

        return distance;
    }

    private void ConstantSegmentsCurve()
    {
        if( segment_Constant )
        {
            List<Vector3> newCurve = new List<Vector3>();

            /*float curve_Length = CurveLenght();

            if(curve_Length % segment_ContantSize != 0.0f)
            {
                segment_ContantSize = curve_Length/(float)((int)(curve_Length / segment_ContantSize) + 1);
            }*/

            //Debug.Log( "Lunghezza curva: " + CurveLenght() + " | D/L = N = " + CurveLenght() / segment_ContantSize );
            float curveLenght = CurveLenght();
            float fixedLenght = curveLenght / curve_Points.Length;

            newCurve.Add( curve_Points[ 0 ] );

            int i = 0;
            int k = 1;

            while( k < curve_Points.Length )
            {
                //if( ( curve_Points[ k ] - newCurve[ i ] ).magnitude < segment_ContantSize )
                if( ( curve_Points[ k ] - newCurve[ i ] ).magnitude < fixedLenght )
                {
                    k++;
                }
                else
                {
                    //newCurve.Add( newCurve[ i ] + ( ( curve_Points[ k ] - newCurve[ i ] ).normalized * segment_ContantSize ) );
                    newCurve.Add( newCurve[ i ] + ( ( curve_Points[ k ] - newCurve[ i ] ).normalized * fixedLenght ) );
                    i++;
                }
            }

            curve_Points = new Vector3[ newCurve.Count ];
            
            int h = 0;

            foreach( Vector3 point in newCurve )
            {
                curve_Points[ h ] = point;
                h++;
            }

            curve_Quadratic.positionCount = curve_Points.Length;
            curve_Quadratic.SetPositions( curve_Points );
        }

    }

    private void DrawBezierCurve()
    {
        LineRenderer reference_Line = new GameObject().AddComponent<LineRenderer>();
        reference_Line.material.color = Color.red;
        reference_Line.startWidth = 1.0f;
        reference_Line.endWidth = 1.0f;

        curve_Quadratic.positionCount = curve_BezierPointsNumber;
        reference_Line.positionCount = curve_Points.Length;

        curve_Points = new Vector3[ curve_BezierPointsNumber ];

        for(int k = 0; k < curve_BezierPointsNumber; k++)
        {
            float t = k / ( float )( curve_BezierPointsNumber - 1 );
            curve_Points[ k ] = BezierCurveCalculator.CalculateNPoint( t, curve_ControlPoints );
        }

        curve_Quadratic.SetPositions( curve_Points );

        reference_Line.SetPositions( curve_Points );
        
    }

    /*private void CalculateMeshVertex()
    {
        Vector3 dir;

        floor_LeftPoints = new List<Vector3>();
        floor_RightPoints = new List<Vector3>();

        dir = Quaternion.Euler(0.0f, 0.0f, 90.0f) * (curve_Points[0] - curve_Points[1]).normalized * tunnel_width;
        floor_LeftPoints.Add(curve_Points[0] + dir);
        floor_RightPoints.Add(curve_Points[0] - dir);


        for(int i = 1; i < curve_BezierPointsNumber - 1; i++)
        {
            dir = Quaternion.Euler(0.0f, 0.0f, 90.0f) * (curve_Points[i - 1] - curve_Points[i + 1]).normalized * tunnel_width;

            floor_LeftPoints.Add(curve_Points[i] + dir);
            floor_RightPoints.Add(curve_Points[i] - dir);
        }


        dir = Quaternion.Euler(0.0f, 0.0f, 90.0f) * (curve_Points[curve_BezierPointsNumber - 2] - curve_Points[curve_BezierPointsNumber - 1]).normalized * tunnel_width;
        floor_LeftPoints.Add(curve_Points[curve_BezierPointsNumber - 1] + dir);
        floor_RightPoints.Add(curve_Points[curve_BezierPointsNumber - 1] - dir);

    }*/

    private Vector3[,] VertexMatrix_2xM( List<Vector3> up, List<Vector3> down )
    {
        if( up.Count == down.Count )
        {
            Vector3[ , ] VertexMatrix = new Vector3[ up.Count, down.Count ];

            for( int row = 0; row < 2; row++ )
            {
               for( int col = 0; col < up.Count; col++ )
               {
                    if( row == 0 )
                    {
                        VertexMatrix[ row, col ] = up[ col ];
                    }
                    else
                    {
                        VertexMatrix[ row, col ] = down[ col ];
                    }
               } 
            }

            return VertexMatrix;

        }
        else
        {
            return null;
        }
    }

    private void CalculateLinearMeshVertex()
    {
        Vector3 dir;

        floor_LeftPoints = new List<Vector3>();
        floor_RightPoints = new List<Vector3>();

        float zHeightPrev = 0.0f;
        float zHeightNext = 0.0f;

        if( mesh_ParabolicCurves )
        {
            zHeightPrev = curve_Points[ 0 ].z;
            zHeightNext = curve_Points[ 1 ].z;
        }

        //dir = Quaternion.Euler(0.0f, 0.0f, 90.0f) * (new Vector3(curve_Points[0].x, curve_Points[0].y, zHeightPrev) - new Vector3(curve_Points[1].x, curve_Points[1].y, zHeightNext)).normalized * tunnel_width;
        dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve_Points[ 0 ].x, curve_Points[ 0 ].y, zHeightPrev ) - new Vector3( curve_ControlPoints[ 1 ].transform.position.x, curve_ControlPoints[ 1 ].transform.position.y, zHeightNext ) ).normalized * tunnel_width;


        floor_LeftPoints.Add( curve_Points[ 0 ] + dir );
        floor_RightPoints.Add(curve_Points[ 0 ] - dir );

        for( int i = 1; i < curve_Points.Length - 1; i++ )
        {
            if( mesh_ParabolicCurves )
            {
                zHeightPrev = curve_Points[ i - 1 ].z;
                zHeightNext = curve_Points[ i + 1 ].z;
            }

            dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3(curve_Points[ i - 1 ].x, curve_Points[ i - 1 ].y, zHeightPrev ) - new Vector3( curve_Points[ i + 1 ].x, curve_Points[ i + 1 ].y, zHeightNext ) ).normalized * tunnel_width;

            floor_LeftPoints.Add( curve_Points[ i ] + dir );
            floor_RightPoints.Add(curve_Points[ i ] - dir );
        }

        if( mesh_ParabolicCurves )
        {
            zHeightPrev = curve_Points[ curve_Points.Length - 2 ].z;
            zHeightNext = curve_Points[ curve_Points.Length - 1 ].z;
        }

        dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve_Points[ curve_Points.Length - 2 ].x, curve_Points[ curve_Points.Length - 2 ].y, zHeightPrev ) - new Vector3( curve_Points[ curve_Points.Length - 1 ].x, curve_Points[ curve_Points.Length - 1 ].y, zHeightNext ) ).normalized * tunnel_width;
        floor_LeftPoints.Add( curve_Points[ curve_Points.Length - 1 ] + dir );
        floor_RightPoints.Add( curve_Points[ curve_Points.Length - 1 ] - dir );

    }

    private void GenerateLinearMesh( Vector3[,] vertMatrix )
    {
        mesh_Pavement = new Mesh();

        /*if(this.gameObject.GetComponent<MeshFilter>() == null)
        {
            this.gameObject.AddComponent<MeshFilter>();
        }
        if(this.gameObject.GetComponent<UnityEngine.MeshRenderer>() == null)
        {
            this.gameObject.AddComponent<UnityEngine.MeshRenderer>();
        }
        this.gameObject.GetComponent<MeshFilter>().sharedMesh = mesh_Pavement;*/

        mesh_Vertices = new Vector3[ curve_Points.Length * 2 ];
        mesh_Edges = new int[( curve_Points.Length - 1) * 6 ];
        mesh_UVs = new Vector2[ mesh_Vertices.Length ];


        for( int i = 0; i < curve_Points.Length; i++ )
        {
            mesh_Vertices[ i * 2 ] = vertMatrix[ 0, i ];
            mesh_Vertices[ ( i * 2 ) + 1 ] = vertMatrix[ 1, i ];

            float meshPercent = i / ( float )( mesh_Vertices.Length - 1 );
            mesh_UVs[ ( i * 2 ) ] = new Vector2( meshPercent, 0 );
            mesh_UVs[ ( i * 2 ) + 1 ] = new Vector2( meshPercent, 1 );

            if( i < curve_Points.Length - 1 )
            {

                mesh_Edges[ ( i * 6 ) + 0 ] = ( i * 2 );
                mesh_Edges[ ( i * 6 ) + 1 ] = mesh_Edges[ ( i * 6 ) + 4 ] = ( i * 2 ) + 2;
                mesh_Edges[ ( i * 6 ) + 2 ] = mesh_Edges[ ( i * 6 ) + 3 ] = ( i * 2 ) + 1;
                mesh_Edges[ ( i * 6 ) + 5 ] = ( i * 2 ) + 3;

            }
        }

        mesh_Pavement.Clear();
        mesh_Pavement.vertices = mesh_Vertices;
        mesh_Pavement.triangles = mesh_Edges;
        mesh_Pavement.uv = mesh_UVs;

        mesh_Pavement.RecalculateNormals();
    }

    /*private Vector3 CalculateQuadraticBezierPoint(float t, List<Transform> points)
    {
        //B(t) = (1 - t)² * P0 + [2 * t * (1 - t)] * P1 + t² * P2

        return (Mathf.Pow((1 - t), 2) * points[0].position) + (2 * t * (1 - t) * points[1].position) + (Mathf.Pow(t, 2) * points[2].position);

    }*/

    private void OnDrawGizmos()
    {
        if ( mesh_Vertices == null )
        {
            return;
        }

        for( int i = 0; i < mesh_Vertices.Length; i++ )
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere( mesh_Vertices[i], 0.0666f );
        }
    }   
    
}
