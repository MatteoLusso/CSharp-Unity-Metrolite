using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MetroGenerator : MonoBehaviour
{
    [ Header ( "Giocatore" ) ]
    public GameObject train;
    public GameObject mainCamera;
    public float trainHeightFromGround = 1.5f;

    [ Header ( "Parametri metropolitana" ) ]
    public int seed = 0;
    public bool randomSeed = false;
    public int controlPointsNumber = 3;
    public int distanceMultiplier = 25;
    public int sectionsNumber = 1;
    public float maxAngle = 2.5f;
    public int stationsDistanceMin = 4;
    public int stationsDistanceMax = 8;
    private int stationsDistance;
    public int switchDistanceMin = 2;
    public int switchDistanceMax = 10;
    private int switchDistance;
    public float stationLenght = 200.0f;
    public float stationExtensionLenght = 100.0f;
    public float stationExtensionHeight = 225.0f;
    public int stationExtensionCurvePoints = 10;
    public float switchLenght = 25.0f;
    public float switchCentralWallDistanceCorrection = 10.0f;
    public int switchCentralWallSkipDown = 15; 
    public int switchCentralWallSkipUp = 15; 
    public float switchCentralWallBezControlPointsDistance = 25.0f;
    public int switchCentralWallBezierPoints = 15;
    public int baseBezierCurvePointsNumber = 50;
    public bool tunnelParabolic = false;
    public float tunnelStraightness = 0.5f;
    public bool startingBidirectional = true;
    public Dictionary<string, List<LineSection>> lines = new Dictionary<string, List<LineSection>>();
    public Dictionary<string, List<List<Vector3>>> proibitedAreas = new Dictionary<string, List<List<Vector3>>>();
    private int lineCounter = 0;
    public int lineTurnDistanceMin = 5;
    public int lineTurnDistanceMax = 20;
    private int lineTurnDistance;
    public float switchBracketsLenght = 50.0f;
    public bool newLineFromSwitch = false;

    [ Header ( "Parametri stazione" ) ]
    public float yellowLineWidth = 5.0f;
    public List<Vector3> stationPlatformShape;
    public float stationPlatformShapeScale = 1.0f;
    public float stationPlatformSmoothFactor = 0.5f;
    public float stationShapeHorPosCorrection = 0.0f;

    [ Header ( "Parametri props" ) ]
    public GameObject pillar;
    public Vector3 pillarRotationCorrection = Vector3.zero;
    public Vector3 pillarPositionCorrection = Vector3.zero;
    public float pillarMinDistance = 5.0f;
    public float pillarMaxDistance = 10.0f;
    public GameObject fan;
    public Vector3 fanRotationCorrection = Vector3.zero;
    public Vector3 fanPositionCorrection = Vector3.zero;
    public float fanMinDistance = 100.0f;
    public float fanMaxDistance = 250.0f;
    public float tunnelWallHeight = 0.5f;
    public GameObject banister;
    public Vector3 banisterRotationCorrection = Vector3.zero;
    public Vector3 banisterPositionCorrectionLeft = Vector3.zero;
    public Vector3 banisterPositionCorrectionRight = Vector3.zero;
    public float banisterMinDistance = 8.5f;
    public float banisterMaxDistance = 25.0f;

    [ Header ( "Parametri tunnel" ) ]
    public float tunnelWidth = 5.0f;
    public float railsWidth = 3.0f;
    public float centerWidth = 5.0f;
    public float platformHeight = 0.5f;
    public float platformWidth = 3.5f;

    [ Header ( "Parametri muro tunnel" ) ]
    public List<Vector3> tunnelWallShape;
    public float tunnelWallShapeScale = 1.0f;
    public float tunnelWallSmoothFactor = 0.5f;
    public float tunnelWallShapeHorPosCorrection = 0.0f;

    [ Header ( "Parametri muro centrale stazione" ) ]
    public List<Vector3> stationCentralWallShape;
    public float stationCentralWallShapeScale = 1.0f;
    public float stationCentralWallSmoothFactor = 0.5f;
    public float stationCentralWallShapeHorPosCorrection = 0.0f;

    [ Header ( "Parametri fili pavimento" ) ]
    public float groundWireShapeScale = 1.0f;
    public float groundWireSmoothFactor = 0.5f;
    public float groundWireRotationCorrection = 0.0f;
    //public float groundWireHorPosCorrection = 0.0f;
    // public float groundWireMinLenght = 2.0f;
    // public float groundWireMaxLenght = 8.0f;
    
    public int groundWireControlPointsNumber = 4;
    public int groundWireBezierPointsNumber = 8;

    public float groundWireFloating = 2.0f;
    public int groundWiresMinNumber = 0;
    public int groundWiresMaxNumber = 3;
    public GameObject groundWiresFuseBox;
    public Vector3 groundWiresFuseBoxRotCorrection;

        [ Header ( "Parametri fili muro" ) ]
    public float wallWireShapeScale = 1.0f;
    public float wallWireSmoothFactor = 0.5f;
    public float wallWireRotationCorrection = 0.0f;
    public float wallWireHorPosCorrection = 0.0f;
    public float wallWireVertPosCorrection = 0.0f;
    public float wallWireMinLenght = 2.0f;
    public float wallWireMaxLenght = 8.0f;
    public float wallWireFloating = 2.5f;
    public int wallWiresMinNumber = 1;
    public int wallWiresMaxNumber = 3;
    public int wallWiresBezierPoints = 5;
    // public GameObject wallWiresFuseBox;
    // public Vector3 wallWiresFuseBoxRotCorrection;

    [ Header ( "Parametri binario tunnel" ) ]
    public List<Vector3> railShape;
    public float railSmoothFactor= 0.5f;
    public float railShapeScale = 1.0f;
    public float railShapeHorPosCorrection = 0.0f;

    [ Header ( "Parametri pavimento banchina" ) ]
    public List<Vector3> platformFloorShape;
    public float platformFloorSmoothFactor= 0.5f;
    public float platformFloorShapeScale = 1.0f;
    public float platformFloorHorPosCorrection = 0.0f;

    [ Header ( "Parametri tombino scambio" ) ]
    public float grateWidth = 3.0f;

    [ Header ( "Parametri tubature muri" ) ]
    public float tubeShapeRadius = 1.0f;
    public int tubeShapePoints;
    public float tubeShapeEccentricity = 1.0f;
    public float tubeSmoothFactor= 0.5f;
    public float tubeShapeScale = 1.0f;
    public float tubeHorPosCorrection = 0.0f;
    public float tubeHeightFromPlatform = 6.0f;
    public float tubeDistanceCorrection = 2.0f;
    
    [ Header ( "Texture" ) ]
    public Material tunnelRailTexture;
    public Vector2 tunnelRailTextureTilting;
    public Material platformSideTexture;
    public Vector2 platformSideTextureTilting;
    public Material platformFloorTexture;
    public Vector2 platformFloorTextureTilting;
    public Material tunnelWallTexture;
    public Vector2 tunnelWallTextureTilting;
    public Material ceilingTexture;
    public Vector2 ceilingTextureTilting;
    public Material railTexture;
    public Vector2 railTextureTilting;
    public Material switchRailTexture;
    public Vector2 switchRailTextureTilting;
    public Material switchGroundTexture;
    public Vector2 switchGroundTextureTilting;
    public Material centerTexture;
    public Vector2 centerTextureTilting;
    public Material grateTexture;
    public Vector2 grateTextureTilting;
    public Material tubeTexture;
    public Vector2 tubeTextureTilting;
    public Material wireTexture;
    public Vector2 wireTextureTilting;

    [ Header ( "Semafori scambi" ) ]
    public GameObject switchLight;
    public float switchLightDistance;
    public float switchLightHeight;
    public Vector3 switchLightRotation;

    [ Header ( "Debug" ) ]
    public bool ready = false;
    public bool lineGizmos = true;

    void Start()
    {
        GenerateMetro();
    }

    public void GenerateMetro() {
        this.ready = false;
        
        this.seed = this.randomSeed ? ( int )Random.Range( 0, 999999 ) : this.seed;
        Random.InitState( seed ); 
        Debug.Log( ">>> seed: " + seed );

        GenerateLine( "Linea " + lineCounter, CardinalPoint.None, CardinalPoint.Random, sectionsNumber, Vector3.zero, Vector3.zero, newLineFromSwitch, null );

        foreach( LineSection section in lines[ "Linea " + ( lineCounter - 1 ) ] ) {
            if( section.newLinesStarts != null && section.newLinesStarts.Count > 0 ) {
                if( section.newLinesStarts != null ) {
                    List<NewLineSide> sides = new List<NewLineSide>( section.newLinesStarts.Keys );
                    for( int i = 0; i < sides.Count; i++ ) {
                        LineStart start = section.newLinesStarts[ sides[ i ] ];

                        if( !start.generated ) {
                            GenerateLine( "Linea " + lineCounter, start.previousOrientation, start.orientation, sectionsNumber/ 2, start.pos, start.dir, false, section );
                            start.generated = true;
                            start.lineName = "Linea " + ( lineCounter - 1 );

                            if( !lines.ContainsKey( "Linea " + ( lineCounter - 1 ) ) ) {
                                section.newLinesStarts.Remove( sides[ i ] );
                            }
                        }
                    }
                }
            }
        }

        GenerateMeshes();
        AddProps();
        InstantiateTrain();

        this.ready = true;
    }

    private MeshGenerator.Floor GenerateSwitchNewBiCentralBaseLine( List<Vector3> c0, List<Vector3> c1, List<Vector3> s0, float s0DistanceCorrection, int skipDown, int skipUp, float bezControlPointsDistance, int bezierPoints ) {
        MeshGenerator.Floor ground = new MeshGenerator.Floor();

        List<Vector3> baseLine = new List<Vector3>();
        
        List<Vector3> reducedC0 = new List<Vector3>();
        List<Vector3> reducedC1 = new List<Vector3>();
        List<Vector3> reducedS0 = new List<Vector3>();

        List<Vector3> bezCP0 = new List<Vector3>();
        List<Vector3> bezCP1 = new List<Vector3>();
        List<Vector3> bezCP2 = new List<Vector3>();
        List<Vector3> bez0 = new List<Vector3>();
        List<Vector3> bez1 = new List<Vector3>();
        List<Vector3> bez2 = new List<Vector3>();

        float r = 0.0f;
        for( int i = 1; i < skipDown; i++ ) {
            r += Vector3.Distance( c0[ i - 1 ], c0[ i ] );
        }

        Vector3 s0Dir = ( s0[ s0.Count - 1 ] - s0[ 0 ] ).normalized * ( r + s0DistanceCorrection );
        reducedS0.Add( s0[ s0.Count - 1 ] - s0Dir );
        reducedS0.Add( s0[ 0 ] + s0Dir );
        
        for( int i = skipDown; i < c0.Count - skipUp; i++ )  {
            int j = c0.Count - 1 - i;

            reducedC0.Add( c0[ i ] );
            reducedC1.Add( c1[ j ] );
            
        }
        reducedC1.Reverse();

        bezCP0.Add( reducedS0[ 1 ] );
        bezCP0.Add( reducedS0[ 1 ] - ( s0Dir.normalized * bezControlPointsDistance ) );
        bezCP0.Add( reducedC0[ 0 ] + ( reducedC0[ 0 ] - reducedC0[ 1 ] ).normalized * bezControlPointsDistance );
        bezCP0.Add( reducedC0[ 0 ] );

        bezCP1.Add( reducedC0[ reducedC0.Count - 1 ] );
        bezCP1.Add( reducedC0[ reducedC0.Count - 1 ] + ( reducedC0[ reducedC0.Count - 1 ] - reducedC0[ reducedC0.Count - 2 ] ).normalized * bezControlPointsDistance );
        bezCP1.Add( reducedC1[ 0 ] + ( reducedC1[ 0 ] - reducedC1[ 1 ] ).normalized * bezControlPointsDistance );
        bezCP1.Add( reducedC1[ 0 ] );

        bezCP2.Add( reducedC1[ reducedC1.Count - 1 ] );
        bezCP2.Add( reducedC1[ reducedC1.Count - 1 ] + ( reducedC1[ reducedC1.Count - 1 ] - reducedC1[ reducedC1.Count - 2 ] ).normalized * bezControlPointsDistance );
        bezCP2.Add( reducedS0[ 0 ] + ( s0Dir.normalized * bezControlPointsDistance ) );
        bezCP2.Add( reducedS0[ 0 ] );

        bez0 = BezierCurveCalculator.CalculateBezierCurve( bezCP0, bezierPoints );
        bez1 = BezierCurveCalculator.CalculateBezierCurve( bezCP1, bezierPoints );
        bez2 = BezierCurveCalculator.CalculateBezierCurve( bezCP2, bezierPoints );

        baseLine.AddRange( reducedS0 );
        baseLine.RemoveAt( baseLine.Count - 1 );
        baseLine.AddRange( bez0 );
        baseLine.RemoveAt( baseLine.Count - 1 );
        baseLine.AddRange( reducedC0 );
        baseLine.RemoveAt( baseLine.Count - 1 );
        baseLine.AddRange( bez1 );
        baseLine.RemoveAt( baseLine.Count - 1 );
        baseLine.AddRange( reducedC1 );
        baseLine.RemoveAt( baseLine.Count - 1 );
        baseLine.AddRange( bez2 );
        baseLine.RemoveAt( baseLine.Count - 1 );

        ground.switchBiNewGroundBaseLine = baseLine;
        ground.switchBiNewGroundBez0Line = bez0;
        ground.switchBiNewGroundBez1Line = bez1;
        ground.switchBiNewGroundBez2Line = bez2;

        return ground;
    }

    private void InstantiatePlane( Transform parentTransform, string gameObjName, List<Vector3> curve, List<Vector3> curveLeft, List<Vector3> curveRight, float verticalPosCorrection, bool clockwiseRotation, bool closeMesh, bool centerTexture, Material textureMaterial, Vector2 textureTilting ) {
        
        Mesh planarMesh = MeshGenerator.GeneratePlanarMesh( curve, MeshGenerator.ConvertListsToMatrix_2xM( curveLeft, curveRight ), clockwiseRotation, closeMesh, centerTexture, textureTilting.x, textureTilting.y );
        planarMesh.name = "Procedural Planar Mesh";

        GameObject planarGameObj = new GameObject( gameObjName );
        planarGameObj.transform.parent = parentTransform;
        planarGameObj.transform.position = new Vector3( 0.0f, 0.0f, verticalPosCorrection );
        planarGameObj.AddComponent<MeshFilter>();
        planarGameObj.AddComponent<MeshRenderer>();
        planarGameObj.GetComponent<MeshFilter>().sharedMesh = planarMesh;
        planarGameObj.GetComponent<MeshRenderer>().material = textureMaterial;
    }

    private Dictionary<Side, List<Vector3>> InstantiateGrate( Transform parentTransform, string gameObjName, List<Vector3> initialVertices, List<Vector3> dirs, float width, bool inverseSides, Material textureMaterial, Vector2 textureTilting ) {
        
        Dictionary<Side, List<Vector3>> planarMeshVertices = new Dictionary<Side, List<Vector3>>();
        planarMeshVertices.Add( inverseSides ? Side.Bottom : Side.Top, initialVertices );
        planarMeshVertices.Add( inverseSides ? Side.Top : Side.Bottom, new List<Vector3>{ initialVertices[ 0 ] + ( dirs[ 0 ].normalized * width ), initialVertices[ 1 ] + ( dirs[ 1 ].normalized * width ) } );

        Mesh planarMesh = MeshGenerator.GeneratePlanarMesh( planarMeshVertices[ Side.Top ], MeshGenerator.ConvertListsToMatrix_2xM( planarMeshVertices[ Side.Top ], planarMeshVertices[ Side.Bottom ] ), false, false, false, textureTilting.x, textureTilting.y );
        planarMesh.name = "Procedural Planar Mesh";

        GameObject planarGameObj = new GameObject( gameObjName );
        planarGameObj.transform.parent = parentTransform;
        planarGameObj.transform.position = new Vector3( 0.0f, 0.0f, -0.01f );
        planarGameObj.AddComponent<MeshFilter>();
        planarGameObj.AddComponent<MeshRenderer>();
        planarGameObj.GetComponent<MeshFilter>().sharedMesh = planarMesh;
        planarGameObj.GetComponent<MeshRenderer>().material = textureMaterial;

        return planarMeshVertices;
    }

    private MeshGenerator.ExtrudedMesh InstantiateExtrudedMesh( Transform parentTransform, string gameObjName, List<Vector3> baseLine, List<Vector3> profileShape, List<Vector3> previousProfileShape, 
                                           float profileScale, float profileHorPositionCorrection, float profileVertPositionCorrection, float profileRotationCorrection, float profileSmoothFactor, 
                                           bool clockwiseRotation, bool closeMesh, Material textureMaterial, Vector2 textureTilting ) {

        if( previousProfileShape == null || previousProfileShape.Count == 0 ) {
            previousProfileShape = null;
        }
        
        MeshGenerator.ExtrudedMesh extrudedMesh = MeshGenerator.GenerateExtrudedMesh( profileShape, profileScale, previousProfileShape, baseLine, profileHorPositionCorrection, profileVertPositionCorrection, clockwiseRotation, closeMesh, textureTilting.x, textureTilting.y, profileRotationCorrection, profileSmoothFactor );
        extrudedMesh.mesh.name = "Procedural Extruded Mesh";

        GameObject extrudedGameObj = new GameObject( gameObjName );
        extrudedGameObj.transform.parent = parentTransform;
        extrudedGameObj.transform.position = Vector3.zero;
        extrudedGameObj.AddComponent<MeshFilter>();
        extrudedGameObj.AddComponent<MeshRenderer>();
        extrudedGameObj.GetComponent<MeshFilter>().sharedMesh = extrudedMesh.mesh;
        extrudedGameObj.GetComponent<MeshRenderer>().material = textureMaterial;

        extrudedMesh.gameObj = extrudedGameObj;

        return extrudedMesh;
    }

    private void InstantiateGroundWires( Transform parentTransform, string gameObjName, int wiresNumber, GameObject fuseBoxPrefab, List<Vector3> baseLine, List<Vector3> profileShape, 
                                        float profileScale, float profileRotationCorrection, float profileSmoothFactor, 
                                        float minLenght, float maxLenght, float floating,
                                        bool clockwiseRotation, bool closeMesh, Material textureMaterial, Vector2 textureTilting ) {

        List<Vector3>[] wiresBaseLines = new List<Vector3>[ wiresNumber ];

        for( int i = 0; i < wiresNumber; i++ ) {
            wiresBaseLines[ i ] = new List<Vector3>();
        }

        for( int i = 0; i < baseLine.Count - 1; i++ ) {

            Vector3 dir = baseLine[ i + 1 ] - baseLine[ i ];

            for( float lenght = 0.0f; lenght < dir.magnitude && ( dir.magnitude - lenght ) >= minLenght; lenght += Random.Range( minLenght, maxLenght ) ) {
                
                Vector3[] nextWiresPoints = new Vector3[ wiresNumber ];
                
                for( int k = 0; k < wiresNumber; k++ ) {
                    nextWiresPoints[ k ] = baseLine[ i ] + dir.normalized * lenght;

                    if( lenght > 0.0f ) {
                        nextWiresPoints[ k ] += Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * dir.normalized * Random.Range( -floating, floating );
                    }

                    wiresBaseLines[ k ].Add( nextWiresPoints[ k ] );
                }
            }
        }

        Vector3 finalDir = baseLine[ baseLine.Count - 1 ] - baseLine[ baseLine.Count - 2 ];
        GameObject fuseBox = Instantiate( fuseBoxPrefab, baseLine[ baseLine.Count - 1 ], Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( finalDir, Vector3.right, -Vector3.forward ) ) );
        fuseBox.transform.Rotate( this.groundWiresFuseBoxRotCorrection, Space.Self );
        fuseBox.transform.parent = parentTransform;
        fuseBox.name = "Fuse Box";

        for( int i = 0; i < wiresNumber; i++ ) {
            wiresBaseLines[ i ].Add( baseLine[ baseLine.Count - 1 ] );

            InstantiateExtrudedMesh( parentTransform, gameObjName, wiresBaseLines[ i ], profileShape, null, profileScale, 0.0f, 0.0f, profileRotationCorrection, profileSmoothFactor, clockwiseRotation, closeMesh, textureMaterial, textureTilting );
        }
    }

    private void InstantiateGroundWires2( Transform parentTransform, string gameObjName, int wiresNumber, GameObject fuseBoxPrefab, List<Vector3> baseLine, List<Vector3> profileShape, 
                                        float profileScale, float profileRotationCorrection, float profileSmoothFactor, 
                                        int controlPointsNumber, int bezierPointsNumber, float floating,
                                        bool clockwiseRotation, bool closeMesh, Material textureMaterial, Vector2 textureTilting ) {
        
        //List<Vector3>[] wiresControlPoints = new List<Vector3>[ wiresNumber ];
        List<Vector3>[] wiresBaseLines = new List<Vector3>[ wiresNumber ];

        for( int k = 0; k < wiresNumber; k++ ) {
            wiresBaseLines[ k ] = new List<Vector3>();
        }

        for( int i = 0; i < baseLine.Count - 1; i++ ) {

            Vector3 dir = baseLine[ i + 1 ] - baseLine[ i ];
            float partialLenght = dir.magnitude / controlPointsNumber;

            for( int k = 0; k < wiresNumber; k++ ) { 

                List<Vector3> wiresControlPoints = new List<Vector3>();
                wiresControlPoints.Add( baseLine[ i ] );
                
                for( int j = 1; j < controlPointsNumber - 1; j ++ ) {
                    wiresControlPoints.Add( baseLine[ i ] + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * dir.normalized * Random.Range( -floating, floating ) ) + ( dir.normalized * ( ( Random.Range( -0.5f, 0.5f ) * partialLenght ) + ( partialLenght * j  ) ) ) );
                }

                wiresControlPoints.Add( baseLine[ i + 1 ] );

                wiresBaseLines[ k ].AddRange( BezierCurveCalculator.CalculateBezierCurve( wiresControlPoints, bezierPointsNumber ) );

                if( i < baseLine.Count - 2 ) {
                    wiresBaseLines[ k ].RemoveAt( wiresBaseLines[ k ].Count - 1 );
                }

            }
        }

        Vector3 finalDir = baseLine[ baseLine.Count - 1 ] - baseLine[ baseLine.Count - 2 ];
        GameObject fuseBox = Instantiate( fuseBoxPrefab, baseLine[ baseLine.Count - 1 ], Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( finalDir, Vector3.right, -Vector3.forward ) ) );
        fuseBox.transform.Rotate( this.groundWiresFuseBoxRotCorrection, Space.Self );
        fuseBox.transform.parent = parentTransform;
        fuseBox.name = "Fuse Box";

        for( int k = 0; k < wiresNumber; k++ ) {
            InstantiateExtrudedMesh( parentTransform, gameObjName, wiresBaseLines[ k ], profileShape, null, profileScale, 0.0f, 0.0f, profileRotationCorrection, profileSmoothFactor, clockwiseRotation, closeMesh, textureMaterial, textureTilting );
        }
    }

    private void InstantiateWallWires( Transform parentTransform, string gameObjName, int wiresNumber, int bezierPoints, /*GameObject fuseBoxPrefab,*/ List<Vector3> baseLine, List<Vector3> profileShape, 
                                        float profileScale, float profileRotationCorrection, float profileHorPosCorrection, float profileVertPosCorrection, float profileSmoothFactor, 
                                        float minLenght, float maxLenght, float floating,
                                        bool clockwiseRotation, bool closeMesh, Material textureMaterial, Vector2 textureTilting ) {
        
        List<Vector3> wiresControlPoints = new List<Vector3>();
        List<Vector3>[] wiresBaseLines = new List<Vector3>[ wiresNumber ];

        for( int i = 0; i < wiresNumber; i++ ) {
            wiresBaseLines[ i ] = new List<Vector3>();
        }

        for( int i = 0; i < baseLine.Count - 1; i++ ) {

            Vector3 dir = baseLine[ i + 1 ] - baseLine[ i ];

            for( float lenght = 0.0f; lenght < dir.magnitude && ( dir.magnitude - lenght ) >= minLenght; lenght += Random.Range( minLenght, maxLenght ) ) {

                wiresControlPoints.Add( baseLine[ i ] + dir.normalized * lenght );
            }
        }

        wiresControlPoints.Add( baseLine[ baseLine.Count - 1 ] );

        for( int i = 0; i < wiresControlPoints.Count - 1; i++ ) {

            // Vector3 finalDir = wiresControlPoints[ i + 1 ] - wiresControlPoints[ i ];
            // GameObject fuseBox = Instantiate( fuseBoxPrefab, wiresControlPoints[ i ] + ( -Vector3.forward * this.wallWireVertPosCorrection ), Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( finalDir, Vector3.right, -Vector3.forward ) ) );
            // fuseBox.transform.Rotate( this.wallWiresFuseBoxRotCorrection, Space.Self );
            // fuseBox.transform.parent = parentTransform;
            // fuseBox.name = "Fuse Box";

            Vector3 dir = wiresControlPoints[ i + 1 ] - wiresControlPoints[ i ];
            Vector3 wallNormal = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * dir.normalized;

            for( int k = 0; k < wiresNumber; k++ ) { 
                List<Vector3> partialBezierCurve = BezierCurveCalculator.CalculateBezierCurve( new List<Vector3>{ wiresControlPoints[ i ], wiresControlPoints[ i ] + ( dir.normalized * dir.magnitude / 2 ) + ( Quaternion.AngleAxis( -90.0f, wallNormal ) * dir.normalized * Random.Range( 0.0f, floating ) ), wiresControlPoints[ i + 1 ] }, bezierPoints );
                if( i < wiresControlPoints.Count - 2 ) {
                    partialBezierCurve.RemoveAt( partialBezierCurve.Count - 1 );
                }
                wiresBaseLines[ k ].AddRange( partialBezierCurve );
            }
        }

        for( int k = 0; k < wiresNumber; k++ ) {     
            MeshGenerator.ExtrudedMesh wire = InstantiateExtrudedMesh( parentTransform, gameObjName, wiresBaseLines[ k ], profileShape, null, profileScale, this.wallWireHorPosCorrection, this.wallWireVertPosCorrection, profileRotationCorrection, profileSmoothFactor, clockwiseRotation, closeMesh, textureMaterial, textureTilting );
        }
    }

    private List<Vector3> InstantiateRail( Transform parentTransform, string gameObjName, List<Vector3> baseLine, List<Vector3> profileShape, List<Vector3> previousProfileShape, 
                                           float profileScale, float profileHorPositionCorrection, float profileRotationCorrection, float profileSmoothFactor, 
                                           bool clockwiseRotation, bool closeMesh, Material textureMaterial, Vector2 textureTilting,
                                           LineSection section, SwitchDirection? switchDirection ) {

        MeshGenerator.ExtrudedMesh rail = InstantiateExtrudedMesh( parentTransform, gameObjName, baseLine, profileShape, previousProfileShape, profileScale, profileHorPositionCorrection, 0.0f, profileRotationCorrection, profileSmoothFactor, clockwiseRotation, closeMesh, textureMaterial, textureTilting );

        if( section != null && section.type == Type.Switch && switchDirection != null ) {
            
            rail.gameObj.AddComponent<RailHighlighter>();
            rail.gameObj.GetComponent<RailHighlighter>().section = section;
            rail.gameObj.GetComponent<RailHighlighter>().direction = ( SwitchDirection )switchDirection;
        }

        return rail.lastProfileVertex;
    }

    private void GenerateTunnelGroundMeshes( LineSection section, GameObject sectionGameObj ) {

        MeshGenerator.Floor floorVertexPoints = new MeshGenerator.Floor();

        List<Vector3> wireShape = MeshGenerator.CalculateCircularShape( 1.0f, 10, Vector3.zero, 0.0f );

        if( section.bidirectional ) {

            floorVertexPoints = MeshGenerator.CalculateBidirectionalWithBothSidesPlatformFloorMeshVertex( section, this.centerWidth, this.tunnelWidth, this.railsWidth );

            // Generazione mesh planari pavimento tunnel
            InstantiatePlane( sectionGameObj.transform, "Tunnel - Pavimento binari - Sinistra", section.bezierCurveLimitedAngle, floorVertexPoints.leftL, floorVertexPoints.leftR, 0.0f, false, false, false, this.tunnelRailTexture, this.tunnelRailTextureTilting );
            InstantiatePlane( sectionGameObj.transform, "Tunnel - Pavimento divisore - Centro", section.bezierCurveLimitedAngle, floorVertexPoints.centerL, floorVertexPoints.centerR, 0.0f, false, false, false, this.centerTexture, this.centerTextureTilting );
            InstantiatePlane( sectionGameObj.transform, "Tunnel - Pavimento binari - Destra", section.bezierCurveLimitedAngle, floorVertexPoints.rightL, floorVertexPoints.rightR, 0.0f, false, false, false, this.tunnelRailTexture, this.tunnelRailTextureTilting );

            // Generazione mesh extruded binari
            InstantiateRail( sectionGameObj.transform, "Tunnel - Binario L - Sinistra", floorVertexPoints.railLeftL, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, null, null );
            InstantiateRail( sectionGameObj.transform, "Tunnel - Binario R - Sinistra", floorVertexPoints.railLeftR, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, null, null );
            InstantiateRail( sectionGameObj.transform, "Tunnel - Binario L - Destra", floorVertexPoints.railRightL, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, null, null );
            InstantiateRail( sectionGameObj.transform, "Tunnel - Binario R - Destra", floorVertexPoints.railRightR, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, null, null );

            //InstantiateGroundWires( sectionGameObj.transform, "Filo pavimento - Sinistra", Random.Range( this.groundWiresMinNumber, this.groundWiresMaxNumber + 1 ), this.groundWiresFuseBox, floorVertexPoints.leftLine, wireShape, this.groundWireShapeScale, this.groundWireRotationCorrection, this.groundWireSmoothFactor, this.groundWireMinLenght, this.groundWireMaxLenght, this.groundWireFloating, true, false, this.wireTexture, this.wireTextureTilting );
            InstantiateGroundWires2( sectionGameObj.transform, "Filo pavimento - Sinistra", Random.Range( this.groundWiresMinNumber, this.groundWiresMaxNumber + 1 ), this.groundWiresFuseBox, floorVertexPoints.leftLine, wireShape, this.groundWireShapeScale, this.groundWireRotationCorrection, this.groundWireSmoothFactor, this.groundWireControlPointsNumber, this.groundWireBezierPointsNumber, this.groundWireFloating, true, false, this.wireTexture, this.wireTextureTilting );
            //InstantiateGroundWires( sectionGameObj.transform, "Filo pavimento - Destra", Random.Range( this.groundWiresMinNumber, this.groundWiresMaxNumber + 1 ), this.groundWiresFuseBox, floorVertexPoints.rightLine, wireShape, this.groundWireShapeScale, this.groundWireRotationCorrection, this.groundWireSmoothFactor, this.groundWireMinLenght, this.groundWireMaxLenght, this.groundWireFloating, true, false, this.wireTexture, this.wireTextureTilting );
            InstantiateGroundWires2( sectionGameObj.transform, "Filo pavimento - Destra", Random.Range( this.groundWiresMinNumber, this.groundWiresMaxNumber + 1 ), this.groundWiresFuseBox, floorVertexPoints.rightLine, wireShape, this.groundWireShapeScale, this.groundWireRotationCorrection, this.groundWireSmoothFactor, this.groundWireControlPointsNumber, this.groundWireBezierPointsNumber, this.groundWireFloating, true, false, this.wireTexture, this.wireTextureTilting );


            section.bidirectional = true;
        }
        else {

            floorVertexPoints = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.bezierCurveLimitedAngle, section.controlsPoints[ 1 ] - section.bezierCurveLimitedAngle[ 0 ], tunnelWidth, tunnelParabolic, this.railsWidth );

            // Generazione mesh planari pavimento tunnel
            InstantiatePlane( sectionGameObj.transform, "Tunnel - Pavimento binari - Centro", section.bezierCurveLimitedAngle, floorVertexPoints.centerL, floorVertexPoints.centerR, 0.0f, false, false, false, this.tunnelRailTexture, this.tunnelRailTextureTilting );

            // Generazione mesh extruded binari
            InstantiateRail( sectionGameObj.transform, "Tunnel - Binario L - Centro", floorVertexPoints.railCenterL, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, null, null );
            InstantiateRail( sectionGameObj.transform, "Tunnel - Binario R - Centro", floorVertexPoints.railCenterR, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, null, null );

            //InstantiateGroundWires( sectionGameObj.transform, "Filo pavimento - Centro", Random.Range( this.groundWiresMinNumber, this.groundWiresMaxNumber + 1 ), this.groundWiresFuseBox, floorVertexPoints.centerLine, wireShape, this.groundWireShapeScale, this.groundWireRotationCorrection, this.groundWireSmoothFactor, this.groundWireMinLenght, this.groundWireMaxLenght, this.groundWireFloating, true, false, this.wireTexture, this.wireTextureTilting );
            InstantiateGroundWires( sectionGameObj.transform, "Filo pavimento - Centro", Random.Range( this.groundWiresMinNumber, this.groundWiresMaxNumber + 1 ), this.groundWiresFuseBox, floorVertexPoints.centerLine, wireShape, this.groundWireShapeScale, this.groundWireRotationCorrection, this.groundWireSmoothFactor, this.groundWireControlPointsNumber, this.groundWireBezierPointsNumber, this.groundWireFloating, true, false, this.wireTexture, this.wireTextureTilting );

            section.bidirectional = false;
        }

        // Update dettagli LineSection 
        section.floorPoints = floorVertexPoints;
    }

    private void InstantiateCeiling( Transform parentTransform, string gameObjName, List<Vector3> closedLine,  
                                     bool clockwiseRotation, Vector3 textureDir, Material textureMaterial, Vector2 textureTilting ) {

        Vector3 center = MeshGenerator.CalculatePoligonCenterPoint( closedLine );
        Mesh planarMesh = MeshGenerator.GeneratePlanarMesh( closedLine, center, clockwiseRotation, textureDir, textureTilting.x, textureTilting.y );

        GameObject planarGameObj = new GameObject( gameObjName );
        planarGameObj.transform.parent = parentTransform;
        planarGameObj.transform.position = Vector3.zero;
        planarGameObj.AddComponent<MeshFilter>();
        planarGameObj.AddComponent<MeshRenderer>();
        planarGameObj.GetComponent<MeshFilter>().sharedMesh = planarMesh;
        planarGameObj.GetComponent<MeshRenderer>().material = textureMaterial;
    }

    private void GenerateSwitchMeshes( string lineName, int i, LineSection section, GameObject sectionGameObj ) {
        SwitchPath switchPath = SwitchPath.CreateInstance( railsWidth, switchLenght, switchBracketsLenght, centerWidth, tunnelWidth, switchLightDistance, switchLightHeight, baseBezierCurvePointsNumber, switchLightRotation, switchLight );

        bool previousBidirectional = this.lines[ lineName ][ i - 1 ].bidirectional;

        LineSection switchSection = new LineSection();

        //Debug.Log( "previousBidirectional: " + previousBidirectional );

        if( previousBidirectional ) {
            if( section.newLinesStarts != null && section.newLinesStarts.Count > 0 ) {
                switchSection = switchPath.GenerateBiToNewBiSwitch( section, this.lines[ lineName ][ i - 1 ].nextStartingDirections[ 0 ], this.lines[ lineName ][ i - 1 ].nextStartingPoints[ 0 ], sectionGameObj );
            }
            else {

                if( Random.Range( 0, 2 ) == 0 ) {
                    switchSection = switchPath.GenerateBiToBiSwitch( this.lines[ lineName ], this.lines[ lineName ][ i - 1 ].nextStartingDirections[ 0 ], this.lines[ lineName ][ i - 1 ].nextStartingPoints[ 0 ], sectionGameObj );
                }
                else{
                    switchSection = switchPath.GenerateBiToMonoSwitch( this.lines[ lineName ], this.lines[ lineName ][ i - 1 ].nextStartingDirections[ 0 ], this.lines[ lineName ][ i - 1 ].nextStartingPoints[ 0 ], sectionGameObj );
                }
            }
        }
        else {
            if( section.newLinesStarts != null && section.newLinesStarts.Count > 0 ) {
                switchSection = switchPath.GenerateMonoToNewMonoSwitch( section, this.lines[ lineName ][ i - 1 ].nextStartingDirections[ 0 ], this.lines[ lineName ][ i - 1 ].nextStartingPoints[ 0 ], sectionGameObj );
            }
            else {

                if( Random.Range( 0, 2 ) == 0 || lineCounter > 0 ) {
                    switchSection = switchPath.GenerateMonoToBiSwitch( this.lines[ lineName ], this.lines[ lineName ][ i - 1 ].nextStartingDirections[ 0 ], this.lines[ lineName ][ i - 1 ].nextStartingPoints[ 0 ], sectionGameObj );
                }
            }
        }

        section.floorMesh = switchSection.floorMesh;
        section.activeSwitch = switchSection.activeSwitch;
        section.type = switchSection.type;
        section.switchType = switchSection.switchType;
        section.bidirectional = switchSection.bidirectional;
        section.switchLights = switchSection.switchLights;
        section.nextStartingDirections = switchSection.nextStartingDirections;
        section.nextStartingPoints = switchSection.nextStartingPoints;
        section.curvePointsCount = switchSection.curvePointsCount;
        section.floorPoints = switchSection.floorPoints;

        float baseWidth = ( this.tunnelWidth / 2 ) + this.platformWidth;
        float angleFromCenter = Mathf.Atan( this.platformHeight / baseWidth ) * Mathf.Rad2Deg;
        float distanceFromCenter = Mathf.Sqrt( Mathf.Pow( this.platformHeight, 2 ) + Mathf.Pow( baseWidth, 2 ) );

        switch( section.switchType ) {
            case SwitchType.MonoToBi:
            case SwitchType.BiToMono:       // Generazione mesh planari pavimento scambio
                                            MeshGenerator.Floor centerToLeftFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.leftCenterLine, null, tunnelWidth, false, this.railsWidth );
                                            MeshGenerator.Floor centerToRightFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.rightCenterLine, null, tunnelWidth, false, this.railsWidth );

                                            InstantiatePlane( sectionGameObj.transform, "Scambio - Pavimento binari - Sinistra/Centro", section.floorPoints.leftCenterLine, centerToLeftFloor.centerL, centerToLeftFloor.centerR, 0.0f, false, false, false, this.switchRailTexture, this.switchRailTextureTilting );
                                            InstantiatePlane( sectionGameObj.transform, "Scambio - Pavimento binari - Destra/Centro", section.floorPoints.rightCenterLine, centerToRightFloor.centerL, centerToRightFloor.centerR, 0.0f, false, false, false, this.switchRailTexture, this.switchRailTextureTilting );

                                            // Generazione mesh extruded binari
                                            InstantiateRail( sectionGameObj.transform, "Scambio - Binario L - Sinistra/Centro", centerToLeftFloor.railCenterL, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, section, SwitchDirection.LeftToCenter );
                                            InstantiateRail( sectionGameObj.transform, "Scambio - Binario R - Sinistra/Centro", centerToLeftFloor.railCenterR, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, section, SwitchDirection.LeftToCenter );
                                            InstantiateRail( sectionGameObj.transform, "Scammbio - Binario L - Destra/Centro", centerToRightFloor.railCenterL, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, section, SwitchDirection.RightToCenter );
                                            InstantiateRail( sectionGameObj.transform, "Scambio - Binario R - Destra/Centro", centerToRightFloor.railCenterR, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, section, SwitchDirection.RightToCenter );

                                            // Generazione mesh piattaforma laterale
                                            MeshGenerator.PlatformSide platformSidesLeftVertexPoints = MeshGenerator.CalculatePlatformSidesMeshesVertex( centerToLeftFloor.centerLine, null, tunnelWidth, tunnelParabolic, platformHeight, platformWidth );
                                            MeshGenerator.PlatformSide platformSidesRightVertexPoints = MeshGenerator.CalculatePlatformSidesMeshesVertex( centerToRightFloor.centerLine, null, tunnelWidth, tunnelParabolic, platformHeight, platformWidth );

                                            InstantiatePlane( sectionGameObj.transform, "Scambio - Piattaforma laterale (lato) - Sinistra", centerToLeftFloor.centerLine, platformSidesLeftVertexPoints.leftUp, platformSidesLeftVertexPoints.leftDown, 0.0f, false, false, false, this.platformSideTexture, this.platformSideTextureTilting );
                                            InstantiatePlane( sectionGameObj.transform, "Scambio - Piattaforma laterale (lato) - Destra", centerToRightFloor.centerLine, platformSidesRightVertexPoints.rightUp, platformSidesRightVertexPoints.rightDown, 0.0f, true, false, false, this.platformSideTexture, this.platformSideTextureTilting );

                                            section.sidePlatformFloorLeftLastProfile = InstantiateExtrudedMesh( sectionGameObj.transform, "Tunnel - Piattaforma laterale (pavimento) - Sinistra", platformSidesLeftVertexPoints.leftFloorRight, this.platformFloorShape, section.previousSection != null ? section.previousSection.sidePlatformFloorLeftLastProfile : null, this.platformFloorShapeScale, this.platformFloorHorPosCorrection, 0.0f, 0.0f, this.platformFloorSmoothFactor, true, false, this.platformFloorTexture, this.platformFloorTextureTilting ).lastProfileVertex;
                                            section.sidePlatformFloorRightLastProfile = InstantiateExtrudedMesh( sectionGameObj.transform, "Tunnel - Piattaforma laterale (pavimento) - Destra", platformSidesRightVertexPoints.rightFloorLeft, this.platformFloorShape, section.previousSection != null ? section.previousSection.sidePlatformFloorRightLastProfile : null, this.platformFloorShapeScale, this.platformFloorHorPosCorrection, 0.0f, 180.0f, this.platformFloorSmoothFactor, false, false, this.platformFloorTexture, this.platformFloorTextureTilting ).lastProfileVertex;

                                            // Generazione mesh muri

                                            Vector3? lastDir = section.previousSection == null ? null : ( Vector3? )( section.previousSection.bezierCurveLimitedAngle[ section.previousSection.bezierCurveLimitedAngle.Count - 1 ] - section.previousSection.bezierCurveLimitedAngle[ section.previousSection.bezierCurveLimitedAngle.Count - 2 ] );
                                            MeshGenerator.SpecularBaseLine wallBaseLinesLeft = MeshGenerator.CalculateBaseLinesFromCurve( centerToLeftFloor.centerLine, lastDir, distanceFromCenter, angleFromCenter );
                                            MeshGenerator.SpecularBaseLine wallBaseLinesRight = MeshGenerator.CalculateBaseLinesFromCurve( centerToRightFloor.centerLine, lastDir, distanceFromCenter, angleFromCenter );
                                      
                                            section.wallLeftLastProfile = InstantiateExtrudedMesh( sectionGameObj.transform, "Tunnel - Muro - Sinistra", wallBaseLinesLeft.left, this.tunnelWallShape, section.previousSection != null ? section.previousSection.wallLeftLastProfile : null, this.tunnelWallShapeScale, 0.0f, 0.0f, 0.0f, this.tunnelWallSmoothFactor, true, false, this.tunnelWallTexture, this.tunnelWallTextureTilting ).lastProfileVertex;
                                            section.wallRightLastProfile = InstantiateExtrudedMesh( sectionGameObj.transform, "Tunnel - Muro - Destra", wallBaseLinesRight.right, this.tunnelWallShape, section.previousSection != null ? section.previousSection.wallRightLastProfile : null, this.tunnelWallShapeScale, 0.0f, 0.0f, 180.0f, this.tunnelWallSmoothFactor, false, false, this.tunnelWallTexture, this.tunnelWallTextureTilting ).lastProfileVertex;

                                            // Generazione mesh tombini

                                            Vector3 dirLeft0_BiToMono_MonoToBi = ( platformSidesLeftVertexPoints.leftFloorLeft[ 1 ] - platformSidesLeftVertexPoints.leftFloorLeft[ 0 ] ).normalized;
                                            Vector3 dirRight0_BiToMono_MonoToBi = ( platformSidesRightVertexPoints.rightFloorRight[ 1 ] - platformSidesRightVertexPoints.rightFloorRight[ 0 ] ).normalized;
                                            Vector3 dirLeft1_BiToMono_MonoToBi = ( platformSidesLeftVertexPoints.leftFloorLeft[ platformSidesLeftVertexPoints.leftFloorLeft.Count - 1 ] - platformSidesLeftVertexPoints.leftFloorLeft[ platformSidesLeftVertexPoints.leftFloorLeft.Count - 2 ] ).normalized;
                                            Vector3 dirRight1_BiToMono_MonoToBi = ( platformSidesRightVertexPoints.rightFloorRight[ platformSidesRightVertexPoints.rightFloorRight.Count - 1 ] - platformSidesRightVertexPoints.rightFloorRight[ platformSidesRightVertexPoints.rightFloorRight.Count -2 ] ).normalized;

                                            Dictionary<Side, List<Vector3>> startingGrateVertices_BiToMono_MonoToBi = InstantiateGrate( sectionGameObj.transform, "Scambio - Tombino iniziale", new List<Vector3>{ centerToRightFloor.centerR[ 0 ], centerToLeftFloor.centerL[ 0 ] }, new List<Vector3>{ dirLeft0_BiToMono_MonoToBi, dirRight0_BiToMono_MonoToBi }, this.grateWidth, false, this.grateTexture, this.grateTextureTilting );
                                            Dictionary<Side, List<Vector3>> endingGrateVertices_BiToMono_MonoToBi = InstantiateGrate( sectionGameObj.transform, "Scambio - Tombino finale", new List<Vector3>{ centerToRightFloor.centerR[ centerToRightFloor.centerR.Count - 1 ] , centerToLeftFloor.centerL[ centerToLeftFloor.centerL.Count - 1 ] }, new List<Vector3>{ -dirLeft1_BiToMono_MonoToBi, -dirRight1_BiToMono_MonoToBi }, this.grateWidth, true, this.grateTexture, this.grateTextureTilting );

                                            // Generazione mesh terreno
                                            List<Vector3> groundDown_BiToMono_MonoToBi = new List<Vector3>();
                                            List<Vector3> groundUp_BiToMono_MonoToBi = new List<Vector3>();

                                            groundDown_BiToMono_MonoToBi.Add( startingGrateVertices_BiToMono_MonoToBi[ Side.Bottom ][ 0 ] );
                                            groundUp_BiToMono_MonoToBi.Add( startingGrateVertices_BiToMono_MonoToBi[ Side.Bottom ][ 1 ]  );
                                            for( int k = 1; k < centerToRightFloor.centerR.Count - 2; k++ ) {
                                                groundDown_BiToMono_MonoToBi.Add( centerToRightFloor.centerR[ k ] );
                                                groundUp_BiToMono_MonoToBi.Add( centerToLeftFloor.centerL[ k ] );
                                            }
                                            groundUp_BiToMono_MonoToBi.Add( endingGrateVertices_BiToMono_MonoToBi[ Side.Top ][ 1 ] );
                                            groundDown_BiToMono_MonoToBi.Add( endingGrateVertices_BiToMono_MonoToBi[ Side.Top ][ 0 ] );

                                            InstantiatePlane( sectionGameObj.transform, "Scambio - Pavimentazione base", groundUp_BiToMono_MonoToBi, groundUp_BiToMono_MonoToBi, groundDown_BiToMono_MonoToBi, 0.01f, false, false, true, this.switchGroundTexture, this.switchGroundTextureTilting );
                                            
                                            break;

            case SwitchType.BiToBi:         MeshGenerator.Floor leftToLeftFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.leftLine, null, tunnelWidth, false, this.railsWidth );
                                            MeshGenerator.Floor rightToRightFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.rightLine, null, tunnelWidth, false, this.railsWidth );
                                            MeshGenerator.Floor leftToRightFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.leftRightLine, null, tunnelWidth, false, this.railsWidth );
                                            MeshGenerator.Floor rightToLeftFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.rightLeftLine, null, tunnelWidth, false, this.railsWidth );

                                            InstantiatePlane( sectionGameObj.transform, "Scambio - Pavimento binari - Sinistra/Sinistra", section.floorPoints.leftLine, leftToLeftFloor.centerL, leftToLeftFloor.centerR, 0.0f, false, false, false, this.switchRailTexture, this.switchRailTextureTilting );
                                            InstantiatePlane( sectionGameObj.transform, "Scambio - Pavimento binari - Sinistra/Destra", section.floorPoints.rightLine, rightToRightFloor.centerL, rightToRightFloor.centerR, 0.0f, false, false, false, this.switchRailTexture, this.switchRailTextureTilting );
                                            InstantiatePlane( sectionGameObj.transform, "Scambio - Pavimento binari - Destra/Sinistra", section.floorPoints.leftRightLine, leftToRightFloor.centerL, leftToRightFloor.centerR, 0.0f, false, false, false, this.switchRailTexture, this.switchRailTextureTilting );
                                            InstantiatePlane( sectionGameObj.transform, "Scambio - Pavimento binari - Destra/Destra", section.floorPoints.rightLeftLine, rightToLeftFloor.centerL, rightToLeftFloor.centerR, 0.0f, false, false, false, this.switchRailTexture, this.switchRailTextureTilting );

                                            // Generazione mesh extruded binari
                                            InstantiateRail( sectionGameObj.transform, "Scambio - Binario L - Sinistra/Sinistra", section.floorPoints.railLeftL, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, section, SwitchDirection.LeftToLeft );
                                            InstantiateRail( sectionGameObj.transform, "Scambio - Binario R - Sinistra/Sinistra", section.floorPoints.railLeftR, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, section, SwitchDirection.LeftToLeft );
                                            InstantiateRail( sectionGameObj.transform, "Scambio - Binario L - Sinistra/Destra", section.floorPoints.railSwitchLeftRightL, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, section, SwitchDirection.LeftToRight );
                                            InstantiateRail( sectionGameObj.transform, "Scambio - Binario R - Sinistra/Destra", section.floorPoints.railSwitchLeftRightR, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, section, SwitchDirection.LeftToRight );
                                            InstantiateRail( sectionGameObj.transform, "Scambio - Binario L - Destra/Sinistra", section.floorPoints.railSwitchRightLeftL, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, section, SwitchDirection.RightToLeft );
                                            InstantiateRail( sectionGameObj.transform, "Scambio - Binario R - Destra/Sinistra", section.floorPoints.railSwitchRightLeftR, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, section, SwitchDirection.RightToLeft );
                                            InstantiateRail( sectionGameObj.transform, "Scambio - Binario L - Destra/Destra", section.floorPoints.railRightL, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, section, SwitchDirection.RightToRight);
                                            InstantiateRail( sectionGameObj.transform, "Scambio - Binario R - Destra/Destra", section.floorPoints.railRightR, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, section, SwitchDirection.RightToRight);


                                            Vector3 switchDir = ( section.floorPoints.leftLine[ section.floorPoints.leftLine.Count - 1 ] - section.floorPoints.leftLine[ section.floorPoints.leftLine.Count - 2 ] ).normalized;

                                            Dictionary<Side, List<Vector3>> startingGrateVertices = InstantiateGrate( sectionGameObj.transform, "Scambio - Tombino iniziale", new List<Vector3>{ rightToRightFloor.centerR[ 0 ], leftToLeftFloor.centerL[ 0 ] }, new List<Vector3>{ switchDir, switchDir }, this.grateWidth, false, this.grateTexture, this.grateTextureTilting );
                                            Dictionary<Side, List<Vector3>> endingGrateVertices = InstantiateGrate( sectionGameObj.transform, "Scambio - Tombino finale", new List<Vector3>{ rightToRightFloor.centerR[ rightToRightFloor.centerR.Count - 1 ], leftToLeftFloor.centerL[ leftToLeftFloor.centerL.Count - 1 ] }, new List<Vector3>{ -switchDir, -switchDir }, this.grateWidth, true, this.grateTexture, this.grateTextureTilting );

                                            InstantiatePlane( sectionGameObj.transform, "Scambio - Pavimentazione base", new List<Vector3>{ startingGrateVertices[ Side.Bottom ][ 1 ], endingGrateVertices[ Side.Top ][ 1 ] }, new List<Vector3>{ startingGrateVertices[ Side.Bottom ][ 1 ], endingGrateVertices[ Side.Top ][ 1 ] }, new List<Vector3>{ startingGrateVertices[ Side.Bottom ][ 0 ], endingGrateVertices[ Side.Top ][ 0 ] }, 0.01f, false, false, false, this.switchGroundTexture, this.switchGroundTextureTilting );

                                            GenerateTunnelSidePlatformMesh( section, sectionGameObj );
                                            GenerateTunnelWallMesh( section, sectionGameObj );
                                            
                                            break;

            case SwitchType.MonoToNewMono:  MeshGenerator.Floor centerToCenterFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.centerLine, null, tunnelWidth, false, this.railsWidth );
                                            
                                            InstantiatePlane( sectionGameObj.transform, "Scambio - Pavimento binari - Centro/Centro", section.floorPoints.centerLine, centerToCenterFloor.centerL, centerToCenterFloor.centerR, 0.0f, false, false, false, this.switchRailTexture, this.switchRailTextureTilting );

                                            if( section.newLinesStarts.ContainsKey( NewLineSide.Left ) ) {

                                                MeshGenerator.Floor centerToEntranceLeftFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.centerEntranceLeft, null, tunnelWidth, false, this.railsWidth );
                                                MeshGenerator.Floor centerToExitLeftFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.centerExitLeft, null, tunnelWidth, false, this.railsWidth );

                                                InstantiatePlane( sectionGameObj.transform, "Scambio - Pavimento binari - Centro/Ingresso (Sinistra)", section.floorPoints.centerEntranceLeft, centerToEntranceLeftFloor.centerL, centerToEntranceLeftFloor.centerR, 0.0f, false, false, false, this.switchRailTexture, this.switchRailTextureTilting );
                                                InstantiatePlane( sectionGameObj.transform, "Scambio - Pavimento binari - Centro/Uscita (Sinistra)", section.floorPoints.centerExitLeft, centerToExitLeftFloor.centerL, centerToExitLeftFloor.centerR, 0.0f, false, false, false, this.switchRailTexture, this.switchRailTextureTilting );
                                            }

                                            if( section.newLinesStarts.ContainsKey( NewLineSide.Right ) ) {
                                                
                                                MeshGenerator.Floor centerToEntranceRightFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.centerEntranceRight, null, tunnelWidth, false, this.railsWidth );
                                                MeshGenerator.Floor centerToExitRightFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.centerExitRight, null, tunnelWidth, false, this.railsWidth );

                                                InstantiatePlane( sectionGameObj.transform, "Scambio - Pavimento binari - Centro/Ingresso (Destra)", section.floorPoints.centerEntranceRight, centerToEntranceRightFloor.centerL, centerToEntranceRightFloor.centerR, 0.0f, false, false, false, this.switchRailTexture, this.switchRailTextureTilting );
                                                InstantiatePlane( sectionGameObj.transform, "Scambio - Pavimento binari - Centro/Uscita (Destra)", section.floorPoints.centerExitRight, centerToExitRightFloor.centerL, centerToExitRightFloor.centerR, 0.0f, false, false, false, this.switchRailTexture, this.switchRailTextureTilting );
                                            }
                                            
                                            break;

            case SwitchType.BiToNewBi:      MeshGenerator.Floor centerFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.centerLine, null, centerWidth, false, this.railsWidth );
                                            
                                            InstantiatePlane( sectionGameObj.transform, "Scambio - Pavimento Divisore - Centro", section.floorPoints.centerLine, centerFloor.centerL, centerFloor.centerR, 0.0f, false, false, false, this.centerTexture, this.centerTextureTilting );

                                            if( section.newLinesStarts.ContainsKey( NewLineSide.Left ) ) {
                                                
                                                // Generazione mesh planari pavimento binari
                                                MeshGenerator.Floor leftFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.leftLine, null, tunnelWidth, false, this.railsWidth );
                                                MeshGenerator.Floor leftToEntranceLeftFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.leftEntranceLeft, null, tunnelWidth, false, this.railsWidth );
                                                MeshGenerator.Floor leftToExitLeftFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.leftExitLeft, null, tunnelWidth, false, this.railsWidth );

                                                InstantiatePlane( sectionGameObj.transform, "Scambio - Pavimento binari - Sinistra/Sinistra", section.floorPoints.leftLine, leftFloor.centerL, leftFloor.centerR, 0.0f, false, false, false, this.switchRailTexture, this.switchRailTextureTilting );
                                                InstantiatePlane( sectionGameObj.transform, "Scambio - Pavimento binari - Sinistra/Ingresso (Sinistra)", section.floorPoints.leftEntranceLeft, leftToEntranceLeftFloor.centerL, leftToEntranceLeftFloor.centerR, 0.0f, false, false, false, this.switchRailTexture, this.switchRailTextureTilting );
                                                InstantiatePlane( sectionGameObj.transform, "Scambio - Pavimento binari - Sinistra/Uscita (Sinistra)", section.floorPoints.leftExitLeft, leftToExitLeftFloor.centerL, leftToExitLeftFloor.centerR, 0.0f, false, false, false, this.switchRailTexture, this.switchRailTextureTilting );

                                                // Generazione mesh planari tombini
                                                Vector3 dirSwitch = ( section.bezierCurveLimitedAngle[ 1 ] - section.bezierCurveLimitedAngle[ 0 ] ).normalized;
                                                Vector3 dirLeftSwitch = Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * dirSwitch;
                                                Vector3 dirEntranceLeft = ( leftToEntranceLeftFloor.centerL[ 1 ] - leftToEntranceLeftFloor.centerL[ 0 ]).normalized;
                                                Vector3 dirExitLeft = ( leftToExitLeftFloor.centerL[ leftToExitLeftFloor.centerL.Count - 2 ] - leftToExitLeftFloor.centerL[ leftToExitLeftFloor.centerL.Count - 1 ]).normalized;

                                                InstantiateGrate( sectionGameObj.transform, "Scambio - Tombino iniziale (Sinistra)", new List<Vector3>{ centerFloor.centerL[ 0 ], leftToEntranceLeftFloor.centerL[ 0 ] }, new List<Vector3>{ dirSwitch, dirEntranceLeft }, this.grateWidth, false, this.grateTexture, this.grateTextureTilting );
                                                InstantiateGrate( sectionGameObj.transform, "Scambio - Tombino finale (Sinistra)", new List<Vector3>{ centerFloor.centerL[ centerFloor.centerL.Count - 1 ], leftToExitLeftFloor.centerL[ leftToExitLeftFloor.centerR.Count - 1 ] }, new List<Vector3>{ -dirSwitch, dirExitLeft }, this.grateWidth, true, this.grateTexture, this.grateTextureTilting );
                                                InstantiateGrate( sectionGameObj.transform, "Scambio - Tombino nuova linea (Sinistra)", new List<Vector3>{ section.newLinesStarts[ NewLineSide.Left ].pos + dirSwitch * ( ( this.centerWidth / 2 ) + tunnelWidth ), section.newLinesStarts[ NewLineSide.Left ].pos - dirSwitch * ( ( this.centerWidth / 2 ) + tunnelWidth ) }, new List<Vector3>{ leftToExitLeftFloor.centerL[ 1 ] - leftToExitLeftFloor.centerL[ 0 ], leftToEntranceLeftFloor.centerL[ leftToEntranceLeftFloor.centerL.Count - 2 ] - leftToEntranceLeftFloor.centerL[ leftToEntranceLeftFloor.centerL.Count - 1 ] }, this.grateWidth, true, this.grateTexture, this.grateTextureTilting );

                                                // Generazione mesh extruded binari
                                                InstantiateRail( sectionGameObj.transform, "Scambio - Binario L - Sinistra-Sinistra", leftFloor.railCenterL, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, section, SwitchDirection.LeftToLeft );
                                                InstantiateRail( sectionGameObj.transform, "Scambio - Binario R - Sinistra-Sinistra", leftFloor.railCenterR, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, section, SwitchDirection.LeftToLeft );
                                                InstantiateRail( sectionGameObj.transform, "Scambio - Binario L - Sinistra-Ingresso", leftToEntranceLeftFloor.railCenterL, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, section, SwitchDirection.LeftToEntranceLeft );
                                                InstantiateRail( sectionGameObj.transform, "Scambio - Binario R - Sinistra-Ingresso", leftToEntranceLeftFloor.railCenterR, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, section, SwitchDirection.LeftToEntranceLeft );
                                                InstantiateRail( sectionGameObj.transform, "Scambio - Binario L - Sinistra-Uscita", leftToExitLeftFloor.railCenterL, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, section, SwitchDirection.LeftToExitLeft );
                                                InstantiateRail( sectionGameObj.transform, "Scambio - Binario R - Sinistra-Uscita", leftToExitLeftFloor.railCenterR, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, section, SwitchDirection.LeftToExitLeft );

                                                // Generazione mesh piattaforma e muro centrale (sinistra)
                                                MeshGenerator.Floor centralWallsBaselineLeft = GenerateSwitchNewBiCentralBaseLine( leftToEntranceLeftFloor.centerLine, leftToExitLeftFloor.centerLine, leftFloor.centerLine, this.switchCentralWallDistanceCorrection, this.switchCentralWallSkipDown, this.switchCentralWallSkipUp, this.switchCentralWallBezControlPointsDistance, this.switchCentralWallBezierPoints );
                                                MeshGenerator.PlatformSide platformSidesLeftSideVertexPoints = MeshGenerator.CalculatePlatformSidesMeshesVertex( centralWallsBaselineLeft.switchBiNewGroundBaseLine, null, tunnelWidth, tunnelParabolic, platformHeight, platformWidth );
                                                MeshGenerator.SpecularBaseLine wallBaseLinesLeftSide = MeshGenerator.CalculateBaseLinesFromCurve( centralWallsBaselineLeft.switchBiNewGroundBaseLine, null, distanceFromCenter, angleFromCenter );

                                                InstantiatePlane( sectionGameObj.transform, "Scambio - Piattaforma laterale (lato) - Sinistra", wallBaseLinesLeftSide.right, platformSidesLeftSideVertexPoints.rightUp, platformSidesLeftSideVertexPoints.rightDown, 0.0f, true, true, false, this.platformSideTexture, this.platformSideTextureTilting );
                                                InstantiateExtrudedMesh( sectionGameObj.transform, "Scambio - Piattaforma laterale (pavimento) - Sinistra", platformSidesLeftSideVertexPoints.rightFloorLeft, this.platformFloorShape, null, this.platformFloorShapeScale, this.platformFloorHorPosCorrection, 0.0f, 180.0f, this.platformFloorSmoothFactor, false, true, this.platformFloorTexture, this.platformFloorTextureTilting );
                                                Dictionary<int, List<Vector3>> verticesStructure = InstantiateExtrudedMesh( sectionGameObj.transform, "Scambio - Muro - Centrale (Sinistra)", wallBaseLinesLeftSide.right, this.tunnelWallShape, null, this.tunnelWallShapeScale, 0.0f, 0.0f, 180.0f, this.tunnelWallSmoothFactor, false, true, this.tunnelWallTexture, this.tunnelWallTextureTilting ).verticesStructure; 
                                                InstantiateCeiling( sectionGameObj.transform, "Scambio - Soffitto - Centrale (Sinistra)", verticesStructure[ this.tunnelWallShape.Count - 1 ], true, dirSwitch, this.ceilingTexture, this.ceilingTextureTilting );
                                                
                                                // Generazione mesh piattaforme e muri ingresso e uscita (sinistra)
                                                MeshGenerator.PlatformSide platformSidesLeftEntranceVertexPoints = MeshGenerator.CalculatePlatformSidesMeshesVertex( leftToEntranceLeftFloor.centerLine, null, tunnelWidth, tunnelParabolic, platformHeight, platformWidth );
                                                MeshGenerator.PlatformSide platformSidesLeftExitVertexPoints = MeshGenerator.CalculatePlatformSidesMeshesVertex( leftToExitLeftFloor.centerLine, null, tunnelWidth, tunnelParabolic, platformHeight, platformWidth );

                                                InstantiatePlane( sectionGameObj.transform, "Scambio - Piattaforma laterale (lato) - Ingresso Sinistra", leftToEntranceLeftFloor.centerLine, platformSidesLeftEntranceVertexPoints.leftUp, platformSidesLeftEntranceVertexPoints.leftDown, 0.0f, false, false, false, this.platformSideTexture, this.platformSideTextureTilting );
                                                InstantiateExtrudedMesh( sectionGameObj.transform, "Tunnel - Piattaforma laterale (pavimento) - Ingresso Sinistra", platformSidesLeftEntranceVertexPoints.leftFloorRight, this.platformFloorShape, section.previousSection != null ? section.previousSection.sidePlatformFloorLeftLastProfile : null, this.platformFloorShapeScale, this.platformFloorHorPosCorrection, 0.0f, 0.0f, this.platformFloorSmoothFactor, true, false, this.platformFloorTexture, this.platformFloorTextureTilting );

                                                InstantiatePlane( sectionGameObj.transform, "Scambio - Piattaforma laterale (lato) - Uscita Sinistra", leftToExitLeftFloor.centerLine, platformSidesLeftExitVertexPoints.leftUp, platformSidesLeftExitVertexPoints.leftDown, 0.0f, false, false, false, this.platformSideTexture, this.platformSideTextureTilting );
                                                InstantiateExtrudedMesh( sectionGameObj.transform, "Tunnel - Piattaforma laterale (pavimento) - Uscita Sinistra", platformSidesLeftExitVertexPoints.leftFloorRight, this.platformFloorShape, null, this.platformFloorShapeScale, this.platformFloorHorPosCorrection, 0.0f, 0.0f, this.platformFloorSmoothFactor, true, false, this.platformFloorTexture, this.platformFloorTextureTilting );
                                                
                                                MeshGenerator.SpecularBaseLine wallBaseLinesLeftSideEntrance = MeshGenerator.CalculateBaseLinesFromCurve( leftToEntranceLeftFloor.centerLine, null, distanceFromCenter, angleFromCenter );
                                                MeshGenerator.SpecularBaseLine wallBaseLinesLeftSideExit = MeshGenerator.CalculateBaseLinesFromCurve( leftToExitLeftFloor.centerLine, null, distanceFromCenter, angleFromCenter );
                                                
                                                InstantiateExtrudedMesh( sectionGameObj.transform, "Scambio - Muro - Sinistra-Ingresso", wallBaseLinesLeftSideEntrance.left, this.tunnelWallShape, section.previousSection != null ? section.previousSection.wallLeftLastProfile : null, this.tunnelWallShapeScale, 0.0f, 0.0f, 0.0f, this.tunnelWallSmoothFactor, true, false, this.tunnelWallTexture, this.tunnelWallTextureTilting );
                                                section.wallLeftLastProfile = InstantiateExtrudedMesh( sectionGameObj.transform, "Scambio - Muro - Sinistra-Uscita", wallBaseLinesLeftSideExit.left, this.tunnelWallShape, null, this.tunnelWallShapeScale, 0.0f, 0.0f, 0.0f, this.tunnelWallSmoothFactor, true, false, this.tunnelWallTexture, this.tunnelWallTextureTilting ).lastProfileVertex;

                                                // Generazione mesh planare terreno (sinistra)
                                                GameObject switch_BiNew_Ground_Left_GameObj = new GameObject( "Scambio - Terreno (Sinistra)" );
                                                switch_BiNew_Ground_Left_GameObj.transform.parent = sectionGameObj.transform;
                                                switch_BiNew_Ground_Left_GameObj.transform.position = new Vector3( 0.0f, 0.0f, 0.01f );
                                                switch_BiNew_Ground_Left_GameObj.AddComponent<MeshFilter>();
                                                switch_BiNew_Ground_Left_GameObj.AddComponent<MeshRenderer>();
                                                switch_BiNew_Ground_Left_GameObj.GetComponent<MeshFilter>().sharedMesh = MeshGenerator.GenerateSwitchNewBiGround( NewLineSide.Left, platformSidesLeftEntranceVertexPoints.leftDown, platformSidesLeftExitVertexPoints.leftDown, this.switchCentralWallSkipDown, this.switchCentralWallSkipUp, platformSidesLeftSideVertexPoints.rightDown, 1, this.switchCentralWallBezierPoints, section.nextStartingDirections[ 0 ], true, this.switchGroundTextureTilting, this.tunnelWidth, this.centerWidth );
                                                switch_BiNew_Ground_Left_GameObj.GetComponent<MeshRenderer>().material = this.switchGroundTexture; 

                                                // //section.sidePlatformFloorLeftLastProfile = platformFloorLeftMesh.lastProfileVertex;
                                                // //section.sidePlatformFloorRightLastProfile = platformFloorRightMesh.lastProfileVertex;

                                                // List<Vector3> groundBiNewUp = new List<Vector3>();
                                                // groundBiNewUp.Add( grate0LeftDown[ 0 ] );
                                                // groundBiNewUp.Add( grate0LeftDown[ 1 ] );
                                                // for( int k = 1; k < leftToEntranceLeftFloor.centerL.Count - 1; k++ ) {
                                                //     groundBiNewUp.Add( leftToEntranceLeftFloor.centerL[ k ] );
                                                //     //Debug.DrawLine( groundBiNewDown[ k ], groundBiNewDown[ k - 1 ], Color.magenta, 9999 );
                                                // }
                                                // groundBiNewUp.Add( grate2LeftUp[ 1 ] );
                                                // //Debug.DrawLine( groundBiNewDown[ groundBiNewDown.Count - 1 ], groundBiNewDown[ groundBiNewDown.Count - 2 ], Color.magenta, 9999 );

                                                // List<Vector3> groundBiNewDown = new List<Vector3>();
                                                // groundBiNewDown.Add( grate1LeftUp[ 0 ] );
                                                // groundBiNewDown.Add( grate1LeftUp[ 1 ] );
                                                // for( int k = leftToExitLeftFloor.centerL.Count - 2; k > 0; k-- ) {
                                                //     groundBiNewDown.Add( leftToExitLeftFloor.centerL[ k ] );

                                                //     //Debug.DrawLine( groundBiNewUp[ leftToExitLeftFloor.centerL.Count - 2 - k ], groundBiNewUp[ leftToExitLeftFloor.centerL.Count - 2 - k + 1 ], Color.cyan, 9999 );
                                                // }
                                                // groundBiNewDown.Add( grate2LeftUp[ 0 ] );
                                                // //Debug.DrawLine( groundBiNewUp[ groundBiNewUp.Count - 1 ], groundBiNewUp[ groundBiNewUp.Count - 2 ], Color.cyan, 9999 );
                                                // Mesh groundBiNewMesh = MeshGenerator.GeneratePlanarMesh( groundBiNewDown, MeshGenerator.ConvertListsToMatrix_2xM( groundBiNewUp, groundBiNewDown ), false, false, this.switchGroundTextureTilting.x, this.switchGroundTextureTilting.y );
                                                // GameObject groundBiNewGameObj = new GameObject( "Pavimentazione base scambio sinistra" );
                                                // groundBiNewGameObj.transform.parent = sectionGameObj.transform;
                                                // groundBiNewGameObj.transform.position = new Vector3( 0.0f, 0.0f, 0.01f );
                                                // groundBiNewGameObj.AddComponent<MeshFilter>();
                                                // groundBiNewGameObj.AddComponent<MeshRenderer>();
                                                // groundBiNewGameObj.GetComponent<MeshFilter>().sharedMesh = groundBiNewMesh;
                                                // groundBiNewGameObj.GetComponent<MeshRenderer>().material = this.switchGroundTexture;

                                            }
                                            else {
                                                MeshGenerator.Floor leftFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.leftLine, null, tunnelWidth, false, this.railsWidth );
                                                
                                                InstantiatePlane( sectionGameObj.transform, "Scambio - Pavimento binari - Sinistra/Sinistra", section.floorPoints.leftLine, leftFloor.centerL, leftFloor.centerR, 0.0f, false, false, false, this.tunnelRailTexture, this.tunnelRailTextureTilting );
                                            
                                                InstantiateRail( sectionGameObj.transform, "Tunnel - Binario L - Sinistra/Sinistra", leftFloor.railCenterL, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, null, null );
                                                InstantiateRail( sectionGameObj.transform, "Tunnel - Binario R - Sinistra/Sinistra", leftFloor.railCenterR, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, null, null );
                                            
                                                MeshGenerator.PlatformSide platformSidesSwitchLeftVertexPoints = MeshGenerator.CalculatePlatformSidesMeshesVertex( section.floorPoints.leftLine, null, tunnelWidth, tunnelParabolic, platformHeight, platformWidth );

                                                InstantiatePlane( sectionGameObj.transform, "Scambio - Piattaforma laterale (lato) - Sinistra", section.floorPoints.leftLine, platformSidesSwitchLeftVertexPoints.leftDown, platformSidesSwitchLeftVertexPoints.leftUp, 0.0f, true, false, false, this.platformSideTexture, this.platformSideTextureTilting );
                                                InstantiateExtrudedMesh( sectionGameObj.transform, "Scambio - Piattaforma laterale (pavimento) - Sinistra", platformSidesSwitchLeftVertexPoints.leftFloorRight, this.platformFloorShape, section.previousSection != null ? section.previousSection.sidePlatformFloorLeftLastProfile : null, this.platformFloorShapeScale, this.platformFloorHorPosCorrection, 0.0f, 0.0f, this.platformFloorSmoothFactor, true, false, this.platformFloorTexture, this.platformFloorTextureTilting );

                                                MeshGenerator.SpecularBaseLine wallBaseLinesSwitchLeft = MeshGenerator.CalculateBaseLinesFromCurve( section.floorPoints.leftLine, null, distanceFromCenter, angleFromCenter );

                                                section.wallLeftLastProfile = InstantiateExtrudedMesh( sectionGameObj.transform, "Scambio - Muro - Sinistra", wallBaseLinesSwitchLeft.left, this.tunnelWallShape, section.previousSection != null ? section.previousSection.wallLeftLastProfile : null, this.tunnelWallShapeScale, 0.0f, 0.0f, 0.0f, this.tunnelWallSmoothFactor, true, false, this.tunnelWallTexture, this.tunnelWallTextureTilting ).lastProfileVertex;
                                            }

                                            if( section.newLinesStarts.ContainsKey( NewLineSide.Right ) ) {

                                                // Generazione mesh planari pavimento binari
                                                MeshGenerator.Floor rightFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.rightLine, null, tunnelWidth, false, this.railsWidth );
                                                MeshGenerator.Floor rightToEntranceRightFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.rightEntranceRight, null, tunnelWidth, false, this.railsWidth );
                                                MeshGenerator.Floor rightToExitRightFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.rightExitRight, null, tunnelWidth, false, this.railsWidth );

                                                InstantiatePlane( sectionGameObj.transform, "Scambio - Pavimento binari - Destra/Destra", section.floorPoints.leftLine, rightFloor.centerL, rightFloor.centerR, 0.0f, false, false, false, this.switchRailTexture, this.switchRailTextureTilting );
                                                InstantiatePlane( sectionGameObj.transform, "Scambio - Pavimento binari - Destra/Ingresso", section.floorPoints.rightEntranceRight, rightToEntranceRightFloor.centerL, rightToEntranceRightFloor.centerR, 0.0f, false, false, false, this.switchRailTexture, this.switchRailTextureTilting );
                                                InstantiatePlane( sectionGameObj.transform, "Scambio - Pavimento binari - Destra/Uscita", section.floorPoints.rightExitRight, rightToExitRightFloor.centerL, rightToExitRightFloor.centerR, 0.0f, false, false, false, this.switchRailTexture, this.switchRailTextureTilting );
                                            
                                                // Generazione mesh planari tombini
                                                Vector3 dirSwitch = ( section.bezierCurveLimitedAngle[ 1 ] - section.bezierCurveLimitedAngle[ 0 ] ).normalized;
                                                Vector3 dirRightSwitch = Quaternion.Euler( 0.0f, 0.0f, 90.0f) * dirSwitch;
                                                Vector3 dirEntranceRight = ( rightToEntranceRightFloor.centerR[ 1 ] - rightToEntranceRightFloor.centerR[ 0 ]).normalized;
                                                Vector3 dirExitRight = ( rightToExitRightFloor.centerR[ rightToExitRightFloor.centerR.Count - 2 ] - rightToExitRightFloor.centerR[ rightToExitRightFloor.centerR.Count - 1 ]).normalized;

                                                InstantiateGrate( sectionGameObj.transform, "Scambio - Tombino iniziale (Destra)", new List<Vector3>{ centerFloor.centerR[ 0 ], rightToEntranceRightFloor.centerR[ 0 ] }, new List<Vector3>{ dirSwitch, dirEntranceRight }, this.grateWidth, true, this.grateTexture, this.grateTextureTilting );
                                                InstantiateGrate( sectionGameObj.transform, "Scambio - Tombino finale (Destra)", new List<Vector3>{ centerFloor.centerR[ centerFloor.centerR.Count - 1 ], rightToExitRightFloor.centerR[ rightToExitRightFloor.centerR.Count - 1 ] }, new List<Vector3>{ -dirSwitch, dirExitRight }, this.grateWidth, false, this.grateTexture, this.grateTextureTilting );
                                                InstantiateGrate( sectionGameObj.transform, "Scambio - Tombino nuova linea (Destra)", new List<Vector3>{ section.newLinesStarts[ NewLineSide.Right ].pos + dirSwitch * ( ( this.centerWidth / 2 ) + tunnelWidth ), section.newLinesStarts[ NewLineSide.Right ].pos - dirSwitch * ( ( this.centerWidth / 2 ) + tunnelWidth ) }, new List<Vector3>{ rightToExitRightFloor.centerR[ 1 ] - rightToExitRightFloor.centerR[ 0 ], rightToEntranceRightFloor.centerR[ rightToEntranceRightFloor.centerR.Count - 2 ] - rightToEntranceRightFloor.centerR[ rightToEntranceRightFloor.centerR.Count - 1 ] }, this.grateWidth, false, this.grateTexture, this.grateTextureTilting );
                                            
                                                // Generazione mesh extruded binari
                                                InstantiateRail( sectionGameObj.transform, "Tunnel - Binario L - Destra-Destra", rightFloor.railCenterL, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, section, SwitchDirection.RightToRight );
                                                InstantiateRail( sectionGameObj.transform, "Tunnel - Binario R - Destra-Destra", rightFloor.railCenterR, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, section, SwitchDirection.RightToRight );
                                                InstantiateRail( sectionGameObj.transform, "Tunnel - Binario L - Destra-Ingresso", rightToEntranceRightFloor.railCenterL, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, section, SwitchDirection.RightToEntranceRight );
                                                InstantiateRail( sectionGameObj.transform, "Tunnel - Binario R - Destra-Ingresso", rightToEntranceRightFloor.railCenterR, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, section, SwitchDirection.RightToEntranceRight );
                                                InstantiateRail( sectionGameObj.transform, "Tunnel - Binario L - Destra-Uscita", rightToExitRightFloor.railCenterL, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, section, SwitchDirection.RightToExitRight );
                                                InstantiateRail( sectionGameObj.transform, "Tunnel - Binario R - Destra-Uscita", rightToExitRightFloor.railCenterR, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, section, SwitchDirection.RightToExitRight );
                                            
                                                // Generazione mesh piattaforma e muro centrale (destra)
                                                MeshGenerator.Floor centralWallsBaselineRight = GenerateSwitchNewBiCentralBaseLine( rightToEntranceRightFloor.centerLine, rightToExitRightFloor.centerLine, rightFloor.centerLine, this.switchCentralWallDistanceCorrection, this.switchCentralWallSkipDown, this.switchCentralWallSkipUp, this.switchCentralWallBezControlPointsDistance, this.switchCentralWallBezierPoints );
                                                MeshGenerator.PlatformSide platformSidesRightSideVertexPoints = MeshGenerator.CalculatePlatformSidesMeshesVertex( centralWallsBaselineRight.switchBiNewGroundBaseLine, null, tunnelWidth, tunnelParabolic, platformHeight, platformWidth );
                                                MeshGenerator.SpecularBaseLine wallBaseLinesRightSide = MeshGenerator.CalculateBaseLinesFromCurve( centralWallsBaselineRight.switchBiNewGroundBaseLine, null, distanceFromCenter, angleFromCenter );

                                                InstantiatePlane( sectionGameObj.transform, "Scambio - Piattaforma laterale (lato) - Destra", platformSidesRightSideVertexPoints.leftFloorRight, platformSidesRightSideVertexPoints.leftUp, platformSidesRightSideVertexPoints.leftDown, 0.0f, false, true, false, this.platformSideTexture, this.platformSideTextureTilting );
                                                InstantiateExtrudedMesh( sectionGameObj.transform, "Tunnel - Piattaforma laterale (pavimento) - Destra", platformSidesRightSideVertexPoints.leftFloorRight, this.platformFloorShape, null, this.platformFloorShapeScale, this.platformFloorHorPosCorrection, 0.0f, 0.0f, this.platformFloorSmoothFactor, true, true, this.platformFloorTexture, this.platformFloorTextureTilting );
                                                Dictionary<int, List<Vector3>> verticesStructure = InstantiateExtrudedMesh( sectionGameObj.transform, "Scambio - Muro - Centrale (Destra)", wallBaseLinesRightSide.left, this.tunnelWallShape, null, this.tunnelWallShapeScale, 0.0f, 0.0f, 0.0f, this.tunnelWallSmoothFactor, true, true, this.tunnelWallTexture, this.tunnelWallTextureTilting ).verticesStructure;
                                                InstantiateCeiling( sectionGameObj.transform, "Scambio - Soffitto - Centrale (Destra)", verticesStructure[ this.tunnelWallShape.Count - 1 ], false, dirSwitch, this.ceilingTexture, this.ceilingTextureTilting );

                                                // Generazione mesh piattaforme e muri ingresso e uscita (sinistra)
                                                MeshGenerator.PlatformSide platformSidesRightEntranceVertexPoints = MeshGenerator.CalculatePlatformSidesMeshesVertex( rightToEntranceRightFloor.centerLine, null, tunnelWidth, tunnelParabolic, platformHeight, platformWidth );
                                                MeshGenerator.PlatformSide platformSidesRightExitVertexPoints = MeshGenerator.CalculatePlatformSidesMeshesVertex( rightToExitRightFloor.centerLine, null, tunnelWidth, tunnelParabolic, platformHeight, platformWidth );

                                                InstantiatePlane( sectionGameObj.transform, "Scambio - Piattaforma laterale (lato) - Ingresso Destra", rightToEntranceRightFloor.centerLine, platformSidesRightEntranceVertexPoints.rightUp, platformSidesRightEntranceVertexPoints.rightDown, 0.0f, true, false, false, this.platformSideTexture, this.platformSideTextureTilting );
                                                InstantiateExtrudedMesh( sectionGameObj.transform, "Tunnel - Piattaforma laterale (pavimento) - Ingresso Destra", platformSidesRightEntranceVertexPoints.rightFloorLeft, this.platformFloorShape, section.previousSection != null ? section.previousSection.sidePlatformFloorRightLastProfile : null, this.platformFloorShapeScale, this.platformFloorHorPosCorrection, 0.0f, 180.0f, this.platformFloorSmoothFactor, false, false, this.platformFloorTexture, this.platformFloorTextureTilting );

                                                InstantiatePlane( sectionGameObj.transform, "Scambio - Piattaforma laterale (lato) - Uscita Sinistra", rightToExitRightFloor.centerLine, platformSidesRightExitVertexPoints.rightUp, platformSidesRightExitVertexPoints.rightDown, 0.0f, true, false, false, this.platformSideTexture, this.platformSideTextureTilting );
                                                InstantiateExtrudedMesh( sectionGameObj.transform, "Scambio - Piattaforma laterale (pavimento) - Uscita Destra", platformSidesRightExitVertexPoints.rightFloorLeft, this.platformFloorShape, null, this.platformFloorShapeScale, this.platformFloorHorPosCorrection, 0.0f, 180.0f, this.platformFloorSmoothFactor, false, false, this.platformFloorTexture, this.platformFloorTextureTilting );
                                                
                                                MeshGenerator.SpecularBaseLine wallBaseLinesRightSideEntrance = MeshGenerator.CalculateBaseLinesFromCurve( rightToEntranceRightFloor.centerLine, null, distanceFromCenter, angleFromCenter );
                                                MeshGenerator.SpecularBaseLine wallBaseLinesRightSideExit = MeshGenerator.CalculateBaseLinesFromCurve( rightToExitRightFloor.centerLine, null, distanceFromCenter, angleFromCenter );
                                                
                                                InstantiateExtrudedMesh( sectionGameObj.transform, "Scambio - Muro - Destra-Ingresso", wallBaseLinesRightSideEntrance.right, this.tunnelWallShape, section.previousSection != null ? section.previousSection.wallRightLastProfile : null, this.tunnelWallShapeScale, 0.0f, 0.0f, 180.0f, this.tunnelWallSmoothFactor, false, false, this.tunnelWallTexture, this.tunnelWallTextureTilting );
                                                section.wallRightLastProfile = InstantiateExtrudedMesh( sectionGameObj.transform, "Scambio - Muro - Destra-Uscita", wallBaseLinesRightSideExit.right, this.tunnelWallShape, null, this.tunnelWallShapeScale, 0.0f, 0.0f, 180.0f, this.tunnelWallSmoothFactor, false, false, this.tunnelWallTexture, this.tunnelWallTextureTilting ).lastProfileVertex;
                                            
                                                // Generazione mesh planare terreno (sinistra)
                                                GameObject switch_BiNew_Ground_Right_GameObj = new GameObject( "Scambio - Terreno (Destra)" );
                                                switch_BiNew_Ground_Right_GameObj.transform.parent = sectionGameObj.transform;
                                                switch_BiNew_Ground_Right_GameObj.transform.position = new Vector3( 0.0f, 0.0f, 0.01f );
                                                switch_BiNew_Ground_Right_GameObj.AddComponent<MeshFilter>();
                                                switch_BiNew_Ground_Right_GameObj.AddComponent<MeshRenderer>();
                                                switch_BiNew_Ground_Right_GameObj.GetComponent<MeshFilter>().sharedMesh = MeshGenerator.GenerateSwitchNewBiGround( NewLineSide.Right, platformSidesRightEntranceVertexPoints.rightDown, platformSidesRightExitVertexPoints.rightDown, this.switchCentralWallSkipDown, this.switchCentralWallSkipUp, platformSidesRightSideVertexPoints.leftDown, 1, this.switchCentralWallBezierPoints, section.nextStartingDirections[ 0 ], false, this.switchGroundTextureTilting, this.tunnelWidth, this.centerWidth );
                                                switch_BiNew_Ground_Right_GameObj.GetComponent<MeshRenderer>().material = this.switchGroundTexture; 
                                            }
                                            else {
                                                MeshGenerator.Floor rightFloor = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( section.floorPoints.rightLine, null, tunnelWidth, false, this.railsWidth );

                                                InstantiatePlane( sectionGameObj.transform, "Scambio - Pavimento binari - Destra/Destra", section.floorPoints.leftLine, rightFloor.centerL, rightFloor.centerR, 0.0f, false, false, false, this.tunnelRailTexture, this.tunnelRailTextureTilting );
                                            
                                                InstantiateRail( sectionGameObj.transform, "Scambio - Binario L - Destra/Destra", rightFloor.railCenterL, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, null, null );
                                                InstantiateRail( sectionGameObj.transform, "Scambio - Binario R - Destra/Destra", rightFloor.railCenterR, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, null, null );

                                                MeshGenerator.PlatformSide platformSidesSwitchRightVertexPoints = MeshGenerator.CalculatePlatformSidesMeshesVertex( section.floorPoints.rightLine, null, tunnelWidth, tunnelParabolic, platformHeight, platformWidth );

                                                InstantiatePlane( sectionGameObj.transform, "Scambio - Piattaforma laterale (lato) - Destra", section.floorPoints.rightLine, platformSidesSwitchRightVertexPoints.rightDown, platformSidesSwitchRightVertexPoints.rightUp, 0.0f, false, false, false, this.platformSideTexture, this.platformSideTextureTilting );
                                                InstantiateExtrudedMesh( sectionGameObj.transform, "Scambio - Piattaforma laterale (pavimento) - Destra", platformSidesSwitchRightVertexPoints.rightFloorLeft, this.platformFloorShape, section.previousSection != null ? section.previousSection.sidePlatformFloorRightLastProfile : null, this.platformFloorShapeScale, this.platformFloorHorPosCorrection, 0.0f, 180.0f, this.platformFloorSmoothFactor, false, false, this.platformFloorTexture, this.platformFloorTextureTilting );

                                                MeshGenerator.SpecularBaseLine wallBaseLinesSwitchRight = MeshGenerator.CalculateBaseLinesFromCurve( section.floorPoints.rightLine, null, distanceFromCenter, angleFromCenter );

                                                section.wallRightLastProfile = InstantiateExtrudedMesh( sectionGameObj.transform, "Scambio - Muro - Destra", wallBaseLinesSwitchRight.right, this.tunnelWallShape, section.previousSection != null ? section.previousSection.wallRightLastProfile : null, this.tunnelWallShapeScale, 0.0f, 0.0f, 180.0f, this.tunnelWallSmoothFactor, false, false, this.tunnelWallTexture, this.tunnelWallTextureTilting ).lastProfileVertex;
                                            
                                            }
                                            
                                            break;

            
        }
        
        GameObject indicatorObj = GameObject.CreatePrimitive( PrimitiveType.Cube );
        indicatorObj.name = "Indicatore Scambio";
        indicatorObj.transform.position = section.centerCoords;
        indicatorObj.transform.parent = sectionGameObj.transform;
        indicatorObj.AddComponent<DynamicIcons>();

        DynamicIcons indicatorScript = indicatorObj.GetComponent<DynamicIcons>();
        indicatorScript.ICON_Texture2D = Resources.Load( "Indicatori/Texture2D/Wrong_Way" ) as Texture2D;
        indicatorScript.ICON_Color = Color.red;
        indicatorScript.ICON_Fade = true;
        indicatorScript.ICON_Rotation = false;
        indicatorScript.TARGET_MaxVisibleDistance = 2000.0f;
        indicatorScript.TARGET_MinVisibleDistanceOnScreen = 500.0f;
        indicatorScript.ICON_Scale = false;
        indicatorScript.ICON_Visibility = Icon.Show.Always;
        indicatorScript.ICON_MaxSize = 0.8f;
        indicatorScript.ICON_Active = true;
        indicatorScript.ICON_RotationCorrection = 270.0f;
        indicatorScript.ICON_DistanceFromBorderInPixels = Mathf.Sqrt( Mathf.Pow( Screen.width, 2 ) + Mathf.Pow( Screen.height, 2 ) ) * 0.025f;

        section.indicatorObj = indicatorObj;
    }

    private void GenerateTunnelSidePlatformMesh( LineSection section, GameObject sectionGameObj ) {

        float sectionWidth = section.bidirectional ? ( ( tunnelWidth * 2 ) + centerWidth ) : tunnelWidth;
        Vector3 startingDir = section.controlsPoints[ 1 ] - section.bezierCurveLimitedAngle[ 0 ];
        MeshGenerator.PlatformSide platformSidesVertexPoints = MeshGenerator.CalculatePlatformSidesMeshesVertex( section.bezierCurveLimitedAngle, startingDir, sectionWidth, tunnelParabolic, platformHeight, platformWidth );

        InstantiatePlane( sectionGameObj.transform, "Tunnel - Piattaforma laterale (lato) - Sinistra", section.bezierCurveLimitedAngle, platformSidesVertexPoints.leftUp, platformSidesVertexPoints.leftDown, 0.0f, false, false, false, this.platformSideTexture, this.platformSideTextureTilting );
        InstantiatePlane( sectionGameObj.transform, "Tunnel - Piattaforma laterale (lato) - Destra", section.bezierCurveLimitedAngle, platformSidesVertexPoints.rightUp, platformSidesVertexPoints.rightDown, 0.0f, true, false, false, this.platformSideTexture, this.platformSideTextureTilting );

        section.sidePlatformFloorLeftLastProfile = InstantiateExtrudedMesh( sectionGameObj.transform, "Tunnel - Piattaforma laterale (pavimento) - Sinistra", platformSidesVertexPoints.leftFloorRight, this.platformFloorShape, section.previousSection != null ? section.previousSection.sidePlatformFloorLeftLastProfile : null, this.platformFloorShapeScale, this.platformFloorHorPosCorrection, 0.0f, 0.0f, this.platformFloorSmoothFactor, true, false, this.platformFloorTexture, this.platformFloorTextureTilting).lastProfileVertex;
        section.sidePlatformFloorRightLastProfile = InstantiateExtrudedMesh( sectionGameObj.transform, "Tunnel - Piattaforma laterale (pavimento) - Destra", platformSidesVertexPoints.rightFloorLeft, this.platformFloorShape, section.previousSection != null ? section.previousSection.sidePlatformFloorRightLastProfile : null, this.platformFloorShapeScale, this.platformFloorHorPosCorrection, 0.0f, 180.0f, this.platformFloorSmoothFactor, false, false, this.platformFloorTexture, this.platformFloorTextureTilting).lastProfileVertex;

        section.platformSidesPoints = platformSidesVertexPoints;
    }

    private void GenerateTunnelWallMesh( LineSection section, GameObject sectionGameObj ) {

        float baseWidth = section.bidirectional ? ( this.tunnelWidth + ( this.centerWidth / 2 ) + this.platformWidth ) : ( ( this.tunnelWidth / 2 ) + this.platformWidth );

        float angleFromCenter = Mathf.Atan( this.platformHeight / baseWidth ) * Mathf.Rad2Deg;
        float distanceFromCenter = Mathf.Sqrt( Mathf.Pow( this.platformHeight, 2 ) + Mathf.Pow( baseWidth, 2 ) );

        Vector3? lastDir = section.previousSection == null ? null : ( Vector3? )( section.previousSection.bezierCurveLimitedAngle[ section.previousSection.bezierCurveLimitedAngle.Count - 1 ] - section.previousSection.bezierCurveLimitedAngle[ section.previousSection.bezierCurveLimitedAngle.Count - 2 ] );
        MeshGenerator.SpecularBaseLine wallBaseLines = MeshGenerator.CalculateBaseLinesFromCurve( section.bezierCurveLimitedAngle, lastDir, distanceFromCenter, angleFromCenter );

        section.wallLeftLastProfile = InstantiateExtrudedMesh( sectionGameObj.transform, "Tunnel - Muro - Sinistra", wallBaseLines.left, this.tunnelWallShape, section.previousSection != null ? section.previousSection.wallLeftLastProfile : null, this.tunnelWallShapeScale, 0.0f, 0.0f, 0.0f, this.tunnelWallSmoothFactor, true, false, this.tunnelWallTexture, this.tunnelWallTextureTilting).lastProfileVertex;
        section.wallRightLastProfile = InstantiateExtrudedMesh( sectionGameObj.transform, "Tunnel - Muro - Destra", wallBaseLines.right, this.tunnelWallShape, section.previousSection != null ? section.previousSection.wallRightLastProfile : null, this.tunnelWallShapeScale, 0.0f, 0.0f, 180.0f, this.tunnelWallSmoothFactor, false, false, this.tunnelWallTexture, this.tunnelWallTextureTilting).lastProfileVertex;

        List<Vector3> wireShape = MeshGenerator.CalculateCircularShape( 1.0f, 10, Vector3.zero, 0.0f );
        InstantiateWallWires( sectionGameObj.transform, "Filo Muro - Sinistra", Random.Range( this.wallWiresMinNumber, this.wallWiresMaxNumber + 1 ), this.wallWiresBezierPoints, /*this.wallWiresFuseBox,*/ wallBaseLines.left, wireShape, this.wallWireShapeScale, 0.0f, this.wallWireHorPosCorrection, this.wallWireVertPosCorrection, this.wallWireSmoothFactor, this.wallWireMinLenght, this.wallWireMaxLenght, this.wallWireFloating, true, false, this.wireTexture, this.wireTextureTilting );
        InstantiateWallWires( sectionGameObj.transform, "Filo Muro - Destra", Random.Range( this.wallWiresMinNumber, this.wallWiresMaxNumber + 1 ), this.wallWiresBezierPoints, /*this.wallWiresFuseBox,*/ wallBaseLines.right, wireShape, this.wallWireShapeScale, 180.0f, this.wallWireHorPosCorrection, this.wallWireVertPosCorrection, this.wallWireSmoothFactor, this.wallWireMinLenght, this.wallWireMaxLenght, this.wallWireFloating, false, false, this.wireTexture, this.wireTextureTilting );

        // Update dettagli LineSection 
        //section.platformSidesPoints = platformSidesVertexPoints;
    }

    private void GenerateTunnelTubesMesh( LineSection section, GameObject sectionGameObj ) {
        MeshGenerator.SpecularBaseLine tubesBaseLines = new MeshGenerator.SpecularBaseLine();

        MeshGenerator.ExtrudedMesh tubeLeftMesh = new MeshGenerator.ExtrudedMesh();
        MeshGenerator.ExtrudedMesh tubeRightMesh = new MeshGenerator.ExtrudedMesh();

        float baseWidth = section.bidirectional ? ( this.tubeDistanceCorrection + this.tunnelWidth + ( this.centerWidth / 2 ) + this.platformWidth ) : ( this.tubeDistanceCorrection + ( this.tunnelWidth / 2 ) + this.platformWidth );

        float angleFromCenter = Mathf.Atan( ( this.platformHeight + this.tubeHeightFromPlatform ) / baseWidth ) * Mathf.Rad2Deg;
        float distanceFromCenter = Mathf.Sqrt( Mathf.Pow( this.platformHeight + this.tubeHeightFromPlatform, 2 ) + Mathf.Pow( baseWidth, 2 ) );

        List<Vector3> tubeShape = MeshGenerator.CalculateCircularShape( this.tubeShapeRadius, this.tubeShapePoints, Vector3.zero, this.tubeShapeEccentricity );

        Vector3? lastDir = section.previousSection == null ? null : ( Vector3? )( section.previousSection.bezierCurveLimitedAngle[ section.previousSection.bezierCurveLimitedAngle.Count - 1 ] - section.previousSection.bezierCurveLimitedAngle[ section.previousSection.bezierCurveLimitedAngle.Count - 2 ] );
        tubesBaseLines = MeshGenerator.CalculateBaseLinesFromCurve( section.bezierCurveLimitedAngle, lastDir, distanceFromCenter, angleFromCenter );

        tubeLeftMesh = MeshGenerator.GenerateExtrudedMesh( tubeShape, this.tubeShapeScale, null, tubesBaseLines.left, this.tubeHorPosCorrection, 0.0f, true, false, this.tubeTextureTilting.x, this.tubeTextureTilting.y, 0.0f, this.tubeSmoothFactor );
        tubeRightMesh = MeshGenerator.GenerateExtrudedMesh( tubeShape, this.tubeShapeScale, null, tubesBaseLines.right, this.tubeHorPosCorrection, 0.0f, false, false, this.tubeTextureTilting.x, this.tubeTextureTilting.y, 180.0f, this.tubeSmoothFactor );

        GameObject tubeLeftGameObj = new GameObject( "Tubo sinistro" );
        tubeLeftGameObj.transform.parent = sectionGameObj.transform;
        tubeLeftGameObj.transform.position = Vector3.zero;
        tubeLeftGameObj.AddComponent<MeshFilter>();
        tubeLeftGameObj.GetComponent<MeshFilter>().sharedMesh = tubeLeftMesh.mesh;
        tubeLeftGameObj.AddComponent<FadingInOutCoroutines>();
        tubeLeftGameObj.AddComponent<MeshCollider>();
        tubeLeftGameObj.GetComponent<MeshCollider>().sharedMesh = tubeLeftMesh.mesh;
        tubeLeftGameObj.GetComponent<MeshCollider>().convex = false;
        tubeLeftGameObj.AddComponent<MeshRenderer>();
        tubeLeftGameObj.GetComponent<MeshRenderer>().material = this.tubeTexture;

        GameObject tubeRightGameObj = new GameObject( "Tubo destro" );
        tubeRightGameObj.transform.parent = sectionGameObj.transform;
        tubeRightGameObj.transform.position = Vector3.zero;
        tubeRightGameObj.AddComponent<MeshFilter>();
        tubeRightGameObj.GetComponent<MeshFilter>().sharedMesh = tubeRightMesh.mesh;
        tubeRightGameObj.AddComponent<FadingInOutCoroutines>();
        tubeRightGameObj.AddComponent<MeshCollider>();
        tubeRightGameObj.GetComponent<MeshCollider>().sharedMesh = tubeRightMesh.mesh;
        tubeRightGameObj.GetComponent<MeshCollider>().convex = false;
        tubeRightGameObj.AddComponent<MeshRenderer>();
        tubeRightGameObj.GetComponent<MeshRenderer>().material = this.tubeTexture;


        //section.wallLeftLastProfile = wallLeftMesh.lastProfileVertex;
        //section.wallRightLastProfile = wallRightMesh.lastProfileVertex;
    }

    private void AddProps() {
        foreach( string lineName in this.lines.Keys ) {
            
            GameObject lineGameObj = new GameObject( lineName );
            Vector2 banisterOffsets = Vector2.zero;
            float pillarOffset = 0.0f;
            float fanOffset = 0.0f;
            for( int i = 0; i < this.lines[ lineName ].Count; i++ ) { 
                
                LineSection section = this.lines[ lineName ][ i ];
                
                banisterOffsets = AddSidePlatformBanisters( section, banisterOffsets, this.banisterMinDistance, this.banisterMaxDistance, this.banisterRotationCorrection, this.banisterPositionCorrectionLeft, this.banisterPositionCorrectionRight );
                pillarOffset = AddPillars( section, pillarOffset, this.pillarMinDistance, this.pillarMaxDistance, this.pillarRotationCorrection, this.pillarPositionCorrection );
                fanOffset = AddCeilingFans( section, fanOffset, this.fanMinDistance, this.fanMaxDistance, ( this.centerWidth + this.tunnelWidth ) / 2 , this.fanRotationCorrection, this.fanPositionCorrection );
            }
        }
    }

    private float AddPillars( LineSection section, float previousOffset, float minDistance, float maxDistance, Vector3 rotationCorrection, Vector3 positionCorrection ) {

        float distance = Random.Range( minDistance, maxDistance );

        float offset = previousOffset;

        if( ( section.type == Type.Tunnel && section.bidirectional ) || ( section.type == Type.Switch && section.switchType == SwitchType.BiToNewBi ) ) {

            GameObject pillarsParent = new GameObject( "Pilatri centrali" );
            pillarsParent.transform.parent = section.sectionObj.transform;

            int c = 0;

            for( int i = 1; i < section.bezierCurveLimitedAngle.Count; i++ ) {
                Vector3 m0 = section.bezierCurveLimitedAngle[ i - 1 ];
                Vector3 m1 =  section.bezierCurveLimitedAngle[ i ];

                float lenght = ( m1 - m0 ).magnitude;
                Vector3 dir = ( m1 - m0 ).normalized;

                Vector3 pp = m0 + ( dir * offset );

                if( offset < lenght ) {
                    if( distance < ( lenght - offset ) ) { 
                        
                        float remaingDistance = ( m1 - pp ).magnitude;

                        while( remaingDistance > distance ) {

                            GameObject pillar = GameObject.Instantiate( this.pillar, pp, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, dir, Vector3.forward ) ) );
                            pillar.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                            pillar.transform.Translate( positionCorrection, Space.Self );
                            pillar.isStatic = true;
                            pillar.name = "Pilastro " + c; 
                            pillar.transform.parent = pillarsParent.transform;
                            pillar.AddComponent<FadingInOutCoroutines>();

                            pp += ( dir * distance );
                            remaingDistance = ( m1 - pp ).magnitude;
                            c++;
                        }

                        GameObject lastPillar = GameObject.Instantiate( this.pillar, pp, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, dir, Vector3.forward ) ) );
                        lastPillar.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                        lastPillar.transform.Translate( positionCorrection, Space.Self );
                        lastPillar.isStatic = true;
                        lastPillar.name = "Pilastro" + c;
                        lastPillar.transform.parent = pillarsParent.transform;
                        lastPillar.AddComponent<FadingInOutCoroutines>();

                        offset = distance - remaingDistance;
                    }
                    else {

                        GameObject pillar = GameObject.Instantiate( this.pillar, pp, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, dir, Vector3.forward ) ) );
                        pillar.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                        pillar.transform.Translate( positionCorrection, Space.Self );
                        pillar.isStatic = true;
                        pillar.name = "Pilastro " + c; 
                        pillar.transform.parent = pillarsParent.transform;
                        pillar.AddComponent<FadingInOutCoroutines>();

                        offset += distance - lenght;
                    }
                }
                else {

                    offset -= lenght;
                }
            }
        }
        else {
            return previousOffset;
        }

        return offset;
    }

    private Vector2 AddSidePlatformBanisters( LineSection section, Vector2 previousOffsets, float minDistance, float maxDistance, Vector3 rotationCorrection, Vector3 positionCorrectionLeft, Vector3 positionCorrectionRight ) {

        float distance = Random.Range( minDistance, maxDistance );

        float meshOffsetLeft = previousOffsets[ 0 ];
        float meshOffsetRight = previousOffsets[ 1 ];

        if( section.type == Type.Tunnel ) {

            GameObject banistersLeftParent = new GameObject( "Ringhiere (piattaforma sinistra)" );
            banistersLeftParent.transform.parent = section.sectionObj.transform;

            GameObject banistersRightParent = new GameObject( "Ringhiere (piattaforma destra)" );
            banistersRightParent.transform.parent = section.sectionObj.transform;
            int cLeft = 0, cRight = 0;

            for( int i = 1; i < section.platformSidesPoints.leftFloorRight.Count; i++ ) {
                Vector3 p0Left = section.platformSidesPoints.leftFloorRight[ i - 1 ];
                Vector3 p1Left =  section.platformSidesPoints.leftFloorRight[ i ];

                Vector3 p0Right = section.platformSidesPoints.rightFloorLeft[ i - 1 ];
                Vector3 p1Right =  section.platformSidesPoints.rightFloorLeft[ i ];

                float meshLenghtLeft = ( p1Left - p0Left ).magnitude;
                float meshLenghtRight = ( p1Right - p0Right ).magnitude;
                
                Vector3 meshDirLeft = ( p1Left - p0Left ).normalized;
                Vector3 meshDirRight = ( p1Right - p0Right ).normalized;

                Vector3 bpLeft = p0Left + ( meshDirLeft * meshOffsetLeft );
                Vector3 bpRight = p0Right + ( meshDirRight * meshOffsetRight );

                // Lato sinistro
                if( meshOffsetLeft < meshLenghtLeft ) {
                    if( distance < ( meshLenghtLeft - meshOffsetLeft ) ) { 
                        
                        // Caso 1: La distanza fra gli elementi è minore della distanza fra due punti (in lunghezza) della mesh della piattaforma
                        float remaingDistanceLeft = ( p1Left - bpLeft ).magnitude;

                        while( remaingDistanceLeft > distance ) {

                            GameObject banisterLeft = GameObject.Instantiate( banister, bpLeft, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, meshDirLeft, Vector3.forward ) ) );
                            banisterLeft.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                            banisterLeft.transform.Translate( positionCorrectionLeft, Space.Self );
                            banisterLeft.isStatic = true;
                            banisterLeft.name = "Ringhiera " + cLeft; 
                            banisterLeft.transform.parent = banistersLeftParent.transform;

                            bpLeft += ( meshDirLeft * distance );
                            remaingDistanceLeft = ( p1Left - bpLeft ).magnitude;
                            cLeft++;
                        }

                        GameObject lastBanisterLeft = GameObject.Instantiate( banister, bpLeft, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, meshDirLeft, Vector3.forward ) ) );
                        lastBanisterLeft.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                        lastBanisterLeft.transform.Translate( positionCorrectionLeft, Space.Self );
                        lastBanisterLeft.isStatic = true;
                        lastBanisterLeft.name = "Ringhiera" + cLeft;
                        lastBanisterLeft.transform.parent = banistersLeftParent.transform;

                        meshOffsetLeft = distance - remaingDistanceLeft;
                    }
                    else {

                        // Caso 2: La distanza fra gli elementi è maggiore della distanza fra due punti (in lunghezza) della mesh della piattaforma. 
                        // L'offset precedente è inferiore alla distanza fra due punti (in lunghezza) della mesh della piattaforma. Quindi genero il GameObject all'offset rimanente sommo la 
                        // differenza fra la distanza fra gli elementi e la distanza fra due punti (in lunghezza) della mesh della piattaforma.
                        GameObject banisterLeft = GameObject.Instantiate( banister, bpLeft, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, meshDirLeft, Vector3.forward ) ) );
                        banisterLeft.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                        banisterLeft.transform.Translate( positionCorrectionLeft, Space.Self );
                        banisterLeft.isStatic = true;
                        banisterLeft.name = "Ringhiera " + cLeft; 
                        banisterLeft.transform.parent = banistersLeftParent.transform;

                        meshOffsetLeft += distance - meshLenghtLeft;
                    }
                }
                else {

                    // Caso 3: La distanza fra gli elementi è maggiore della distanza fra due punti (in lunghezza) della mesh della piattaforma. 
                    // L'offset precedente è maggiore della distanza fra due punti (in lunghezza) della mesh della piattaforma.
                    // Non genero nulla e sottraggo all'offset la distanza fra due punti (in lunghezza) della mesh della piattaforma.
                    meshOffsetLeft -= meshLenghtLeft;
                }

                // Lato destro
                if( meshOffsetRight < meshLenghtRight ) {

                    if( distance < ( meshLenghtRight - meshOffsetRight ) ) {

                        // Caso 1: La distanza fra gli elementi è minore della distanza fra due punti (in lunghezza) della mesh della piattaforma
                        float remaingDistanceRight = ( p1Right - bpRight ).magnitude;

                        while( remaingDistanceRight > distance ) {

                            GameObject banisterRight = GameObject.Instantiate( banister, bpRight, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, meshDirRight, Vector3.forward ) ) );
                            banisterRight.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                            banisterRight.transform.Translate( positionCorrectionRight, Space.Self );
                            banisterRight.isStatic = true;
                            banisterRight.name = "Ringhiera " + cRight; 
                            banisterRight.transform.parent = banistersRightParent.transform;

                            bpRight += ( meshDirRight * distance );
                            remaingDistanceRight = ( p1Right - bpRight ).magnitude;
                            cRight++;
                        }

                        GameObject lastBanisterRight = GameObject.Instantiate( banister, bpRight, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, meshDirRight, Vector3.forward ) ) );
                        lastBanisterRight.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                        lastBanisterRight.transform.Translate( positionCorrectionRight, Space.Self );
                        lastBanisterRight.isStatic = true;
                        lastBanisterRight.name = "Ringhiera" + cRight;
                        lastBanisterRight.transform.parent = banistersRightParent.transform;

                        meshOffsetRight = distance - remaingDistanceRight;
                    }
                    else{

                        // Caso 2: La distanza fra gli elementi è maggiore della distanza fra due punti (in lunghezza) della mesh della piattaforma. 
                        // L'offset precedente è inferiore alla distanza fra due punti (in lunghezza) della mesh della piattaforma. Quindi genero il GameObject all'offset rimanente sommo la 
                        // differenza fra la distanza fra gli elementi e la distanza fra due punti (in lunghezza) della mesh della piattaforma.
                        GameObject banisterRight = GameObject.Instantiate( banister, bpRight, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, meshDirRight, Vector3.forward ) ) );
                        banisterRight.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                        banisterRight.transform.Translate( positionCorrectionRight, Space.Self );
                        banisterRight.isStatic = true;
                        banisterRight.name = "Ringhiera " + cRight; 
                        banisterRight.transform.parent = banistersRightParent.transform;

                        meshOffsetRight += distance - meshLenghtRight;
                    }
                }
                else {

                    // Caso 3: La distanza fra gli elementi è maggiore della distanza fra due punti (in lunghezza) della mesh della piattaforma. 
                    // L'offset precedente è maggiore della distanza fra due punti (in lunghezza) della mesh della piattaforma.
                    // Non genero nulla e sottraggo all'offset la distanza fra due punti (in lunghezza) della mesh della piattaforma.
                    meshOffsetRight -= meshLenghtRight;
                }
            }
        }
        else {
            return previousOffsets;
        }

        return new Vector2( meshOffsetLeft, meshOffsetRight );
    }

    private float AddCeilingFans( LineSection section, float previousOffset, float minDistance, float maxDistance, float distanceFromCenter, Vector3 rotationCorrection, Vector3 positionCorrection ) {

        float distance = Random.Range( minDistance, maxDistance );

        float offset = previousOffset;

        if( section.type == Type.Tunnel ) {

            GameObject fansParent = new GameObject( "Ventole" );
            fansParent.transform.parent = section.sectionObj.transform;

            int c = 0;

            for( int i = 1; i < section.bezierCurveLimitedAngle.Count; i++ ) {
                Vector3 m0 = section.bezierCurveLimitedAngle[ i - 1 ];
                Vector3 m1 =  section.bezierCurveLimitedAngle[ i ];

                float lenght = ( m1 - m0 ).magnitude;
                Vector3 dir = ( m1 - m0 ).normalized;

                Vector3 fp = m0 + ( dir * offset );
                fp = new Vector3( fp.x, fp.y, fp.z - tunnelWallHeight );

                int position = 2;

                if( offset < lenght ) {
                    if( distance < ( lenght - offset ) ) { 
                        
                        float remaingDistance = ( m1 - fp ).magnitude;

                        while( remaingDistance > distance ) {

                            if( section.bidirectional ) {
                                
                                position = Random.Range( 0, 3 ); // 0 = Only Left, 1 = Only Right, 2 = Both Sides
                                if( position == 0 || position == 2 ) {
                                    Vector3 fpLeft = fp + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * dir ) * distanceFromCenter;

                                    GameObject fan = GameObject.Instantiate( this.fan, fpLeft, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, dir, Vector3.forward ) ) );
                                    fan.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                                    fan.transform.Translate( positionCorrection, Space.Self );
                                    fan.isStatic = true;
                                    fan.name = "Ventola (sinistra) " + c; 
                                    fan.transform.parent = fansParent.transform;

                                    //Debug.DrawLine( m0, fpLeft, Color.red, 999 );
                                }
                                if( position == 1 || position == 2 ) {
                                    Vector3 fpRight = fp + ( Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * dir ) * distanceFromCenter;

                                    GameObject fan = GameObject.Instantiate( this.fan, fpRight, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, dir, Vector3.forward ) ) );
                                    fan.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                                    fan.transform.Translate( positionCorrection, Space.Self );
                                    fan.isStatic = true;
                                    fan.name = "Ventola (destra) " + c; 
                                    fan.transform.parent = fansParent.transform;

                                    //Debug.DrawLine( m0, fpRight, Color.green, 999 );
                                }
                            }
                            else {
                                GameObject fan = GameObject.Instantiate( this.fan, fp, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, dir, Vector3.forward ) ) );
                                fan.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                                fan.transform.Translate( positionCorrection, Space.Self );
                                fan.isStatic = true;
                                fan.name = "Ventola (centrale) " + c; 
                                fan.transform.parent = fansParent.transform;
                            }

                            fp += ( dir * distance );
                            remaingDistance = ( m1 - fp ).magnitude;
                            c++;
                        }

                        if( section.bidirectional ) {
                            position = Random.Range( 0, 3 ); // 0 = Only Left, 1 = Only Right, 2 = Both Sides
                            if( position == 0 || position == 2 ) {
                                Vector3 fpLeft = fp + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * dir ) * distanceFromCenter;

                                GameObject lastFan = GameObject.Instantiate( this.fan, fpLeft, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, dir, Vector3.forward ) ) );
                                lastFan.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                                lastFan.transform.Translate( positionCorrection, Space.Self );
                                lastFan.isStatic = true;
                                lastFan.name = "Ventola (sinistra) " + c; 
                                lastFan.transform.parent = fansParent.transform;
                            }
                            if( position == 1 || position == 2 ) {
                                Vector3 fpRight = fp + ( Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * dir ) * distanceFromCenter;

                                GameObject lastFan = GameObject.Instantiate( this.fan, fpRight, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, dir, Vector3.forward ) ) );
                                lastFan.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                                lastFan.transform.Translate( positionCorrection, Space.Self );
                                lastFan.isStatic = true;
                                lastFan.name = "Ventola (destra) " + c; 
                                lastFan.transform.parent = fansParent.transform;
                            }
                        }
                        else {
                            GameObject fan = GameObject.Instantiate( this.fan, fp, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, dir, Vector3.forward ) ) );
                            fan.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                            fan.transform.Translate( positionCorrection, Space.Self );
                            fan.isStatic = true;
                            fan.name = "Ventola (centrale) " + c; 
                            fan.transform.parent = fansParent.transform;
                        }
                    }
                    else {
                        
                        if( section.bidirectional ) {
                            position = Random.Range( 0, 3 ); // 0 = Only Left, 1 = Only Right, 2 = Both Sides
                            if( position == 0 || position == 2 ) {
                                Vector3 fpLeft = fp + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * dir ) * distanceFromCenter;

                                GameObject lastFan = GameObject.Instantiate( this.fan, fpLeft, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, dir, Vector3.forward ) ) );
                                lastFan.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                                lastFan.transform.Translate( positionCorrection, Space.Self );
                                lastFan.isStatic = true;
                                lastFan.name = "Ventola (sinistra) " + c; 
                                lastFan.transform.parent = fansParent.transform;
                            }
                            if( position == 1 || position == 2 ) {
                                Vector3 fpRight = fp + ( Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * dir ) * distanceFromCenter;

                                GameObject fan = GameObject.Instantiate( this.fan, fpRight, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, dir, Vector3.forward ) ) );
                                fan.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                                fan.transform.Translate( positionCorrection, Space.Self );
                                fan.isStatic = true;
                                fan.name = "Ventola (destra) " + c; 
                                fan.transform.parent = fansParent.transform;
                            }
                        }
                        else {
                            GameObject fan = GameObject.Instantiate( this.fan, fp, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, dir, Vector3.forward ) ) );
                            fan.transform.localRotation *= Quaternion.Euler( rotationCorrection.x, rotationCorrection.y, rotationCorrection.z );
                            fan.transform.Translate( positionCorrection, Space.Self );
                            fan.isStatic = true;
                            fan.name = "Ventola (centrale) " + c; 
                            fan.transform.parent = fansParent.transform;
                        }

                        offset += distance - lenght;
                    }
                }
                else {

                    offset -= lenght;
                }
            }
        }
        else {
            return previousOffset;
        }

        return offset;
    }

    private void GenerateMeshes() {

        foreach( string lineName in this.lines.Keys ) {
            
            GameObject lineGameObj = new GameObject( lineName );

            //foreach( LineSection section in this.lines[ lineName ] ) {
            for( int i = 0; i < this.lines[ lineName ].Count; i++ ) { 
                
                LineSection section = this.lines[ lineName ][ i ];

                if( i == 0 ) {

                    if( section.fromSection != null && section.fromSection.switchType == SwitchType.MonoToNewMono ) {
                        section.bidirectional = false;
                    }
                    else if( section.fromSection != null && section.fromSection.switchType == SwitchType.BiToNewBi ) {
                        section.bidirectional = true;
                    }
                    else {
                        section.bidirectional = startingBidirectional;
                    }
                }
                else {
                    section.bidirectional = this.lines[ lineName ][ i - 1 ].bidirectional;
                }
                
                string sectionName = "Sezione " + i;
                section.sectionName = sectionName;

                GameObject sectionGameObj = new GameObject( sectionName );
                sectionGameObj.transform.parent = lineGameObj.transform;
                sectionGameObj.transform.position = section.bezierCurveLimitedAngle[ 0 ];

                section.sectionObj = sectionGameObj;

                switch( section.type ) {
                    case Type.Tunnel:   GenerateTunnelGroundMeshes( section, sectionGameObj );
                                        GenerateTunnelSidePlatformMesh( section, sectionGameObj );
                                        GenerateTunnelWallMesh( section, sectionGameObj );
                                        GenerateTunnelTubesMesh( section, sectionGameObj );

                                        break;

                    case Type.Station:  List<Vector3> stationsPoints = section.bezierCurveLimitedAngle;

                                        MeshGenerator.Floor stationRails = new MeshGenerator.Floor();

                                        if( section.bidirectional ) {

                                            if( section.stationType == StationType.BothSidesPlatform ) {
                                            
                                                stationRails = MeshGenerator.CalculateBidirectionalWithBothSidesPlatformFloorMeshVertex( section, centerWidth, tunnelWidth, railsWidth );

                                                // Generazione mesh planari pavimento tunnel
                                                InstantiatePlane( sectionGameObj.transform, "Stazione - Pavimento binari - Sinistra", stationsPoints, stationRails.leftL, stationRails.leftR, 0.0f, false, false, false, this.tunnelRailTexture, this.tunnelRailTextureTilting );
                                                InstantiatePlane( sectionGameObj.transform, "Stazione - Pavimento divisore - Centro", stationsPoints, stationRails.centerL, stationRails.centerR, 0.0f, false, false, false, this.centerTexture, this.centerTextureTilting );
                                                InstantiatePlane( sectionGameObj.transform, "Stazione - Pavimento binari - Destra", stationsPoints, stationRails.rightL, stationRails.rightR, 0.0f, false, false, false, this.tunnelRailTexture, this.tunnelRailTextureTilting );

                                                // Generazione mesh extruded binari
                                                InstantiateRail( sectionGameObj.transform, "Tunnel - Binario L - Sinistra", stationRails.railLeftL, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, null, null );
                                                InstantiateRail( sectionGameObj.transform, "Tunnel - Binario R - Sinistra", stationRails.railLeftR, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, null, null );
                                                InstantiateRail( sectionGameObj.transform, "Tunnel - Binario L - Destra", stationRails.railRightL, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, null, null );
                                                InstantiateRail( sectionGameObj.transform, "Tunnel - Binario R - Destra", stationRails.railRightR, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, null, null );

                                                section.controlsPoints = stationsPoints;
                                                section.floorPoints = stationRails;
                                                section.curvePointsCount = stationRails.centerLine.Count;
                                            }
                                            else if( section.stationType == StationType.CentralPlatform ) {
                                                stationRails = MeshGenerator.CalculateBidirectionalWithCentralPlatformFloorMeshVertex( section, this.centerWidth, this.tunnelWidth, this.stationLenght, this.stationExtensionLenght, this.stationExtensionHeight, this.stationExtensionCurvePoints );

                                                // Generazione mesh planari pavimento tunnel
                                                InstantiatePlane( sectionGameObj.transform, "Stazione - Pavimento binari - Sinistra", stationRails.leftLine, stationRails.leftL, stationRails.leftR, 0.0f, false, false, false, this.tunnelRailTexture, this.tunnelRailTextureTilting );
                                                InstantiatePlane( sectionGameObj.transform, "Stazione - Pavimento binari - Destra", stationRails.rightLine, stationRails.rightL, stationRails.rightR, 0.0f, false, false, false, this.tunnelRailTexture, this.tunnelRailTextureTilting );

                                                // Generazione mesh extruded binari
                                                // TODO: Non sono ancora calcolari i punti dei binari in CalculateBidirectionalWithCentralPlatformFloorMeshVertex
                                                // InstantiateRail( sectionGameObj.transform, "Tunnel - Binario L - Sinistra", stationRails.railLeftL, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, null, null, null );
                                                // InstantiateRail( sectionGameObj.transform, "Tunnel - Binario R - Sinistra", stationRails.railLeftR, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, null, null, null );
                                                // InstantiateRail( sectionGameObj.transform, "Tunnel - Binario L - Destra", stationRails.railRightL, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, null, null, null );
                                                // InstantiateRail( sectionGameObj.transform, "Tunnel - Binario R - Destra", stationRails.railRightR, this.railShape, null, this.railShapeScale, this.railShapeHorPosCorrection, 0.0f, this.railSmoothFactor, true, false, this.railTexture, this.railTextureTilting, null, null, null );

                                                List<Vector3> centralWallStartBaseline = new List<Vector3>();
                                                List<Vector3> centralWallStartBaselineLeft = new List<Vector3>();
                                                List<Vector3> centralWallStartBaselineRight = new List<Vector3>();

                                                List<Vector3> centralWallEndBaseline = new List<Vector3>();
                                                List<Vector3> centralWallEndBaselineLeft = new List<Vector3>();
                                                List<Vector3> centralWallEndBaselineRight = new List<Vector3>();

                                                for( int k = 0; k < this.stationExtensionCurvePoints; k++ ) {

                                                    centralWallStartBaselineLeft.Add( stationRails.leftR[ this.stationExtensionCurvePoints - 1 - k ] );
                                                    centralWallStartBaselineRight.Add( stationRails.rightL[ k ] );

                                                    centralWallEndBaselineLeft.Add( stationRails.leftR[ stationRails.leftR.Count - this.stationExtensionCurvePoints + k ] );
                                                    centralWallEndBaselineRight.Add( stationRails.rightL[ stationRails.rightL.Count - 1 - k ] );
                                                }

                                                centralWallStartBaseline.AddRange( centralWallStartBaselineLeft );
                                                centralWallStartBaseline.AddRange( centralWallStartBaselineRight );

                                                centralWallEndBaseline.AddRange( centralWallEndBaselineLeft );
                                                centralWallEndBaseline.AddRange( centralWallEndBaselineRight );

                                                Vector3 stationDir = ( stationsPoints[ stationsPoints.Count - 1] - stationsPoints[ 0 ] ).normalized;

                                                Dictionary<int, List<Vector3>> verticesStructure;
                                                verticesStructure = InstantiateExtrudedMesh( sectionGameObj.transform, "Stazione - Muro - Centrale Iniziale", centralWallStartBaseline, this.stationCentralWallShape, null, this.stationCentralWallShapeScale, 0.0f, 0.0f, 0.0f, this.stationCentralWallSmoothFactor, true, false, this.tunnelWallTexture, this.tunnelWallTextureTilting ).verticesStructure;
                                                InstantiateCeiling( sectionGameObj.transform, "Stazione - Soffitto - Centrale Iniziale", verticesStructure[ this.stationCentralWallShape.Count - 1 ], false, stationDir, this.ceilingTexture, this.ceilingTextureTilting );
                                                
                                                verticesStructure = InstantiateExtrudedMesh( sectionGameObj.transform, "Stazione - Muro - Centrale Finale", centralWallEndBaseline, this.stationCentralWallShape, null, this.stationCentralWallShapeScale, 0.0f, 0.0f, 180.0f, this.stationCentralWallSmoothFactor, false, false, this.tunnelWallTexture, this.tunnelWallTextureTilting ).verticesStructure;
                                                InstantiateCeiling( sectionGameObj.transform, "Stazione - Soffitto - Centrale Finale", verticesStructure[ this.stationCentralWallShape.Count - 1 ], true, stationDir, this.ceilingTexture, this.ceilingTextureTilting );


                                                section.controlsPoints = stationsPoints;
                                                section.floorPoints = stationRails;
                                                section.curvePointsCount = stationRails.leftLine.Count;
                                            }
                                        }
                                        else {
                                            stationRails = MeshGenerator.CalculateMonodirectionalFloorMeshVertex( stationsPoints, null, tunnelWidth, tunnelParabolic, this.railsWidth );

                                            Mesh centerFloorMesh = new Mesh();

                                            centerFloorMesh = MeshGenerator.GeneratePlanarMesh( stationsPoints, MeshGenerator.ConvertListsToMatrix_2xM( stationRails.centerL, stationRails.centerR ), false, false, false, tunnelRailTextureTilting.x, tunnelRailTextureTilting.y );
                                            GameObject leftFloorGameObj = new GameObject( "Binari centrali" );
                                            leftFloorGameObj.transform.parent = sectionGameObj.transform;
                                            leftFloorGameObj.transform.position = Vector3.zero;
                                            leftFloorGameObj.AddComponent<MeshFilter>();
                                            leftFloorGameObj.AddComponent<MeshRenderer>();
                                            leftFloorGameObj.GetComponent<MeshFilter>().sharedMesh = centerFloorMesh;
                                            leftFloorGameObj.GetComponent<MeshRenderer>().material = tunnelRailTexture;

                                            section.controlsPoints = stationsPoints;
                                            section.floorPoints = stationRails;
                                            section.curvePointsCount = stationRails.centerLine.Count;
                                        }
                    
                                        break;

                    case Type.Switch:   GenerateSwitchMeshes( lineName, i, section, sectionGameObj );
                                        break;
                }
            }
        }
    }

    private Vector3 GenerateLine( string lineName, CardinalPoint previousLineOrientation, CardinalPoint lineOrientation, int lineLength, Vector3 startingPoint, Vector3 startingDir, bool generateNewLines, LineSection fromSection ) {
        
        LineSection previousSection = null;

        lineTurnDistance = Random.Range( lineTurnDistanceMin, lineTurnDistanceMax );
        stationsDistance = Random.Range( stationsDistanceMin, stationsDistanceMax );
        switchDistance = stationsDistance;
        while( switchDistance == stationsDistance ) {
            switchDistance = Random.Range( switchDistanceMin, switchDistanceMax );
        }

        List<CardinalPoint> sectionAllAvailableOrientations = new List<CardinalPoint>();

        if( lineOrientation == CardinalPoint.Random ) {
            lineOrientation = ( CardinalPoint )Random.Range( 0, 4 );
        }
        switch( lineOrientation ) {
            case CardinalPoint.East:    sectionAllAvailableOrientations.Add( CardinalPoint.North );
                                        sectionAllAvailableOrientations.Add( CardinalPoint.East );
                                        sectionAllAvailableOrientations.Add( CardinalPoint.South );
                                        break;

            case CardinalPoint.West:    sectionAllAvailableOrientations.Add( CardinalPoint.North );
                                        sectionAllAvailableOrientations.Add( CardinalPoint.West );
                                        sectionAllAvailableOrientations.Add( CardinalPoint.South );
                                        break;

            case CardinalPoint.North:   sectionAllAvailableOrientations.Add( CardinalPoint.East );
                                        sectionAllAvailableOrientations.Add( CardinalPoint.North );
                                        sectionAllAvailableOrientations.Add( CardinalPoint.West );
                                        break;

            case CardinalPoint.South:   sectionAllAvailableOrientations.Add( CardinalPoint.East );
                                        sectionAllAvailableOrientations.Add( CardinalPoint.South );
                                        sectionAllAvailableOrientations.Add( CardinalPoint.West );   
                                        break;
        }
        //Debug.Log( "MAIN DIR: " + lineOrientation );

        CardinalPoint sectionOrientation = lineOrientation;

        if( startingPoint == Vector3.zero ) {
            startingPoint = gameObject.transform.position;
        }

        // Genero la lista delle sezioni della linea
        List<LineSection> sections = new List<LineSection>();
        List<Vector3> bordersUp = new List<Vector3>();
        List<Vector3> bordersDown = new List<Vector3>();

        bool removeLine = false;
        int sectionsBeforeTurnCounter = 0;

        for( int i = 0; i < lineLength; i++ ) {
            
            if( i % lineTurnDistance == 0 && sections.Count != 0 ) {

                List<LineSection> actualSections = new List<LineSection>();
                //Debug.Log( ">>>>>>>>>>> Proibited Areas: " + lineName );
                //Debug.Log( ">>>>>>>>>>> Line Turn Distance: " + lineTurnDistance );
                for( int k = i - sectionsBeforeTurnCounter; k < i; k++ ) {
                    actualSections.Add( sections[ k ] );

                    //Debug.Log( ">>>>>>>>>>> indice sezioni: " + k );
                }
                //Debug.Log( "actualSections[0]: " + actualSections[ 0 ] );
                if( this.proibitedAreas.ContainsKey( lineName ) ){ 
                    this.proibitedAreas[ lineName ].Add( CalculateProibitedRectangularArea( actualSections, sectionOrientation ) );
                }
                else {
                    this.proibitedAreas.Add( lineName, new List<List<Vector3>>{ CalculateProibitedRectangularArea( actualSections, sectionOrientation ) } );
                }

                List<CardinalPoint> sectionActualAvailableOrientations = sectionAllAvailableOrientations;
                if( sectionOrientation != CardinalPoint.Random ) {
                    
                    switch( sectionOrientation ) {
                        case CardinalPoint.East:    sectionActualAvailableOrientations.Remove( CardinalPoint.West );
                                                    break;

                        case CardinalPoint.West:    sectionActualAvailableOrientations.Remove( CardinalPoint.East );
                                                    break;

                        case CardinalPoint.North:   sectionActualAvailableOrientations.Remove( CardinalPoint.South );
                                                    break;

                        case CardinalPoint.South:   sectionActualAvailableOrientations.Remove( CardinalPoint.North );   
                                                    break;
                    }
                }
                sectionOrientation = sectionActualAvailableOrientations[ Random.Range( 0, sectionActualAvailableOrientations.Count ) ];

                sectionsBeforeTurnCounter = 0;
                int nextTurnAfter = Random.Range( lineTurnDistanceMin, lineTurnDistanceMax );

                if( lineTurnDistance + nextTurnAfter < lineLength ) {
                    lineTurnDistance += nextTurnAfter;
                }
            }

            sectionsBeforeTurnCounter++;

            if( startingDir == Vector3.zero ) {
                switch( sectionOrientation ) {
                    case CardinalPoint.East:    startingDir = Vector3.right;
                                                break;

                    case CardinalPoint.West:    startingDir = -Vector3.right;
                                                break;

                    case CardinalPoint.North:   startingDir = Vector3.up;
                                                break;

                    case CardinalPoint.South:   startingDir = -Vector3.up;    
                                                break;
                }
            }

            string sectionName = "Sezione " + i;

            if( lines.ContainsKey( lineName ) ) {
                sections = lines[ lineName ];
            }
            else {
                lines.Add( lineName, sections );
            }

            if( sections.Count > 0 ) {
                startingDir =  sections[ ^1 ].nextStartingDirections[ 0 ];
                startingPoint = sections[ ^1 ].nextStartingPoints[ 0 ];
            }

            LineSection section = new LineSection();
            section.previousSection = previousSection;
            section.mainDir = sectionOrientation;

            if( i > 0 && i < sectionsNumber - 1 && ( i % stationsDistance == 0 || i % switchDistance == 0 ) ) {

                section.bidirectional = sections[ i - 1 ].bidirectional;

                List<Vector3> nextStartingDirections = new List<Vector3>();
                nextStartingDirections.Add( startingDir );
                section.nextStartingDirections = nextStartingDirections;

                List<Vector3> nextStartingPoints = new List<Vector3>();
                if( i % stationsDistance == 0 ) {
                    section.type = Type.Station;
                    int variant = Random.Range( 0, 2 );
                    if( variant == 0 ) {
                        // Variante con banchine su entrambi i lati
                        nextStartingPoints.Add( startingPoint + ( startingDir.normalized * stationLenght ) );
                        section.stationType = StationType.BothSidesPlatform;
                    }
                    else if(variant == 1 ) {
                        // Variante con banchina centrale
                        nextStartingPoints.Add( startingPoint + ( startingDir.normalized * ( stationLenght + ( 2 * stationExtensionLenght ) ) ) );
                        section.stationType = StationType.CentralPlatform;
                    }

                    nextStartingPoints.Add( startingPoint + ( startingDir.normalized * stationLenght ) );
                    section.nextStartingPoints = nextStartingPoints;
                    stationsDistance += Random.Range( stationsDistanceMin, stationsDistanceMax );
                    if( switchDistance == stationsDistance ) {
                        switchDistance = stationsDistance + 1;
                    }
                }
                else if( i % switchDistance == 0 ) { 
                    section.type = Type.Switch;

                    nextStartingPoints.Add( startingPoint + ( startingDir.normalized * switchLenght ) );
                    section.nextStartingPoints = nextStartingPoints;
                    switchDistance += Random.Range( switchDistanceMin, switchDistanceMax );
                }

                List<Vector3> curvePoints = new List<Vector3>{ startingPoint, nextStartingPoints[ 0 ] };
                section.controlsPoints = curvePoints;
                section.bezierCurveLimitedAngle = curvePoints;
                section.curvePointsCount = 2;
                //if( section.type == Type.Switch ) {
                    section.centerCoords = curvePoints[ 0 ] + ( startingDir.normalized * ( curvePoints[ 1 ] - curvePoints[ 0 ] ).magnitude / 2 );
                //}
                
            }
            else {
                section.type = Type.Tunnel;

                List<Vector3> controlPoints = GenerateControlPoints( sectionOrientation, startingDir, startingPoint, distanceMultiplier, controlPointsNumber, tunnelStraightness );

                List<Vector3> baseCurve = BezierCurveCalculator.CalculateBezierCurve( controlPoints, baseBezierCurvePointsNumber );
                List<Vector3> fixedLenghtCurve = BezierCurveCalculator.RecalcultateCurveWithFixedLenght( baseCurve, baseCurve.Count );
                List<Vector3> limitedAngleCurve = BezierCurveCalculator.RecalcultateCurveWithLimitedAngle( fixedLenghtCurve, maxAngle, startingDir );

                // Update dettagli LineSection 
                section.controlsPoints = controlPoints;
                section.bezierCurveBase = baseCurve;
                section.bezierCurveFixedLenght = fixedLenghtCurve;
                section.bezierCurveLimitedAngle = limitedAngleCurve;
                section.curvePointsCount = limitedAngleCurve.Count;

                List<Vector3> nextStartingDirections = new List<Vector3>();
                nextStartingDirections.Add( limitedAngleCurve[ ^1 ] - limitedAngleCurve[ ^2 ] );
                section.nextStartingDirections = nextStartingDirections;

                List<Vector3> nextStartingPoints = new List<Vector3>();
                nextStartingPoints.Add( limitedAngleCurve[ ^1 ] );
                section.nextStartingPoints = nextStartingPoints;

                List<Vector3> bordersSectionUp = new List<Vector3>();
                List<Vector3> bordersSectionDown = new List<Vector3>();
            }

            section.orientation = sectionOrientation;
            section.sectionIndex = i;
            section.lineName = lineName;
            if( i == 0 ) {
                section.fromSection = fromSection;
            }

            sections.Add( section );
            previousSection = section;

            foreach( Vector3 curvePoint in section.bezierCurveLimitedAngle ) {
                if( isPointInsideProibitedArea( curvePoint, lineName ) ) {
                    removeLine = true;
                    break;
                }
            }

            if( i == lineLength - 1  && sections.Count != 0 ) {
                List<LineSection> lastSections = new List<LineSection>();
                for( int k = lineTurnDistance; k < lineLength; k++ ) {
                    lastSections.Add( sections[ k ] );
                }
                this.proibitedAreas[ lineName ].Add( CalculateProibitedRectangularArea( lastSections, sectionOrientation ) );
            }
        }

        if( removeLine ) {
            this.proibitedAreas.Remove( lineName );
            this.lines.Remove( lineName );
        }
        else {
            if( generateNewLines ) {
                foreach( LineSection finalSection in sections ) {
                    if( finalSection.type == Type.Switch ) {
                        Vector3 finalDir = ( finalSection.nextStartingPoints[ 0 ] - finalSection.bezierCurveLimitedAngle[ 0 ] );
                        Vector3 switchCenter = finalSection.bezierCurveLimitedAngle[ 0 ] + ( finalDir / 2 );
                        Vector3 switchLeft = switchCenter + Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * finalDir.normalized * switchBracketsLenght;
                        Vector3 switchRight = switchCenter + Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * finalDir.normalized * switchBracketsLenght;

                        bool leftAvailable = !isPointInsideProibitedArea( switchLeft, null );
                        bool rightAvailable = !isPointInsideProibitedArea( switchRight, null );

                        Dictionary<NewLineSide, LineStart> newLinesStarts = new Dictionary<NewLineSide, LineStart>();
                        switch( finalSection.orientation ) {
                            case CardinalPoint.East:    if( leftAvailable ) {
                                                            LineStart newLineStart = new LineStart( CardinalPoint.North, NewLineSide.Left, switchLeft, switchLeft - switchCenter);
                                                            newLinesStarts.Add( NewLineSide.Left, newLineStart );
                                                        }
                                                        if( rightAvailable ) {
                                                            LineStart newLineStart = new LineStart( CardinalPoint.South, NewLineSide.Right, switchRight, switchRight - switchCenter );
                                                            newLinesStarts.Add( NewLineSide.Right, newLineStart );
                                                        }
                                                        break;

                            case CardinalPoint.West:    if( leftAvailable ) {
                                                            LineStart newLineStart = new LineStart( CardinalPoint.South, NewLineSide.Left, switchLeft, switchLeft - switchCenter );
                                                            newLinesStarts.Add( NewLineSide.Left, newLineStart );
                                                        }
                                                        if( rightAvailable ) {
                                                            LineStart newLineStart = new LineStart( CardinalPoint.North, NewLineSide.Right, switchRight, switchRight - switchCenter );
                                                            newLinesStarts.Add( NewLineSide.Right, newLineStart );
                                                        }
                                                        break;

                            case CardinalPoint.North:   if( leftAvailable ) {
                                                            LineStart newLineStart = new LineStart( CardinalPoint.West, NewLineSide.Left, switchLeft, switchLeft - switchCenter );
                                                            newLinesStarts.Add( NewLineSide.Left, newLineStart );
                                                        }
                                                        if( rightAvailable ) {
                                                            LineStart newLineStart = new LineStart( CardinalPoint.East, NewLineSide.Right, switchRight, switchRight - switchCenter );
                                                            newLinesStarts.Add( NewLineSide.Right, newLineStart );
                                                        }
                                                        break;

                            case CardinalPoint.South:   if( leftAvailable ) {
                                                        LineStart newLineStart = new LineStart( CardinalPoint.East, NewLineSide.Left, switchLeft, switchLeft - switchCenter );
                                                        newLinesStarts.Add( NewLineSide.Left, newLineStart );
                                                    }
                                                    if( rightAvailable ) {
                                                        LineStart newLineStart = new LineStart( CardinalPoint.West, NewLineSide.Right, switchRight, switchRight - switchCenter );
                                                        newLinesStarts.Add( NewLineSide.Right, newLineStart );
                                                    }   
                                                    break;
                        }

                        if( leftAvailable && rightAvailable ) {
                            finalSection.newLineSide = NewLineSide.Both;
                        } 
                        else if( leftAvailable ) {
                            finalSection.newLineSide = NewLineSide.Left;
                        }
                        else if( leftAvailable ) {
                            finalSection.newLineSide = NewLineSide.Right;
                        } 
                        finalSection.newLinesStarts = newLinesStarts;
                    }
                }
            }
        }

        lineCounter++;

        return sections[ ^1 ].bezierCurveLimitedAngle[ ^1 ];
    }

    private bool isPointInsideProibitedArea( Vector3 point, string excludedLine ) {
        foreach( string lineName in this.proibitedAreas.Keys ) {
            
            if( excludedLine != lineName ) {
                List<List<Vector3>> proibitedAreas = this.proibitedAreas[ lineName ];
                foreach( List<Vector3> proibitedAreaVertex in proibitedAreas) {

                    float xMin = proibitedAreaVertex[ 0 ].x, xMax = proibitedAreaVertex[ 0 ].x, yMin = proibitedAreaVertex[ 0 ].y, yMax = proibitedAreaVertex[ 0 ].y;
                    for( int i = 1; i < proibitedAreaVertex.Count; i++ ) {
                        
                        if( xMin > proibitedAreaVertex[ i ].x ) {
                            xMin = proibitedAreaVertex[ i ].x;
                        }
                        if( xMax < proibitedAreaVertex[ i ].x ) {
                            xMax = proibitedAreaVertex[ i ].x;
                        }

                        if( yMin > proibitedAreaVertex[ i ].y ) {
                            yMin = proibitedAreaVertex[ i ].y;
                        }
                        if( yMax < proibitedAreaVertex[ i ].y ) {
                            yMax = proibitedAreaVertex[ i ].y;
                        }
                    }

                    if( point.x >= xMin && point.x <= xMax && point.y >= yMin && point.y <= yMax ) {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private List<Vector3> CalculateProibitedRectangularArea( List<LineSection> sections, CardinalPoint lineOrientation ) {

        float xMin = 0.0f, xMax = 0.0f, yMin = 0.0f, yMax = 0.0f;

        for( int i = 0; i < sections.Count; i++ ) {
            for( int j = 0; j < sections[ i ].bezierCurveLimitedAngle.Count; j++ ) {
                if( i == 0 && j == 0 ) {
                    xMin = xMax = sections[ i ].bezierCurveLimitedAngle[ j ].x;
                    yMin = yMax = sections[ i ].bezierCurveLimitedAngle[ j ].y;
                }
                else {
                    if( xMin > sections[ i ].bezierCurveLimitedAngle[ j ].x ) {
                        xMin = sections[ i ].bezierCurveLimitedAngle[ j ].x;
                    }
                    if( xMax < sections[ i ].bezierCurveLimitedAngle[ j ].x ) {
                        xMax = sections[ i ].bezierCurveLimitedAngle[ j ].x;
                    }

                    if( yMin > sections[ i ].bezierCurveLimitedAngle[ j ].y ) {
                        yMin = sections[ i ].bezierCurveLimitedAngle[ j ].y;
                    }
                    if( yMax < sections[ i ].bezierCurveLimitedAngle[ j ].y ) {
                        yMax = sections[ i ].bezierCurveLimitedAngle[ j ].y;
                    }
                }
            }
        }

        switch( lineOrientation ) {
            case CardinalPoint.East: 
            case CardinalPoint.West:    yMin -= ( tunnelWidth+ 2 );
                                        yMax += ( tunnelWidth+ 2 );
                                        break;

            case CardinalPoint.North: 
            case CardinalPoint.South:   xMin -= ( tunnelWidth+ 2 );
                                        xMax += ( tunnelWidth+ 2 );
                                        break;
        }

        return new List<Vector3> { new Vector3( xMin, yMin, 0.0f ), new Vector3( xMax, yMin, 0.0f ), new Vector3( xMax, yMax, 0.0f ), new Vector3( xMin, yMax, 0.0f ) };
    }

    private void InstantiateTrain() {
        Vector3 trainPos = lines[ "Linea 0" ][ 0 ].bezierCurveLimitedAngle[ 0 ];
        trainPos.z += trainHeightFromGround;
        Vector3 trainDir = lines[ "Linea 0" ][ 0 ].bezierCurveLimitedAngle[ 1 ] - lines[ "Linea 0" ][ 0 ].bezierCurveLimitedAngle[ 0 ];
        GameObject instantiatedTrain = Instantiate( train, trainPos, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, trainDir, Vector3.forward ) ) );
        instantiatedTrain.name = "Train";

        InstantiateMainCamera();
    }

    private void InstantiateMainCamera() {
        Vector3 trainPos = lines[ "Linea 0" ][ 0 ].bezierCurveLimitedAngle[ 0 ];
        trainPos.z += trainHeightFromGround;
        Vector3 trainDir = lines[ "Linea 0" ][ 0 ].bezierCurveLimitedAngle[ 1 ] - lines[ "Linea 0" ][ 0 ].bezierCurveLimitedAngle[ 0 ];
        mainCamera.tag = "MainCamera";
        Instantiate( mainCamera, trainPos, Quaternion.identity );
    }

    private List<Vector3> GenerateControlPoints( CardinalPoint orientation, Vector3 startingDir, Vector3 startingPoint, int pointsDistanceMultiplier, int pointsNumber, float straightness ) {
        List<Vector3> controlPoints = new List<Vector3>();
        controlPoints.Add( startingPoint );
        Vector3 furthermostPoint;
            
        controlPoints.Add( startingPoint + ( startingDir.normalized * pointsDistanceMultiplier ) );
        
        furthermostPoint = controlPoints[ 1 ];
        //if( furthermostPoint.x < controlPoints[ 0 ].x ) {
            //furthermostPoint = new Vector3( controlPoints[ 0 ].x, furthermostPoint.y, furthermostPoint.z );
        //}

        for( int i = 1; i < pointsNumber; i++ ) { 

            Vector2 range = new Vector2( -90.0f, 90.0f );
            range.x += 90.0f * straightness;
            range.y -= 90.0f * straightness;
            float angle = Random.Range( range.x, range.y );

            switch( orientation ) {
                case CardinalPoint.East:    angle += 0.0f;
                                            break;

                case CardinalPoint.West:    angle += 180.0f;
                                            break;

                case CardinalPoint.North:   angle += 90.0f;
                                            break;

                case CardinalPoint.South:   angle -= 90.0f;    
                                            break;
            }

            Vector3 newDir = Quaternion.Euler( 0.0f, 0.0f, angle ) * Vector3.right;
            furthermostPoint = furthermostPoint + ( newDir.normalized * pointsDistanceMultiplier );
            controlPoints.Add( furthermostPoint );
        }
        
        return controlPoints;
    }

    private void OnDrawGizmos()
    {
        if( lineGizmos ) {
            foreach( string lineName in this.lines.Keys ) {

                foreach( LineSection segment in this.lines[ lineName ] ) {

                    if( segment.type == Type.Tunnel ) {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawSphere( segment.bezierCurveLimitedAngle[ 0 ], 5.0f );
                    }
                    else if( segment.type == Type.Switch ) {
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawSphere( segment.bezierCurveLimitedAngle[ 0 ], 5.0f );
                    }
                    else if( segment.type == Type.Station ) {
                        Gizmos.color = Color.green;
                        Gizmos.DrawSphere( segment.bezierCurveLimitedAngle[ 0 ], 5.0f );
                    }

                    for( int i = 0; i < segment.bezierCurveLimitedAngle.Count; i++ ) {

                        
                        if( i > 0 ) {

                            //if( segment.type == Type.Tunnel ) {
                                if( segment.orientation == CardinalPoint.North ) {
                                    Gizmos.color = Color.yellow;
                                }
                                else if( segment.orientation == CardinalPoint.South ) {
                                    Gizmos.color = Color.cyan;
                                }
                                else if( segment.orientation == CardinalPoint.East ) {
                                    Gizmos.color = Color.green;
                                }
                                else if( segment.orientation == CardinalPoint.West ) {
                                    Gizmos.color = Color.white;
                                }
                            //}
                            //else if( segment.type == Type.Switch ) {
                                //Gizmos.color = Color.magenta;
                            //}
                            //else if( segment.type == Type.Station ) {
                                //Gizmos.color = Color.green;
                            //}
                            Gizmos.DrawLine( segment.bezierCurveLimitedAngle[ i - 1 ], segment.bezierCurveLimitedAngle[ i ] );
                            Gizmos.DrawWireSphere( segment.bezierCurveLimitedAngle[ i ], 1.0f );
                        }
                    }

                    if( segment.newLinesStarts != null && segment.newLinesStarts.Count > 0 ) {
                        foreach( NewLineSide side in segment.newLinesStarts.Keys ) {
                            Gizmos.color = Color.magenta;
                            Gizmos.DrawWireCube( segment.newLinesStarts[ side ].pos, Vector3.one * 25 );
                            Gizmos.DrawLine( segment.bezierCurveLimitedAngle[ 0 ], segment.newLinesStarts[ side ].pos );
                        }
                    }
                }
            }

            foreach( string lineName in this.proibitedAreas.Keys ) {
                Gizmos.color = Color.red;

                List<List<Vector3>> proibitedAreas = this.proibitedAreas[ lineName ];
                foreach( List<Vector3> proibitedAreaVertex in proibitedAreas) {
                    for( int i = 0; i < proibitedAreaVertex.Count; i++ ) {
                        
                        if( i < proibitedAreaVertex.Count - 1 ) {
                            Gizmos.DrawLine( proibitedAreaVertex[ i ], proibitedAreaVertex[ i + 1 ] );
                        }
                        else {
                            Gizmos.DrawLine( proibitedAreaVertex[ i ], proibitedAreaVertex[ 0 ] );
                        }
                    }
                }
            }   
        }

        foreach( string line in lines.Keys ) {

            foreach( LineSection segment in lines[ line ] ) {

                if(  segment.type == Type.Switch ) {

                    if( segment.switchType == SwitchType.BiToBi ) {
                        for( int i = 0; i < segment.floorPoints.leftLine.Count; i++ ) {
                            
                            if( i > 0 ) {
                                if( segment.activeSwitch == SwitchDirection.LeftToLeft ) {
                                    Gizmos.color = Color.green;
                                }
                                else {
                                    Gizmos.color = Color.yellow;
                                }
                                Gizmos.DrawLine( segment.floorPoints.leftLine[ i - 1 ], segment.floorPoints.leftLine[ i ] );

                                if( segment.activeSwitch == SwitchDirection.RightToRight ) {
                                    Gizmos.color = Color.green;
                                }
                                else {
                                    Gizmos.color = Color.blue;
                                }
                                Gizmos.DrawLine( segment.floorPoints.rightLine[ i - 1 ], segment.floorPoints.rightLine[ i ] );
                            }
                        }
                        for( int i = 0; i < segment.floorPoints.leftRightLine.Count; i++ ) {
                            
                            if( i > 0 ) {
                                if( segment.activeSwitch == SwitchDirection.LeftToRight ) {
                                    Gizmos.color = Color.green;
                                }
                                else {
                                    Gizmos.color = Color.yellow;
                                }
                                Gizmos.DrawLine( segment.floorPoints.leftRightLine[ i - 1 ], segment.floorPoints.leftRightLine[ i ] );
                                if( segment.activeSwitch == SwitchDirection.RightToLeft ) {
                                    Gizmos.color = Color.green;
                                }
                                else {
                                    Gizmos.color = Color.blue;
                                }
                                Gizmos.DrawLine( segment.floorPoints.rightLeftLine[ i - 1 ], segment.floorPoints.rightLeftLine[ i ] );
                            }
                        }
                    }
                    else if( segment.switchType == SwitchType.BiToMono || segment.switchType == SwitchType.MonoToBi ) {
                        for( int i = 0; i < segment.floorPoints.leftCenterLine.Count; i++ ) {
                            if( i > 0 ) {
                                if( segment.activeSwitch == SwitchDirection.LeftToCenter ) {
                                    Gizmos.color = Color.green;
                                }
                                else {
                                    Gizmos.color = Color.yellow;
                                }
                                Gizmos.DrawLine( segment.floorPoints.leftCenterLine[ i - 1 ], segment.floorPoints.leftCenterLine[ i ] );
                            }
                        }
                        for( int i = 0; i < segment.floorPoints.rightCenterLine.Count; i++ ) {
                            if( i > 0 ) {
                                if( segment.activeSwitch == SwitchDirection.RightToCenter ) {
                                    Gizmos.color = Color.green;
                                }
                                else {
                                    Gizmos.color = Color.blue;
                                }
                                Gizmos.DrawLine( segment.floorPoints.rightCenterLine[ i - 1 ], segment.floorPoints.rightCenterLine[ i ] );
                            }
                        }
                    }

                    if( segment.switchType == SwitchType.MonoToNewMono ) {
                        for( int i = 0; i < segment.floorPoints.centerLine.Count; i++ ) {
                            if( i > 0 ) {
                                if( segment.activeSwitch == SwitchDirection.CenterToCenter ) {
                                    Gizmos.color = Color.green;
                                }
                                else {
                                    Gizmos.color = Color.yellow;
                                }
                                Gizmos.DrawLine( segment.floorPoints.centerLine[ i - 1 ], segment.floorPoints.centerLine[ i ] );
                            }
                        }
                        if( segment.floorPoints.centerEntranceLeft != null ) {
                            for( int i = 0; i < segment.floorPoints.centerEntranceLeft.Count; i++ ) {
                                if( i > 0 ) {
                                    if( segment.activeSwitch == SwitchDirection.CenterToEntranceLeft ) {
                                        Gizmos.color = Color.green;
                                    }
                                    else {
                                        Gizmos.color = Color.blue;
                                    }
                                    Gizmos.DrawLine( segment.floorPoints.centerEntranceLeft[ i - 1 ], segment.floorPoints.centerEntranceLeft[ i ] );
                                }
                            }
                        }
                        if( segment.floorPoints.centerEntranceRight != null ) {
                            for( int i = 0; i < segment.floorPoints.centerEntranceRight.Count; i++ ) {
                                if( i > 0 ) {
                                    if( segment.activeSwitch == SwitchDirection.CenterToEntranceRight ) {
                                        Gizmos.color = Color.green;
                                    }
                                    else {
                                        Gizmos.color = Color.blue;
                                    }
                                    Gizmos.DrawLine( segment.floorPoints.centerEntranceRight[ i - 1 ], segment.floorPoints.centerEntranceRight[ i ] );
                                }
                            }
                        }
                        if( segment.floorPoints.centerExitLeft != null ) {
                            for( int i = 0; i < segment.floorPoints.centerExitLeft.Count; i++ ) {
                                if( i > 0 ) {
                                    if( segment.activeSwitch == SwitchDirection.CenterToExitLeft ) {
                                        Gizmos.color = Color.green;
                                    }
                                    else {
                                        Gizmos.color = Color.red;
                                    }
                                    Gizmos.DrawLine( segment.floorPoints.centerExitLeft[ i - 1 ], segment.floorPoints.centerExitLeft[ i ] );
                                }
                            }
                        }
                        if( segment.floorPoints.centerExitRight != null ) {
                            for( int i = 0; i < segment.floorPoints.centerExitRight.Count; i++ ) {
                                if( i > 0 ) {
                                    if( segment.activeSwitch == SwitchDirection.CenterToExitRight ) {
                                        Gizmos.color = Color.green;
                                    }
                                    else {
                                        Gizmos.color = Color.red;
                                    }
                                    Gizmos.DrawLine( segment.floorPoints.centerExitRight[ i - 1 ], segment.floorPoints.centerExitRight[ i ] );
                                }
                            }
                        }
                    }
                    else if( segment.switchType == SwitchType.BiToNewBi ) {
                        for( int i = 0; i < segment.floorPoints.leftLine.Count; i++ ) {
                            if( i > 0 ) {
                                if( segment.activeSwitch == SwitchDirection.LeftToLeft ) {
                                    Gizmos.color = Color.green;
                                }
                                else {
                                    Gizmos.color = Color.yellow;
                                }
                                Gizmos.DrawLine( segment.floorPoints.leftLine[ i - 1 ], segment.floorPoints.leftLine[ i ] );
                            }
                        }
                        for( int i = 0; i < segment.floorPoints.rightLine.Count; i++ ) {
                            if( i > 0 ) {
                                if( segment.activeSwitch == SwitchDirection.RightToRight ) {
                                    Gizmos.color = Color.green;
                                }
                                else {
                                    Gizmos.color = Color.yellow;
                                }
                                Gizmos.DrawLine( segment.floorPoints.rightLine[ i - 1 ], segment.floorPoints.rightLine[ i ] );
                            }
                        }
                        if( segment.floorPoints.leftEntranceLeft != null ) {
                            for( int i = 0; i < segment.floorPoints.leftEntranceLeft.Count; i++ ) {
                                if( i > 0 ) {
                                    if( segment.activeSwitch == SwitchDirection.LeftToEntranceLeft ) {
                                        Gizmos.color = Color.green;
                                    }
                                    else {
                                        Gizmos.color = Color.blue;
                                    }
                                    Gizmos.DrawLine( segment.floorPoints.leftEntranceLeft[ i - 1 ], segment.floorPoints.leftEntranceLeft[ i ] );
                                }
                            }
                        }
                        if( segment.floorPoints.rightEntranceRight != null ) {
                            for( int i = 0; i < segment.floorPoints.rightEntranceRight.Count; i++ ) {
                                if( i > 0 ) {
                                    if( segment.activeSwitch == SwitchDirection.RightToEntranceRight ) {
                                        Gizmos.color = Color.green;
                                    }
                                    else {
                                        Gizmos.color = Color.blue;
                                    }
                                    Gizmos.DrawLine( segment.floorPoints.rightEntranceRight[ i - 1 ], segment.floorPoints.rightEntranceRight[ i ] );
                                }
                            }
                        }
                        if( segment.floorPoints.leftExitLeft != null ) {
                            for( int i = 0; i < segment.floorPoints.leftExitLeft.Count; i++ ) {
                                if( i > 0 ) {
                                    if( segment.activeSwitch == SwitchDirection.LeftToExitLeft ) {
                                        Gizmos.color = Color.green;
                                    }
                                    else {
                                        Gizmos.color = Color.red;
                                    }
                                    Gizmos.DrawLine( segment.floorPoints.leftExitLeft[ i - 1 ], segment.floorPoints.leftExitLeft[ i ] );
                                }
                            }
                        }
                        if( segment.floorPoints.rightExitRight != null ) {
                            for( int i = 0; i < segment.floorPoints.rightExitRight.Count; i++ ) {
                                if( i > 0 ) {
                                    if( segment.activeSwitch == SwitchDirection.RightToExitRight ) {
                                        Gizmos.color = Color.green;
                                    }
                                    else {
                                        Gizmos.color = Color.red;
                                    }
                                    Gizmos.DrawLine( segment.floorPoints.rightExitRight[ i - 1 ], segment.floorPoints.rightExitRight[ i ] );
                                }
                            }
                        }
                    }
                }
            }
        }
    }   
}
