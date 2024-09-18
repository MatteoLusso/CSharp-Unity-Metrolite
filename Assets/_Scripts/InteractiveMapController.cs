using UnityEngine;

public class InteractiveMapController : MonoBehaviour
{
    public GameObject ui;
    public Camera mapCamera;
    public GameObject trainIndicator;
    private Vector2 uiStartingPosition;

    public float cameraZoomFactor = 2.0f; 
    public float slowDownFactor = 0.0f;

    private float fixedDeltaTime;

    private float openingElapsedTime = 0.0f;
    private float closingElapsedTime = 0.0f;
    public float zoomInOutTime = 1.0f;
    public float cameraMovementSpeed = 50.0f;
    public float cameraZoomSpeed = 25.0f;
     public float cameraRotationSpeed = 50.0f;

    private Vector3 startingMapPosition; 
    private Vector2 startingMapSize;
    private float startingCameraZoom;
    
    private Vector3 currentMapPosition; 
    private Vector2 currentMapSize;
    private float currentCameraZoom;
    private float currentTimeScale;
    private bool expanded = false;
    private bool opening = false;
    private bool closing = false;

    private InteractiveMapGenerator interactiveMapGenerator;

    private Vector2 expandedMapSize;
    private Vector3 expandedMapPosition;
    private float expandedOrthographicSize;

    private GameObject train;

    private float lineMaterialStartingSpeed = 1.0f;

    private Vector3 cameraDelta = Vector3.zero;
    private Vector3 closingCameraDelta = Vector3.zero;
    private float closingRotationDelta = 0.0f;

    private float zoomDelta = 0.0f;
    private float closingZoomDelta = 0.0f;
    private float rotationDelta = 0.0f;

    private Vector2 closingMapSize = Vector2.zero;
    private Vector3 closingMapPosition = Vector3.zero;
    private float closingCameraZoom = 0.0f;

    public Vector2 zoomRange = new( -15000.0f, 5000.0f );

    private TrainController trainController;
    private CameraController mainCameraController;

    private bool ready = false;

    void Awake()
    {
        this.fixedDeltaTime = Time.fixedDeltaTime;
    }

    public void Initialize( GameManager gm ) {

        this.interactiveMapGenerator = gm.interactiveMapGenerator;
        // this.lineMaterialStartingSpeed = interactiveMapGenerator.lineObjs[ 0 ].material.GetFloat( "_Speed" );

        this.train = gm.instantiatedTrain;
        this.trainController = train.GetComponent<TrainController>();

        this.mainCameraController = gm.instantiatedMainCamera.GetComponent<CameraController>();

        this.startingMapPosition = this.currentMapPosition = this.ui.GetComponent<RectTransform>().position;
        this.startingMapSize = this.currentMapSize = this.ui.GetComponent<RectTransform>().sizeDelta;
        this.startingCameraZoom = this.currentCameraZoom = this.mapCamera.orthographicSize;
        this.currentTimeScale = Time.timeScale = 1.0f;

        this.closingElapsedTime = this.zoomInOutTime;

        this.ui.SetActive( true );
        this.ready = true;
    }

    private void UpdateCamera() {

        this.mapCamera.gameObject.transform.position = this.train.transform.position;
        this.trainIndicator.transform.position = this.train.transform.position + this.interactiveMapGenerator.mapTranslation;
        this.trainIndicator.transform.right = this.train.transform.right;
        Time.fixedDeltaTime = this.fixedDeltaTime * Time.timeScale;
        
        if( Input.GetButtonDown( "LB" ) ) {

            if( !opening && !expanded ) {
                this.openingElapsedTime = zoomInOutTime - this.closingElapsedTime;
                opening = true;
                closing = false;
                expanded = true;
            }
            else if( !closing && expanded ) {
                this.closingElapsedTime = zoomInOutTime - this.openingElapsedTime;
                opening = false;
                closing = true;
                expanded = false;

                this.closingCameraDelta = this.cameraDelta;
                this.closingZoomDelta = this.zoomDelta;

                this.closingMapSize = this.ui.GetComponent<RectTransform>().sizeDelta;
                this.closingMapPosition = this.ui.GetComponent<RectTransform>().position;
                this.closingCameraZoom = this.mapCamera.orthographicSize;
            }

            float shorterSide = Mathf.Min( Screen.width, Screen.height );
            this.expandedMapSize = new( shorterSide - ( uiStartingPosition.x / 2 ), shorterSide - ( uiStartingPosition.x / 2 ) );
            this.expandedMapPosition = new( Screen.width / 2, Screen.height / 2, 0.0f );
            this.expandedOrthographicSize = startingCameraZoom * cameraZoomFactor;

            this.trainController.IgnoreInputs( this.expanded );
            this.mainCameraController.IgnoreInputs( this.expanded );
        }

        if( this.opening ) {

            if( openingElapsedTime > zoomInOutTime ) {
                Time.timeScale = slowDownFactor;

                this.ui.GetComponent<RectTransform>().sizeDelta = this.expandedMapSize;
                this.ui.GetComponent<RectTransform>().position = this.expandedMapPosition;
                this.mapCamera.orthographicSize = this.expandedOrthographicSize; 

                openingElapsedTime = zoomInOutTime;
                opening = false;
            }
            else {
                openingElapsedTime += Time.deltaTime / Time.timeScale;
                currentTimeScale = Time.timeScale = Mathf.Lerp( currentTimeScale, slowDownFactor, openingElapsedTime / zoomInOutTime );

                currentMapSize = this.ui.GetComponent<RectTransform>().sizeDelta = Vector2.Lerp( this.startingMapSize, this.expandedMapSize, openingElapsedTime / zoomInOutTime );
                currentMapPosition = this.ui.GetComponent<RectTransform>().position = Vector3.Lerp( this.startingMapPosition, this.expandedMapPosition, openingElapsedTime / zoomInOutTime  );
                currentCameraZoom = this.mapCamera.orthographicSize = Mathf.Lerp( this.startingCameraZoom, this.expandedOrthographicSize, openingElapsedTime / zoomInOutTime );
            }

            foreach( LineRenderer line in interactiveMapGenerator.lineObjs ) {
                line.material.SetFloat( "_Speed", lineMaterialStartingSpeed / Time.timeScale );
            }
        }
        else if( this.closing ) {
            if( closingElapsedTime > zoomInOutTime ) {
                Time.timeScale = 1.0f;

                this.ui.GetComponent<RectTransform>().sizeDelta = startingMapSize;
                this.ui.GetComponent<RectTransform>().position = startingMapPosition;
                this.mapCamera.orthographicSize = this.startingCameraZoom;

                this.cameraDelta = Vector3.zero;
                this.zoomDelta = 0.0f;

                closingElapsedTime = zoomInOutTime;
                this.closing = false;
            } 
            else {
                closingElapsedTime += Time.deltaTime / Time.timeScale;
                currentTimeScale = Time.timeScale = Mathf.Lerp( currentTimeScale, 1.0f, closingElapsedTime / zoomInOutTime );

                currentMapSize = this.ui.GetComponent<RectTransform>().sizeDelta = Vector2.Lerp( this.expandedMapSize, this.startingMapSize, closingElapsedTime / zoomInOutTime );
                currentMapPosition = this.ui.GetComponent<RectTransform>().position = Vector3.Lerp( this.expandedMapPosition, this.startingMapPosition, closingElapsedTime / zoomInOutTime );
                currentCameraZoom = this.mapCamera.orthographicSize = Mathf.Lerp( this.expandedOrthographicSize, this.startingCameraZoom, closingElapsedTime / zoomInOutTime ) + Mathf.Lerp( this.closingZoomDelta, 0.0f, closingElapsedTime / zoomInOutTime  );

                this.mapCamera.gameObject.transform.position = this.train.transform.position + Vector3.Lerp( this.closingCameraDelta, Vector3.zero, closingElapsedTime / zoomInOutTime  );
            }

            foreach( LineRenderer line in interactiveMapGenerator.lineObjs ) {
                line.material.SetFloat( "_Speed", lineMaterialStartingSpeed / Time.timeScale );
            }
        }
        else if( !this.opening && !this.closing && this.expanded ) {

            if( Mathf.Abs( Input.GetAxis( "Horizontal" ) ) > 0.1f ) {
                cameraDelta += new Vector3( Input.GetAxis( "Horizontal" ) * this.cameraMovementSpeed * Time.deltaTime / Time.timeScale, 0.0f, 0.0f );
            }
            if( Mathf.Abs( Input.GetAxis( "Vertical" ) ) > 0.1f ) {
                cameraDelta += new Vector3( 0.0f, Input.GetAxis( "Vertical" ) * this.cameraMovementSpeed * Time.deltaTime / Time.timeScale, 0.0f );
            }

            Vector4 mapBoundaries = interactiveMapGenerator.mapBoundaries;
            if( cameraDelta.x < mapBoundaries.x ) {
                cameraDelta = new Vector3( mapBoundaries.x, cameraDelta.y, 0.0f );
            }
            else if( cameraDelta.x > mapBoundaries.y ) {
                cameraDelta = new Vector3( mapBoundaries.y, cameraDelta.y, 0.0f );
            }

            if( cameraDelta.y < mapBoundaries.z ) {
                cameraDelta = new Vector3( cameraDelta.x, mapBoundaries.z, 0.0f );
            }
            else if( cameraDelta.y > mapBoundaries.w ) {
                cameraDelta = new Vector3( cameraDelta.x, mapBoundaries.w, 0.0f );
            }

            if( Mathf.Abs(  Input.GetAxis( "LT" ) ) > 0.1f ) {
                zoomDelta += Input.GetAxis( "LT" ) * this.cameraZoomSpeed * Time.deltaTime / Time.timeScale;
            }
            if( Mathf.Abs(  Input.GetAxis( "RT" ) ) > 0.1f ) {
                zoomDelta += Input.GetAxis( "RT" ) * -this.cameraZoomSpeed * Time.deltaTime / Time.timeScale;
            }
            zoomDelta = Mathf.Clamp( zoomDelta, zoomRange.x, zoomRange.y );
            
            this.mapCamera.transform.position += cameraDelta;
            this.mapCamera.orthographicSize = this.expandedOrthographicSize + zoomDelta;
        }
    }

    public static Vector3? CalculateFocusPoint( Vector3 dir, Vector3 p0, Vector3 pp, Vector3 np )
    {
        // Calcola il denominatore
        float denominator = Vector3.Dot( np, dir );

        // Se il denominatore è 0, la retta è parallela al piano
        if( Mathf.Abs( denominator ) < Mathf.Epsilon )
        {
            Debug.Log( "La retta è parallela al piano, nessuna intersezione." );
            return null;
        }

        // Calcola il numeratore
        float t = Vector3.Dot( np, pp - p0 ) / denominator;

        // Calcola il punto di intersezione
        return p0 + t * dir;
    }

    private void LateUpdate() {

        if( this.ready ) {
            UpdateCamera();
        }
    }
}
