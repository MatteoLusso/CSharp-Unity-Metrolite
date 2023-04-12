using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainController : MonoBehaviour
{
    private enum Direction
    {
        None,
        Forward,
        Backward,
    }

    public float speed = 0.0f;
    public float maxSpeed = 10.0f;
    public float acceleration = 2.5f;
    public float curveDrag = 0.1f;
    public float drag = 0.1f;
    public float smooth = 1.0f;

    private Dictionary<string, List<LineSection>> lines;
    private AudioSource noise;
    private AudioSource braking;

    private bool goFowardActive = false;
    private bool goBackwardActive = false;
    private float brakingNoiseDecreasingSpeed = 2.0f;
    private float previousSpeed = 0.0f;
    private Direction mainDir = Direction.None;

    private string keyLine = "Linea 1";
    private int indexPoint = 0;
    private int indexSection = 0;

    private float heightCorrection;

    // Start is called before the first frame update
    void Start()
    {
        AudioSource[] audios = this.transform.GetComponents<AudioSource>();
        foreach( AudioSource audio in audios ) {
            if( audio.clip.name.Equals( "Train Noise" ) ) {
                noise = audio;
                noise.Stop();
                noise.loop = true;
            }
            else if( audio.clip.name.Equals( "Train Brakes" ) ) {
                braking = audio;
                braking.Stop();
                braking.loop = true;
            }
        }

        FullLineGenerator lineGen = GameObject.Find( "LineGenerator" ).GetComponent<FullLineGenerator>();
        lines = lineGen.lineMap;
        heightCorrection = lineGen.trainHeightFromGround;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        HandleMovement();
        HandleBrakingNoise();
    }

    private void HandleMovement() {
        if( speed > 1f && !goFowardActive )
        {
            noise.Play();
            StartCoroutine( "GoForward2" );
            goFowardActive = true;
        }
        else if( speed <= 1f && goFowardActive) {
            noise.Stop();
            StopCoroutine( "GoForward2" );
            goFowardActive = false;
        }

        if( speed < -1f && !goBackwardActive )
        {
            noise.Play();
            StartCoroutine( "GoBackward2" );
            goBackwardActive = true;
        }
        else if( speed >= -1f && goBackwardActive) {
            noise.Stop();
            StopCoroutine( "GoBackward2" );
            goBackwardActive = false;
        }

        if(Input.GetKey( KeyCode.D ) ) {
            speed = Mathf.Lerp( speed, speed + acceleration, smooth * Time.fixedDeltaTime );
            if( speed > maxSpeed ) {
                speed = maxSpeed;
            }
        }
        if(Input.GetKey( KeyCode.A ) ) {
             speed = Mathf.Lerp( speed, speed - acceleration, smooth * Time.fixedDeltaTime );
            if( speed < -maxSpeed ) {
                speed = -maxSpeed;
            }
        }
    }

    private void HandleBrakingNoise() {
        if( Mathf.Abs( previousSpeed ) > Mathf.Abs( speed ) ) {

            if( !braking.isPlaying ) {
                Debug.Log( "Braking" );
                braking.volume = Mathf.Abs( speed ) / maxSpeed;
                braking.time = 0.0f;
                braking.Play();
            }
            else {
                braking.volume = Mathf.Lerp( braking.volume, Mathf.Abs( speed ), brakingNoiseDecreasingSpeed * Time.fixedDeltaTime );
            }
        }
        else if( Mathf.Abs( previousSpeed ) <= Mathf.Abs( speed ) ) {

            braking.volume = Mathf.Lerp( braking.volume, 0.0f, brakingNoiseDecreasingSpeed * Time.fixedDeltaTime );

            if( braking.isPlaying && braking.volume <= 0.0f ) {
                braking.Stop();
            }
        }
        previousSpeed = speed;
    }

    IEnumerator GoBackward()
    {

        //Debug.Log( "GoBackward started" );
        //foreach( string lineName in lines.Keys ) {
            List<LineSection> sections = lines[ keyLine ];

            for( int i = indexSection; i >= 0; i-- ) {

                //Debug.Log( "Section: " + i );

                List<Vector3> coords = sections[ i ].bezierCurveLimitedAngle;

                while( Mathf.Abs( speed ) < ( coords[ 0 ] - this.transform.position ).magnitude ) {

                    noise.pitch = Mathf.Abs( speed ) / maxSpeed;
                    
                    Vector3 nearestPoint = Vector3.zero;

                    for( int j = indexPoint; j >= 0; j-- ) {

                        //Debug.Log( "Point: " + j );
                        
                        nearestPoint = coords[ j ];
                        nearestPoint.z += heightCorrection;
                        if( ( nearestPoint - this.transform.position ).magnitude > Mathf.Abs( speed ) && j >= 0 ) { 
                            indexPoint = j;
                            break;
                        }
                    }

                    //Debug.Log( "nearestPoint: " + nearestPoint + " | this.transform.position: " + this.transform.position );
                    Debug.DrawLine( this.transform.position, nearestPoint, Color.red, Time.deltaTime );

                    this.transform.position = Vector3.Lerp( this.transform.position, nearestPoint, Mathf.Abs( speed ) * Time.deltaTime );
                    //float losedSpeed = curveDrag * Vector3.Angle( this.transform.right, nearestPoint - this.transform.position ) / 360.0f;
                    this.transform.right = Vector3.Lerp( this.transform.right, this.transform.position - nearestPoint, Mathf.Abs( speed ) * Time.deltaTime );
                    //speed += losedSpeed;

                    yield return new WaitForEndOfFrame();
                }

                if( i >= 0 ) {
                    indexSection--;
                    indexPoint = sections[ indexSection ].bezierCurveLimitedAngle.Count - 1;
                }
            }
        //}
    }

    IEnumerator GoForward()
    {
        //Debug.Log( "GoForward started" );
        //foreach( string lineName in lines.Keys ) {
            List<LineSection> sections = lines[ keyLine ];

            for( int i = indexSection; i < sections.Count; i++ ) {

                //Debug.Log( "Section: " + i );

                List<Vector3> coords = sections[ i ].bezierCurveLimitedAngle;
                float sectionLength = 0.0f;
                for( int k = 1; k < coords.Count; k++ ) {
                    sectionLength += ( coords[ k ] - coords[ k - 1 ] ).magnitude;
                }
                
                float segmentLength = sectionLength / coords.Count;

                while( Mathf.Abs( speed ) < ( coords[ coords.Count - 1 ] - this.transform.position ).magnitude ) {
                    Debug.Log( "Section: " + i );
                    Debug.Log( "Mathf.Abs( speed ) : " + segmentLength / Mathf.Abs( speed )  + " ( coords[ coords.Count - 1 ] - this.transform.position ).magnitude: " + ( coords[ coords.Count - 1 ] - this.transform.position ).magnitude );

                    noise.pitch = Mathf.Abs( speed ) / maxSpeed;
                    
                    Vector3 nearestPoint = Vector3.zero;

                    //int nextIndex = indexPoint + ( int )( segmentLength * Mathf.Abs( speed ) * Time.deltaTime );
                    //if( nextIndex >= coords.Count ) {
                        //break;
                    //}
                    //indexPoint = nextIndex;
                    //nearestPoint = coords[ nextIndex ];


                    for( int j = indexPoint; j < coords.Count; j++ ) {

                        Debug.Log( "Point: " + j );
                        
                        nearestPoint = coords[ j ];
                        nearestPoint.z += heightCorrection;
                        if( ( nearestPoint - this.transform.position ).magnitude > Mathf.Abs( speed ) ) { 
                            indexPoint = j;
                            break;
                        }
                    }

                    //Debug.Log( "nearestPoint: " + nearestPoint );
                    //Debug.DrawLine( this.transform.position, nearestPoint, Color.green, Time.deltaTime );

                    //while( ( nearestPoint - this.transform.position ).magnitude > Mathf.Abs( speed ) ) {

                        //this.transform.position = Vector3.Lerp( this.transform.position, nearestPoint, ( Mathf.Abs( speed ) / segmentLength ) * Time.deltaTime );
                        this.transform.position = Vector3.Lerp( this.transform.position, nearestPoint, Mathf.Abs( speed ) * Time.deltaTime );
                        //this.transform.position = Vector3.Lerp( this.transform.position, this.transform.position + ( nearestPoint - this.transform.position ).normalized, Mathf.Abs( speed ) * Time.deltaTime );
                        //float losedSpeed = curveDrag * Vector3.Angle( this.transform.right, nearestPoint - this.transform.position ) / 360.0f;
                        this.transform.right = Vector3.Lerp( this.transform.right, nearestPoint - this.transform.position, Mathf.Abs( speed ) * Time.deltaTime );
                        //speed -= losedSpeed;


                        yield return new WaitForEndOfFrame();

                    //}
                }

                indexSection++;
                indexPoint = 0;
            }
        //}
    }


    IEnumerator GoForward2()
    {
        //foreach( string lineName in lines.Keys ) {
            List<LineSection> sections = lines[ keyLine ];

            for( int i = indexSection; i < sections.Count; i++ ) {

                Debug.Log( "Section: " + i );

                List<Vector3> points = sections[ i ].bezierCurveLimitedAngle;
                
                float deltaDist = 0.0f;
                float dist = 0.0f;
                while( ( points[ points.Count - 1 ] - this.transform.position ).magnitude > deltaDist ) {
                    
                    deltaDist = Time.deltaTime * Mathf.Abs( speed );
                    Vector3 nextPoint = Vector3.zero;

                    if( mainDir == Direction.Backward ) {

                        Debug.Log( "Previous index: " + indexPoint );
                        indexPoint++;
                        Debug.Log( "Next index: " + indexPoint );
                    }
                    mainDir = Direction.Forward;

                    for( int j = indexPoint; j < points.Count; j++ ) {
                        dist = ( points[ j ] - this.transform.position ).magnitude;

                        if( dist > deltaDist ) {
                            indexPoint = j;
                            nextPoint = points[ j ];
                            //Debug.Log( "indexPoint: " + indexPoint );
                            Debug.DrawLine( this.transform.position, nextPoint, Color.cyan, 1.0f );
                            break;
                        }
                    }
                    
                    //int k = 0;
                    Vector3 nextDir = nextPoint - this.transform.position;
                    Vector3 startDir = this.transform.right;
                    Vector3 startPos = this.transform.position;
                    float sumDist = 0.0f;
                    while( sumDist < ( nextPoint - this.transform.position ).magnitude ) {
                        //Debug.Log( "deltaDist: " + deltaDist + " - dist: " + ( nextPoint - this.transform.position ).magnitude );

                        //k++;
                        
                        //Vector3 nextDir = nextPoint - this.transform.position;
                        sumDist += Time.deltaTime * Mathf.Abs( speed );

                        //Debug.Log( "Frame: " + k + " - t: " +  deltaDist / dist );
                        //this.transform.position += nextDir.normalized * deltaDist;

                        this.transform.position = Vector3.Lerp( startPos, nextPoint, sumDist / dist ); 

                        //Debug.DrawRay( this.transform.position, this.transform.right * 20, Color.red, 3.0f );
                        //Debug.Log( "this.transform.right (before): " + this.transform.right );
                        //this.transform.right = Vector3.Lerp( startDir, nextDir, sumDist / dist );
                        //Debug.DrawRay( this.transform.position, this.transform.right * 20, Color.green, 3.0f );
                        //Debug.Log( "this.transform.right (after): " + this.transform.right );
                        this.transform.right = nextDir;

                        noise.pitch = Mathf.Abs( speed ) / maxSpeed;

                        yield return new WaitForFixedUpdate();
                    }
                }

                indexSection++;
                indexPoint = 0;
            }
        //}

        Debug.Log( "GoFoward ended" );
    }

    IEnumerator GoBackward2()
    {
        //foreach( string lineName in lines.Keys ) {
            List<LineSection> sections = lines[ keyLine ];

            for( int i = indexSection; i >= 0; i-- ) {

                Debug.Log( "Section: " + i );

                List<Vector3> points = sections[ i ].bezierCurveLimitedAngle;
                
                float deltaDist = 0.0f;
                float dist = 0.0f;
                while( ( points[ 0 ] - this.transform.position ).magnitude > deltaDist ) {
                    
                    deltaDist = Time.deltaTime * Mathf.Abs( speed );
                    Vector3 previousPoint = Vector3.zero;

                    if( mainDir == Direction.Forward ) {
                        Debug.Log( "Previous index: " + indexPoint );
                        indexPoint--;
                        Debug.Log( "Next index: " + indexPoint );
                    }
                    mainDir = Direction.Backward;

                    for( int j = indexPoint; j >= 0; j-- ) {
                        dist = ( points[ j ] - this.transform.position ).magnitude;

                        if( dist > deltaDist ) {
                            indexPoint = j;
                            previousPoint = points[ j ];
                            //Debug.Log( "indexPoint: " + indexPoint );
                            Debug.DrawLine( this.transform.position, previousPoint, Color.cyan, 1.0f );
                            break;
                        }
                    }
                    
                    //int k = 0;
                    Vector3 previousDir = this.transform.position - previousPoint;
                    Vector3 startDir = this.transform.right;
                    Vector3 startPos = this.transform.position;
                    float sumDist = 0.0f;
                    while( sumDist < ( previousPoint - this.transform.position ).magnitude ) {
                        //Debug.Log( "deltaDist: " + deltaDist + " - dist: " + ( previousPoint - this.transform.position ).magnitude );

                        //k++;
                        
                        //Vector3 nextDir = nextPoint - this.transform.position;
                        sumDist += Time.deltaTime * Mathf.Abs( speed );

                        //Debug.Log( "Frame: " + k + " - t: " +  deltaDist / dist );
                        //this.transform.position += nextDir.normalized * deltaDist;

                        this.transform.position = Vector3.Lerp( startPos, previousPoint, sumDist / dist ); 

                        //Debug.DrawRay( this.transform.position, this.transform.right * 20, Color.red, 3.0f );
                        //Debug.Log( "this.transform.right (before): " + this.transform.right );
                        //this.transform.right = Vector3.Lerp( startDir, previousDir, sumDist / dist );
                        //Debug.DrawRay( this.transform.position, this.transform.right * 20, Color.green, 3.0f );
                        //Debug.Log( "this.transform.right (after): " + this.transform.right );
                        this.transform.right = previousDir;

                        noise.pitch = Mathf.Abs( speed ) / maxSpeed;

                        yield return new WaitForFixedUpdate();
                    }
                }

                if( i >= 0 ) {
                    indexSection--;
                    indexPoint = sections[ indexSection ].bezierCurveLimitedAngle.Count - 1;
                }
            }
        //}

        Debug.Log( "GoFoward ended" );
    }
}
