using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public MetroGenerator metroGenerator;
    public MaintenanceGenerator maintenanceGenerator;
    
    public MapController mapController;

    public bool instantiateMap = true;
    public bool instantiateTrain = true;
    public bool instantiateMainCamera = true;

    public GameObject mainCamera;
    public GameObject train;
    public float trainHeightFromGround;

    void Awake()
    {
        StartCoroutine( InitializeGame() );
    }


    IEnumerator InitializeGame() {

        metroGenerator.GenerateMetro( /*this*/ );
        while( !metroGenerator.ready ) {
            yield return new WaitForEndOfFrame();
        }

        maintenanceGenerator.GenerateMaintenance( metroGenerator );
        while( !maintenanceGenerator.ready ) {
            yield return new WaitForEndOfFrame();
        }

        if( instantiateTrain ) {
            InstantiateTrain();

            if( instantiateMainCamera ) {
                InstantiateMainCamera();
            }
        }

        // if( instantiateMap ) {
            
        //     mapController.

        //     while( !mapController.ready ) {
        //         yield return new WaitForEndOfFrame();
        //     }
        // }
        
    }

    private void InstantiateTrain() {
        Vector3 trainPos = metroGenerator.lines[ "Linea 0" ][ 0 ].bezierCurveLimitedAngle[ 0 ];
        trainPos.z += this.trainHeightFromGround;
        Vector3 trainDir = metroGenerator.lines[ "Linea 0" ][ 0 ].bezierCurveLimitedAngle[ 1 ] - metroGenerator.lines[ "Linea 0" ][ 0 ].bezierCurveLimitedAngle[ 0 ];
        GameObject instantiatedTrain = Instantiate( this.train, trainPos, Quaternion.Euler( 0.0f, 0.0f, Vector3.SignedAngle( Vector3.right, trainDir, Vector3.forward ) ) );
        instantiatedTrain.name = "Train";
    }

    private void InstantiateMainCamera() {
        Vector3 trainPos = metroGenerator.lines[ "Linea 0" ][ 0 ].bezierCurveLimitedAngle[ 0 ];
        trainPos.z += metroGenerator.trainHeightFromGround;
        mainCamera.tag = "MainCamera";
        Instantiate( mainCamera, trainPos, Quaternion.identity );
    }
}
