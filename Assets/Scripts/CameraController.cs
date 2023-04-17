using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private GameObject train = null;
    public Vector3 rotation;

    public float lookingDistance = -100.0f;
    public float smoothTime = 1.0f;
    public float timeElapsed = 0.0f;
    public float horizontalAxisRotationSpeed = 1.0f;
    public float horizontalAxisDeadZone = 0.1f;
    public float horizontalButtonRotationSpeed = 1.0f;


    public Vector3 offset;

    public Vector3 velocity;

    Vector3 newPos = Vector3.zero;
    float smoothDelta = 0.0f;
    Vector3 prevPos = Vector3.zero;

    void LateUpdate() {

        if(Input.GetKey( KeyCode.Escape ) )
        {
            Application.Quit();
        }
        
        if( train == null ) {
            train = GameObject.Find( "Train" );
        }
        else {

            if( Input.GetKey( KeyCode.Q ) ) {
                rotation.z -= horizontalButtonRotationSpeed * Time.deltaTime;
                //rotation.z = rotation.z - ( ( int )( rotation.z / 360.0f ) * 360.0f );
            }
            else if( Input.GetKey( KeyCode.E ) ) {
                rotation.z += horizontalButtonRotationSpeed * Time.deltaTime;
                //rotation.z = rotation.z + ( ( int )( rotation.z / 360.0f ) * 360.0f );
            }

            if( ( Mathf.Abs( Input.GetAxis( "Horizontal" ) ) > horizontalAxisDeadZone ) && !Input.GetKey( KeyCode.A ) && !Input.GetKey( KeyCode.D ) ) {
                rotation.z += horizontalAxisRotationSpeed * Input.GetAxis( "Horizontal" ) * Time.deltaTime;
                //rotation.z = rotation.z - ( ( int )( rotation.z / 360.0f ) * 360.0f );
            }

            newPos = train.transform.position + Quaternion.Euler( rotation.x, rotation.y, rotation.z ) * ( offset + -Vector3.forward );

            this.transform.position = Vector3.SmoothDamp( this.transform.position, newPos, ref velocity, smoothTime );
            //this.transform.position = newPos;
            this.transform.LookAt( train.transform, -Vector3.forward );

            prevPos = newPos;
        }

    }

    private void OnDrawGizmos() { 
        Gizmos.color = Color.green;
        Gizmos.DrawLine( this.transform.position, train.transform.position );
        Gizmos.color = Color.red;
        Gizmos.DrawLine( newPos, train.transform.position );
        Gizmos.DrawSphere( newPos, smoothDelta );
    }
}
