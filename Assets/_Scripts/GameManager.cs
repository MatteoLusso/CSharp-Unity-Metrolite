using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public MetroGenerator metroGenerator;
    public MaintenanceGenerator maintenanceGenerator;
    public InteractiveMapGenerator interactiveMapGenerator;
    public InteractiveMapController interactiveMapController;
    public NavMeshGenerator navMeshGenerator;
    
    public MapController mapController;

    public Slider loadingBar;
    public Image loadingBackground;
    public TMP_Text loadingText;
    public float loadingFadingTime = 2.0f;
    private float loadingBarNextValue = 0.0f;

    public bool instantiateMap = true;
    public bool instantiateTrain = true;
    public bool instantiateMainCamera = true;

    public GameObject train;
    public float trainHeightFromGround;


    public GameObject instantiatedTrain;
    public GameObject instantiatedMainCamera;

    public float metroGenerationMaxProgress = 0.5f;
    public float metroGenerationPercent = 0.0f;
    public float maintenanceGenerationMaxProgress = 0.4f;
    public float maintenanceGenerationPercent = 0.0f;

    void Awake()
    {
        StartCoroutine( InitializeGame() );
    }


    IEnumerator InitializeGame() {
        this.loadingBarNextValue = 0.0f;
        StartCoroutine( HandleLoading() );

        this.loadingText.text = "Generating metro lines";
        StartCoroutine( metroGenerator.GenerateMetro( this ) );
        while( !metroGenerator.ready ) {
            this.loadingBarNextValue = this.metroGenerationMaxProgress * this.metroGenerationPercent;
            yield return new WaitForEndOfFrame();
        }

        this.loadingText.text = "Generating maintenance tunnels";
        StartCoroutine( maintenanceGenerator.GenerateMaintenance( this ) );
        while( !maintenanceGenerator.ready ) {
            this.loadingBarNextValue = this.metroGenerationMaxProgress + ( this.maintenanceGenerationMaxProgress * this.maintenanceGenerationPercent );
            yield return new WaitForEndOfFrame();
        }

        this.loadingText.text = "Instantiating props";
        if( instantiateTrain ) {
            InstantiateTrain();
            this.loadingBarNextValue = 0.92f;
            yield return new WaitForEndOfFrame();
        }

        if( instantiateMap ) {
            interactiveMapGenerator.GenerateMap( this );
            this.loadingBarNextValue = 0.96f;
            yield return new WaitForEndOfFrame();

            interactiveMapController.Initialize( this );
            this.loadingBarNextValue = 0.98f;
            yield return new WaitForEndOfFrame();
        }

        this.loadingText.text = "Generating navigation mesh";
        navMeshGenerator.GenerateNavMesh();

        InitializeMainCamera();
        this.loadingBarNextValue = 1.0f;
    }


    private void ResetSwitchRails() {
        foreach( GameObject switchRail in metroGenerator.switchRails ) {
            switchRail.GetComponent<RailHighlighter>().ready = true;
        }
    }

    private IEnumerator HandleLoading() { 

        while( this.loadingBarNextValue < 1.0f ) {

            this.loadingBar.value = this.loadingBarNextValue;

            yield return new WaitForEndOfFrame();
        }

        this.loadingBar.value = 1.0f;

        this.loadingBar.gameObject.SetActive( false );
        this.loadingText.gameObject.SetActive( false );

        float elapsedTime = 0.0f;
        
        while( elapsedTime < this.loadingFadingTime ) {

            elapsedTime += Time.deltaTime / Time.timeScale;

            this.loadingBackground.color = Color.Lerp( new Vector4( 0.0f, 0.0f, 0.0f, 1.0f ), new Vector4( 0.0f, 0.0f, 0.0f, 0.0f ), elapsedTime / this.loadingFadingTime );

            yield return new WaitForEndOfFrame();
        }

        this.loadingBackground.gameObject.SetActive( false );
    }

    private void InstantiateTrain() {
        Vector3 trainPos = metroGenerator.lines[ "Linea 0" ][ 0 ].bezierCurveLimitedAngle[ 0 ];
        trainPos.z += this.trainHeightFromGround;
        Vector3 trainDir = metroGenerator.lines[ "Linea 0" ][ 0 ].bezierCurveLimitedAngle[ 1 ] - metroGenerator.lines[ "Linea 0" ][ 0 ].bezierCurveLimitedAngle[ 0 ];
        this.instantiatedTrain = Instantiate( this.train, trainPos, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, trainDir, Vector3.forward ) ) );
        this.instantiatedTrain.name = "Train";

        ResetSwitchRails();
    }

    private void InitializeMainCamera() {
        Vector3 trainPos = metroGenerator.lines[ "Linea 0" ][ 0 ].bezierCurveLimitedAngle[ 0 ];
        trainPos.z += metroGenerator.trainHeightFromGround;
        this.instantiatedMainCamera.tag = "MainCamera";
        this.instantiatedMainCamera.name = "MainCamera";

        this.instantiatedMainCamera.GetComponent<CameraController>().UpdateCamera( trainPos, instantiatedTrain.transform );
        this.instantiatedMainCamera.GetComponent<CameraController>().ready = true;


    }
}
