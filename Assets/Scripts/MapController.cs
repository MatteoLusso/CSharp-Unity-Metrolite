using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    private class MapPoint {
        public Vector3 coords { get; set; }
        public Type type { get; set; }
        public string lineName { get; set; }
        public int index { get; set; }
    }

    public GameObject ui;
    public GameObject mapTrain;

    public float lineWidth = 400.0f;
    public float lineBackgroundWidth = 600.0f;
    public float stationSize = 800.0f;
    public float stationBackgroundSize = 1200.0f;
    public float switchSize = 200.0f;

    private MetroGenerator metroData;
    private bool ready = false;

    private Dictionary<int, Vector3> lineStarts = new Dictionary<int, Vector3>();
    private Dictionary<string, List<MapPoint>> linesMap = new Dictionary<string, List<MapPoint>>();
    private Dictionary<string, Color> linesColors = new Dictionary<string, Color>();
    private HashSet<Color> usedLinesColors = new HashSet<Color>();
    private Dictionary<string, LineRenderer> linesRenders = new Dictionary<string, LineRenderer>();

    private GameObject train;
    //private bool zoomingMap = false;
    private Vector2 uiStartingSize;
    private Vector2 uiStartingPosition;

    public float cameraZoomFactor = 2.0f; 
    public float slowDownFactor = 0.0f;

    private float fixedDeltaTime;

    private int mapIndex = 0;

    public float lightWaveSpeed = 1.0f;

    private Color startingMapTrainColor;

    private float elapsedTime = 0.0f;
    public float zoomInTime = 1.0f;

    private Vector3 startingMapPosition; 
    private Vector2 startingMapSize;
    private float startingCameraZoom;
    
    private Vector3 currentMapPosition; 
    private Vector2 currentMapSize;
    private float currentCameraZoom;
    private float currentTimeScale;

    private bool expand = true;

    private GameObject uiMapObj;

    // private AudioSource[] audioSources;
    // private List<float> audioSourcesPitches;


    void Awake()
    {
        this.fixedDeltaTime = Time.fixedDeltaTime;
    }

    private void Start() {
        metroData = GameObject.Find( "MetroGenerator" ).GetComponent<MetroGenerator>();
        uiMapObj =  GameObject.Find( "UIMap" );
    }

    private void LateUpdate()
    {
        if( metroData.ready && !this.ready ) {
            GenerateMap();

            uiStartingSize = ui.GetComponent<RectTransform>().sizeDelta;
            uiStartingPosition = ui.GetComponent<RectTransform>().position;

            train = GameObject.Find( "Train" );
            mapTrain = GameObject.Find( "MapTrain" );
            this.startingMapTrainColor = mapTrain.GetComponent<Renderer>().material.color;

            this.startingMapPosition = this.ui.GetComponent<RectTransform>().position;
            this.startingMapSize = this.ui.GetComponent<RectTransform>().sizeDelta;
            this.startingCameraZoom = this.GetComponent<Camera>().orthographicSize;

            // this.audioSources = FindObjectsOfType<AudioSource>();
        }

        if( this.ready ) {

            if( Input.GetButtonDown( "LB" ) ) {
                elapsedTime = 0.0f;
                expand = true;
                //Time.timeScale = slowDownFactor;
            }
            else if( Input.GetButton( "LB" ) ) {
                if( expand ) {
                    elapsedTime += Time.deltaTime;
                    float shorterSide = Mathf.Min( Screen.width, Screen.height );

                    Time.timeScale = Mathf.Lerp( 1.0f, slowDownFactor, ( elapsedTime / zoomInTime ) / Time.timeScale );

                    this.ui.GetComponent<RectTransform>().sizeDelta = Vector2.Lerp( startingMapSize, new Vector2( shorterSide - ( uiStartingPosition.x / 2 ), shorterSide - ( uiStartingPosition.x / 2 ) ), ( elapsedTime / zoomInTime ) / Time.timeScale );
                    this.ui.GetComponent<RectTransform>().position = Vector3.Lerp( startingMapPosition, new Vector3( Screen.width / 2, Screen.height / 2, 0.0f ), ( elapsedTime / zoomInTime ) / Time.timeScale  );
                    this.GetComponent<Camera>().orthographicSize = Mathf.Lerp( startingCameraZoom, startingCameraZoom * cameraZoomFactor, ( elapsedTime / zoomInTime ) / Time.timeScale );

                    // foreach( AudioSource audioSource in this.audioSources ) {
                    //     Debug.Log( audioSource.name + " pitch: " + audioSource.pitch );
                    //     audioSource.pitch *= Time.timeScale;
                    // }
                }
            }
            else if( Input.GetButtonUp( "LB" ) ) {
                
                expand = false;
                elapsedTime = 0.0f;

                currentMapPosition = this.ui.GetComponent<RectTransform>().position;
                currentMapSize = this.ui.GetComponent<RectTransform>().sizeDelta;
                currentCameraZoom = this.GetComponent<Camera>().orthographicSize;
                currentTimeScale = Time.timeScale;

                // this.ui.GetComponent<RectTransform>().sizeDelta = uiStartingSize;
                // this.ui.GetComponent<RectTransform>().position = uiStartingPosition;

                // this.GetComponent<Camera>().orthographicSize /= cameraZoomFactor;

                // Time.timeScale = 1.0f;
            }
            else if( !expand ) {
                elapsedTime += Time.deltaTime;
                float shorterSide = Mathf.Min( Screen.width, Screen.height );

                Time.timeScale = Mathf.Lerp( currentTimeScale, 1.0f, ( elapsedTime / zoomInTime ) / Time.timeScale );

                this.ui.GetComponent<RectTransform>().sizeDelta = Vector2.Lerp( currentMapSize, startingMapSize, ( elapsedTime / zoomInTime ) / Time.timeScale );
                this.ui.GetComponent<RectTransform>().position = Vector3.Lerp( currentMapPosition, startingMapPosition, ( elapsedTime / zoomInTime ) / Time.timeScale  );
                this.GetComponent<Camera>().orthographicSize = Mathf.Lerp( currentCameraZoom, startingCameraZoom, ( elapsedTime / zoomInTime ) / Time.timeScale );

                // foreach( AudioSource audioSource in this.audioSources ) {
                //     audioSource.pitch *= Time.timeScale;
                // }
            }

            Time.fixedDeltaTime = this.fixedDeltaTime * Time.timeScale;

            int trainIndex = train.GetComponent<TrainController>().indexSection;

            if( trainIndex == 0 ) {
                mapIndex = 0;
            }
            else if( metroData.lines[ train.GetComponent<TrainController>().keyLine ][ train.GetComponent<TrainController>().indexSection ].switchType == SwitchType.BiToNewBi || metroData.lines[ train.GetComponent<TrainController>().keyLine ][ train.GetComponent<TrainController>().indexSection ].switchType == SwitchType.MonoToNewMono ) {

                foreach( MapPoint mapPoint in linesMap[ train.GetComponent<TrainController>().keyLine ] ) {
                    if( mapPoint.index == train.GetComponent<TrainController>().indexSection ) {
                        mapIndex = linesMap[ train.GetComponent<TrainController>().keyLine ].IndexOf( mapPoint );
                        break;
                    }
                }
            }

            if( mapIndex + 1 < linesMap[ train.GetComponent<TrainController>().keyLine ].Count ) {
                int previousTrainIndex = linesMap[ train.GetComponent<TrainController>().keyLine ][ mapIndex ].index;
                int nextTrainIndex = linesMap[ train.GetComponent<TrainController>().keyLine ][ mapIndex + 1 ].index;

                if( nextTrainIndex < trainIndex ) {
                    mapIndex++;
                    previousTrainIndex = linesMap[ train.GetComponent<TrainController>().keyLine ][ mapIndex ].index;
                    nextTrainIndex = linesMap[ train.GetComponent<TrainController>().keyLine ][ mapIndex + 1 ].index;
                }

                float mapPerc = ( float )( trainIndex - previousTrainIndex ) / ( float )( nextTrainIndex - previousTrainIndex );
                // Debug.Log( "trainIndex: " + trainIndex );
                // Debug.Log( "previousTrainIndex: " + previousTrainIndex );
                // Debug.Log( "nextTrainIndex: " + nextTrainIndex );
                // Debug.Log( "mapPerc: " + mapPerc );
                // Debug.Log( "linesMap[ train.GetComponent<TrainController>().keyLine ][ mapIndex ].coords: " + linesMap[ train.GetComponent<TrainController>().keyLine ][ mapIndex ].coords );
                // Debug.Log( "linesMap[ train.GetComponent<TrainController>().keyLine ][ mapIndex + 1 ].coords: " + linesMap[ train.GetComponent<TrainController>().keyLine ][ mapIndex + 1 ].coords );

                Vector3 newPos = linesMap[ train.GetComponent<TrainController>().keyLine ][ mapIndex ].coords + ( ( linesMap[ train.GetComponent<TrainController>().keyLine ][ mapIndex + 1 ].coords - linesMap[ train.GetComponent<TrainController>().keyLine ][ mapIndex ].coords ).normalized * ( linesMap[ train.GetComponent<TrainController>().keyLine ][ mapIndex + 1 ].coords - linesMap[ train.GetComponent<TrainController>().keyLine ][ mapIndex ].coords ).magnitude * mapPerc );
                mapTrain.transform.position = newPos;
                this.transform.position = newPos;
            }

            this.mapTrain.GetComponent<Renderer>().material.SetColor( "_EmissionColor", Color.Lerp( Color.black, startingMapTrainColor, Mathf.PingPong( Time.time, lightWaveSpeed ) ) );
            this.mapTrain.GetComponent<Renderer>().material.EnableKeyword( "_EMISSION" );
        }
    }

    private void GenerateMap() {
        
        foreach( string lineName in metroData.lines.Keys ) { 
            
            bool mainLine = true;
            Vector3 start = Vector3.zero;
            if( metroData.lines[ lineName ][ 0 ].fromSection != null ) {
                start = lineStarts[ metroData.lines[ lineName ][ 0 ].fromSection.GetHashCode() ];
                mainLine = false;
            }
            Color lineColor = new Color( Random.Range( 0.0f, 1.0f ), Random.Range( 0.0f, 1.0f ), Random.Range( 0.0f, 1.0f ) );
            while( usedLinesColors.Contains( lineColor ) ) {
                lineColor = new Color( Random.Range( 0.0f, 1.0f ), Random.Range( 0.0f, 1.0f ), Random.Range( 0.0f, 1.0f ) );
            }
            usedLinesColors.Add( lineColor );
            linesColors.Add( lineName, lineColor );

            List<Vector3> coords = new List<Vector3>();

            GameObject mapLineObj = new GameObject( "Mappa " + lineName );
            mapLineObj.transform.parent = uiMapObj.transform;
            mapLineObj.AddComponent<LineRenderer>();
            mapLineObj.layer = LayerMask.NameToLayer( "Map" );
            LineRenderer lineRender = mapLineObj.GetComponent<LineRenderer>();
            lineRender.useWorldSpace = false;

            GameObject mapLineBackgroundObj = new GameObject( "Mappa Background" + lineName );
            mapLineBackgroundObj.transform.Translate( 0.0f, 0.0f, 0.05f );
            mapLineBackgroundObj.transform.parent = uiMapObj.transform;
            mapLineBackgroundObj.AddComponent<LineRenderer>();
            mapLineBackgroundObj.layer = LayerMask.NameToLayer( "Map" );
            LineRenderer lineRenderBackground = mapLineBackgroundObj.GetComponent<LineRenderer>();
            lineRenderBackground.useWorldSpace = false;

            if( !mainLine ) {
                 mapLineObj.transform.Translate( 0.0f, 0.0f, 0.01f );
            }

            //Debug.Log( ">>> linename: " + lineName );

            for( int i = 0; i < metroData.lines[ lineName ].Count; i++ ) {

                MapPoint mapPoint = new MapPoint();
                mapPoint.index = i;
                mapPoint.lineName = lineName;

                if( metroData.lines[ lineName ][ i ].type == Type.Switch ) {

                    mapPoint.coords = metroData.lines[ lineName ][ i ].centerCoords;
                    if( metroData.lines[ lineName ][ i ].switchType == SwitchType.BiToNewBi || metroData.lines[ lineName ][ i ].switchType == SwitchType.MonoToNewMono ) {

                        lineStarts.Add( metroData.lines[ lineName ][ i ].GetHashCode(), mapPoint.coords );
                    }

                    coords.Add( mapPoint.coords );
                    mapPoint.type = Type.Switch;

                    //linesMap[ lineName ].Add( mapPoint );
                    if( linesMap.ContainsKey( lineName ) ) {
                        linesMap[ lineName ].Add( mapPoint );
                    }
                    else {
                        linesMap.Add( lineName, new List<MapPoint>{  mapPoint } );
                    }

                    GameObject switchIndicatorObj = GameObject.CreatePrimitive( PrimitiveType.Sphere );
                    switchIndicatorObj.transform.parent = GameObject.Find( lineName ).transform;
                    switchIndicatorObj.transform.position = mapPoint.coords;
                    switchIndicatorObj.name = "Indicatore scambio";
                    switchIndicatorObj.layer = LayerMask.NameToLayer( "Map" );
                    switchIndicatorObj.transform.localScale = new Vector3( switchSize, switchSize, 1.0f );
                    switchIndicatorObj.GetComponent<Renderer>().material.SetColor( "_EmissionColor", Color.white );
                    switchIndicatorObj.GetComponent<Renderer>().material.EnableKeyword( "_EMISSION" );
                    
                }
                else if( metroData.lines[ lineName ][ i ].type == Type.Station ) {

                    mapPoint.coords = metroData.lines[ lineName ][ i ].centerCoords;
                    coords.Add( mapPoint.coords );
                    mapPoint.type = Type.Station;
                    
                    // linesMap[ lineName ].Add( mapPoint );
                    if( linesMap.ContainsKey( lineName ) ) {
                        linesMap[ lineName ].Add( mapPoint );
                    }
                    else {
                        linesMap.Add( lineName, new List<MapPoint>{  mapPoint } );
                    }

                    GameObject stationIndicatorObj = GameObject.CreatePrimitive( PrimitiveType.Sphere );
                    stationIndicatorObj.transform.parent = GameObject.Find( lineName ).transform;
                    stationIndicatorObj.transform.position = new Vector3( mapPoint.coords.x, mapPoint.coords.y, -1.0f );
                    stationIndicatorObj.name = "Indicatore stazione";
                    stationIndicatorObj.layer = LayerMask.NameToLayer( "Map" );
                    stationIndicatorObj.transform.localScale = new Vector3( stationSize, stationSize, 1.0f );
                    stationIndicatorObj.GetComponent<Renderer>().material.color = linesColors[ lineName ];
                    stationIndicatorObj.GetComponent<Renderer>().material.SetColor( "_EmissionColor", linesColors[ lineName ] );
                    stationIndicatorObj.GetComponent<Renderer>().material.EnableKeyword( "_EMISSION" );

                    GameObject stationIndicatorBackgroundObj = GameObject.CreatePrimitive( PrimitiveType.Sphere );
                    stationIndicatorBackgroundObj.transform.parent = GameObject.Find( lineName ).transform;
                    stationIndicatorBackgroundObj.transform.position = mapPoint.coords;
                    stationIndicatorBackgroundObj.name = "Indicatore stazione background";
                    stationIndicatorBackgroundObj.layer = LayerMask.NameToLayer( "Map" );
                    stationIndicatorBackgroundObj.transform.localScale = new Vector3( stationBackgroundSize, stationBackgroundSize, 1.0f );
                    stationIndicatorBackgroundObj.GetComponent<Renderer>().material.SetColor( "_EmissionColor", Color.white );
                    stationIndicatorBackgroundObj.GetComponent<Renderer>().material.EnableKeyword( "_EMISSION" );
                }
                else if( metroData.lines[ lineName ][ i ].type == Type.Tunnel ) {

                    mapPoint.type = Type.Tunnel;

                    if( metroData.lines[ lineName ].IndexOf( metroData.lines[ lineName ][ i ] ) == 0 ) {
                        if( metroData.lines[ lineName ][ 0 ].fromSection != null ) { 
                            mapPoint.coords = start;
                        }
                        else {
                            mapPoint.coords = metroData.lines[ lineName ][ i ].bezierCurveLimitedAngle[ 0 ];
                        }
                        coords.Add( mapPoint.coords );
                        
                        // linesMap[ lineName ].Add( mapPoint );
                        if( linesMap.ContainsKey( lineName ) ) {
                            linesMap[ lineName ].Add( mapPoint );
                        }
                        else {
                            linesMap.Add( lineName, new List<MapPoint>{  mapPoint } );
                        }

                        //Debug.Log( "MapPoint.coords: " + mapPoint.coords );
                    }
                    else if( metroData.lines[ lineName ].IndexOf( metroData.lines[ lineName ][ i ] ) == metroData.lines[ lineName ].Count - 1 ) {
                        mapPoint.coords = metroData.lines[ lineName ][ i ].bezierCurveLimitedAngle[ metroData.lines[ lineName ][ i ].bezierCurveLimitedAngle.Count - 1 ];
                        coords.Add( mapPoint.coords );
                        
                        // linesMap[ lineName ].Add( mapPoint );
                        if( linesMap.ContainsKey( lineName ) ) {
                            linesMap[ lineName ].Add( mapPoint );
                        }
                        else {
                            linesMap.Add( lineName, new List<MapPoint>{  mapPoint } );
                        }
                    }
                    else if( i > 0 && metroData.lines[ lineName ][ i ].mainDir != metroData.lines[ lineName ][ i - 1 ].mainDir ) {
                        mapPoint.coords = metroData.lines[ lineName ][ i ].bezierCurveLimitedAngle[ 0 ] + ( ( metroData.lines[ lineName ][ i ].bezierCurveLimitedAngle[ metroData.lines[ lineName ][ i ].bezierCurveLimitedAngle.Count - 1 ] - metroData.lines[ lineName ][ i ].bezierCurveLimitedAngle[ 0 ] ).normalized * ( ( metroData.lines[ lineName ][ i ].bezierCurveLimitedAngle[ metroData.lines[ lineName ][ i ].bezierCurveLimitedAngle.Count - 1 ] - metroData.lines[ lineName ][ i ].bezierCurveLimitedAngle[ 0 ] ).magnitude / 2 ) );
                        coords.Add( mapPoint.coords );
                        
                        // linesMap[ lineName ].Add( mapPoint );
                        if( linesMap.ContainsKey( lineName ) ) {
                            linesMap[ lineName ].Add( mapPoint );
                        }
                        else {
                            linesMap.Add( lineName, new List<MapPoint>{  mapPoint } );
                        }

                        //Debug.Log( "MapPoint.coords: " + mapPoint.coords );
                    }
                }
            }

            lineRender.positionCount = lineRenderBackground.positionCount = coords.Count;
            lineRender.SetPositions( coords.ToArray() );
            lineRenderBackground.SetPositions( coords.ToArray() );
            lineRender.material = lineRenderBackground.material = new Material( Shader.Find( "Sprites/Default" ) );
            lineRender.startColor = lineRender.endColor = linesColors[ lineName ];
            lineRenderBackground.startColor = lineRenderBackground.endColor = Color.white;
            //lineRender.numCornerVertices = lineRenderBackground.numCornerVertices = 1;
            lineRender.startWidth = lineRender.endWidth = lineWidth;
            lineRenderBackground.startWidth = lineRenderBackground.endWidth = lineBackgroundWidth;
        }

        this.ready = true;
    }

    private void OnDrawGizmos() {
        if( this.ready ) {
            foreach( string lineName in linesMap.Keys ){ 
                for( int i = 1; i < linesMap[ lineName ].Count; i++ ) {
                    Gizmos.color = linesColors[ lineName ];

                    //Gizmos.DrawLine( linesMap[ lineName ][ i ].coords, linesMap[ lineName ][ i - 1 ].coords );
                    if( linesMap[ lineName ][ i ].type == Type.Station ) {
                        Gizmos.DrawSphere( linesMap[ lineName ][ i ].coords, 150.0f );
                    }
                    else if( linesMap[ lineName ][ i ].type == Type.Switch ) {
                        Gizmos.DrawCube( linesMap[ lineName ][ i ].coords, Vector3.one * 150.0f );
                    }
                }
            }
        }
    }
}
