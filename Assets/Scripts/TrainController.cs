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

    private enum Side
    {
        Center,
        Right,
        Left,
    }

    public float speed = 0.0f;
    public float maxSpeed = 250.0f;
    public float acceleration = 50.0f;
    public float deceleration = 75.0f;
    public float curveDrag = 0.1f;
    public float drag = 5.0f;

    public float deadZoneTriggerRight = 0.1f;
    public float deadZoneTriggerLeft = 0.1f;

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

    private Side railSide = Side.Left;

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
    void Update()
    {
        previousSpeed = speed;
        HandleMovement();
        HandleBrakingNoise();
    }

    private void HandleMovement() {
        if( speed > 1.0f && !goFowardActive )
        {
            noise.Play();
            StartCoroutine( "GoForward" );
            goFowardActive = true;
        }
        else if( speed <= 1.0f && goFowardActive) {
            noise.Stop();
            StopCoroutine( "GoForward" );
            goFowardActive = false;
        }

        if( speed < -1.0f && !goBackwardActive )
        {
            noise.Play();
            StartCoroutine( "GoBackward" );
            goBackwardActive = true;
        }
        else if( speed >= -1.0f && goBackwardActive) {
            noise.Stop();
            StopCoroutine( "GoBackward" );
            goBackwardActive = false;
        }

        if( Input.GetKey( KeyCode.D ) || Input.GetAxis( "RT" ) > ( deadZoneTriggerRight ) ) {

            float rightTriggerPression = 1.0f;
            if( Input.GetAxis( "RT" ) > ( deadZoneTriggerRight ) ) {
                rightTriggerPression = Input.GetAxis( "RT" );
            }

            Debug.Log( "rightTriggerPression: " + rightTriggerPression );

            if( mainDir == Direction.Forward || mainDir == Direction.None ) {
                speed += acceleration * rightTriggerPression * Time.deltaTime;
            }
            else if( mainDir == Direction.Backward ) {
                // In questo caso la speed è negativa
                speed += deceleration * Time.deltaTime;
            }

            if( speed > maxSpeed ) {
                speed = maxSpeed;
            }
        }
        if( Input.GetKey( KeyCode.A ) || Input.GetAxis( "LT" ) > ( deadZoneTriggerLeft ) ) {

            float leftTriggerPression = 1.0f;
            if( Input.GetAxis( "LT" ) > ( deadZoneTriggerLeft ) ) {
                leftTriggerPression = Input.GetAxis( "LT" );
            }

            if( mainDir == Direction.Backward || mainDir == Direction.None ) {
                speed -= acceleration * leftTriggerPression * Time.deltaTime;
            }
            else if( mainDir == Direction.Forward ) {
                // In questo caso la speed è positiva
                speed -= deceleration * Time.deltaTime;
            }

            if( speed < -maxSpeed ) {
                speed = -maxSpeed;
            }
        }

        if( Mathf.Abs( speed ) >= acceleration * Time.deltaTime ) {
            if( mainDir == Direction.Forward ) {
                speed -= drag * Time.deltaTime;
            }
            else if( mainDir == Direction.Backward ) {
                speed += drag * Time.deltaTime;
            }
        }
        else if( Mathf.Abs( speed ) <= acceleration * Time.deltaTime ) {
            speed = 0.0f;
        }
    }

    private void HandleBrakingNoise() {

        if( ( ( Input.GetKey( KeyCode.D ) || Input.GetAxis( "RT" ) > ( deadZoneTriggerRight ) ) && mainDir == Direction.Backward ) || ( ( Input.GetKey( KeyCode.A ) || Input.GetAxis( "LT" ) > ( deadZoneTriggerLeft ) ) && mainDir == Direction.Forward ) ) { 

            Debug.Log( "Braking" );
            if( !braking.isPlaying ) {
                braking.volume = Mathf.Abs( speed ) / maxSpeed;
                braking.time = 0.0f;
                braking.Play();
            }
            else {
                braking.volume = Mathf.Lerp( 0.0f, 1.0f, ( Mathf.Abs( speed ) / maxSpeed ) );
            }
        }
        else{
            braking.volume = Mathf.Lerp( braking.volume, 0.0f, brakingNoiseDecreasingSpeed * Time.deltaTime );

            if( braking.isPlaying && braking.volume <= 0.0f ) {
                Debug.Log( "Stop Braking" );
                braking.Stop();
            }
        }


    }

    IEnumerator GoForward()
    {
        //foreach( string lineName in lines.Keys ) {
            List<LineSection> sections = lines[ keyLine ];

            // Ciclo le sezioni della linea in avanti
            for( int i = indexSection; i < sections.Count; i++ ) {

                Debug.Log( "Section: " + i );

                List<Vector3> points = null;
                if( sections[ i ].type == Type.Tunnel ) {
                    if( sections[ i ].bidirectional ) {
                        if( railSide == Side.Left ) {
                            points = sections[ i ].floorPoints.leftLine;
                        }
                        else if( railSide == Side.Right ) {
                            points = sections[ i ].floorPoints.rightLine;
                        }
                    }
                    else {
                        points = sections[ i ].floorPoints.centerLine;
                    }
                }
                else {
                    points = sections[ i ].bezierCurveLimitedAngle;
                }
                
                float deltaDist = 0.0f;
                float dist = 0.0f;
                // Finché non sono abbastanza vicino all'ultimo punto della curva (mi sto muovendo in avanti, quindi "navigo" la lista dei punti in avanti)
                // continuo a cercare il punto sufficientemente lontano dal vagone per raggiungerlo
                while( ( points[ points.Count - 1 ] - this.transform.position ).magnitude > deltaDist ) {
                    // Distanza che sarà percorsa in un frame
                    deltaDist = Time.deltaTime * Mathf.Abs( speed );
                    Vector3 nextPoint = Vector3.zero;
                    Vector3 nextOrientationPoint = Vector3.zero;

                    // Aggiornamento del movimento attuale del vagone (in questo caso avanti)
                    if( mainDir == Direction.Backward ) {
                        // Se il vagone stava andando indietro, allora devo puntare all'index successivo, ma se sono
                        // già all'ultimo punto della curva passo alla sezione succcessiva
                        if( indexPoint + 1 < points.Count ) {
                            indexPoint++;
                        }
                    }
                    mainDir = Direction.Forward;

                    int startingIndex = indexPoint;
                    // Ricerca del punto della curva successivo più lontano della distanza (per frame) percorsa dal vagone, che diventerà
                    // il punto successivo verso cui si dirigerà il vagone
                    for( int j = indexPoint; j < points.Count; j++ ) {
                        int indexDiff = j - startingIndex;
                        dist = ( points[ j ] - this.transform.position ).magnitude;

                        indexPoint = j;
                        nextPoint = points[ j ];

                        if( j + indexDiff >= points.Count ) {
                            if( i + 1 < sections.Count ) {
                                indexDiff = j + indexDiff - ( points.Count - 1 );

                                if( sections[ i + 1 ].type == Type.Tunnel ) {
                                    if( sections[ i + 1 ].bidirectional ) {
                                        if( railSide == Side.Left ) {
                                            nextOrientationPoint = sections[ i + 1 ].floorPoints.leftLine[ indexDiff ];
                                        }
                                        else if( railSide == Side.Right ) {
                                            nextOrientationPoint = sections[ i + 1 ].floorPoints.rightLine[ indexDiff ];
                                        }
                                    }
                                    else {
                                        nextOrientationPoint = sections[ i + 1 ].floorPoints.centerLine[ indexDiff ];
                                    }
                                }
                                else {
                                    nextOrientationPoint = sections[ i + 1 ].bezierCurveLimitedAngle[ indexDiff ];
                                }
                            }
                            else {
                                nextOrientationPoint = points[ points.Count - 1 ];
                            }
                        }
                        else {
                            nextOrientationPoint = points[ j + indexDiff ];
                        }


                        if( dist > deltaDist ) {
                            //Debug.Log( "indexDiff: " + indexDiff );
                            break;
                        }
                    }
                    
                    // Posizione e rotazione iniziale del vagone per punto della curva
                    Vector3 nextDir = nextOrientationPoint - this.transform.position;
                    Vector3 startDir = this.transform.right;
                    Vector3 startPos = this.transform.position;
                    float sumDist = 0.0f;
                    while( sumDist < ( nextPoint - startPos ).magnitude ) {
                        Debug.DrawLine( this.transform.position, nextPoint, Color.red, Time.deltaTime );
                        Debug.DrawLine( this.transform.position, nextOrientationPoint, Color.magenta, Time.deltaTime );

                        // Interpolazione lineare (sulla distanza) per gestire posizione e rotazione frame successivo
                        this.transform.position = Vector3.Lerp( startPos, nextPoint, sumDist / dist ); 
                        this.transform.right = Vector3.Slerp( startDir, nextDir, sumDist / dist );

                        noise.pitch = Mathf.Abs( speed ) / maxSpeed;

                        //Debug.DrawRay( this.transform.position, this.transform.right * 20, Color.red, 1.0f );

                        sumDist += Time.deltaTime * Mathf.Abs( speed );

                        yield return new WaitForEndOfFrame();
                    }
                }

                indexSection++;
                indexPoint = 0;
            }
        //}

        Debug.Log( "GoFoward ended" );
    }

    IEnumerator GoBackward()
    {
        //foreach( string lineName in lines.Keys ) {
            List<LineSection> sections = lines[ keyLine ];

            // Ciclo le sezioni della linea all'indietro
            for( int i = indexSection; i >= 0; i-- ) {

                Debug.Log( "Section: " + i );

                List<Vector3> points = null;
                if( sections[ i ].type == Type.Tunnel ) {
                    if( sections[ i ].bidirectional ) {
                        if( railSide == Side.Left ) {
                            points = sections[ i ].floorPoints.leftLine;
                        }
                        else if( railSide == Side.Right ) {
                            points = sections[ i ].floorPoints.rightLine;
                        }
                    }
                    else {
                        points = sections[ i ].floorPoints.centerLine;
                    }
                }
                else {
                    points = sections[ i ].bezierCurveLimitedAngle;
                }
                
                float deltaDist = 0.0f;
                float dist = 0.0f;
                // Finché non sono abbastanza vicino al primo punto della curva (mi sto muovendo all'indietro, quindi "navigo" la lista dei punti all'indietro)
                // continuo a cercare il punto sufficientemente lontano dal vagone per raggiungerlo
                while( ( points[ 0 ] - this.transform.position ).magnitude > deltaDist ) {
                    // Distanza che sarà percorsa in un frame
                    deltaDist = Time.deltaTime * Mathf.Abs( speed );
                    Vector3 previousPoint = Vector3.zero;
                    Vector3 previousOrientationPoint = Vector3.zero;

                    // Aggiornamento del movimento attuale del vagone (in questo caso indietro)
                    if( mainDir == Direction.Forward ) {
                        // Se il vagone stava andando avanti, allora devo puntare all'index precedente, ma se sono
                        // già al primo punto della curva passo alla sezione precedente
                        if( indexPoint - 1 >= 0 ) {
                            indexPoint--;
                        }
                    }
                    mainDir = Direction.Backward;

                    int startingIndex = indexPoint;
                    // Ricerca del punto della curva precedente più lontano della distanza (per frame) percorsa dal vagone, che diventerà
                    // il punto successivo verso cui si dirigerà il vagone
                    for( int j = indexPoint; j >= 0; j-- ) {
                        int indexDiff = startingIndex - j;
                        dist = ( points[ j ] - this.transform.position ).magnitude;

                        indexPoint = j;
                        previousPoint = points[ j ];

                        if( j - indexDiff < 0 ) {
                            if( i - 1 >= 0 ) {
                                indexDiff = ( sections[ i - 1 ].bezierCurveLimitedAngle.Count - 1 ) + ( j - indexDiff );
                                
                                if( sections[ i - 1 ].type == Type.Tunnel ) {
                                    if( sections[ i - 1 ].bidirectional ) {
                                        if( railSide == Side.Left ) {
                                            previousOrientationPoint = sections[ i - 1 ].floorPoints.leftLine[ indexDiff ];
                                        }
                                        else if( railSide == Side.Right ) {
                                            previousOrientationPoint = sections[ i - 1 ].floorPoints.rightLine[ indexDiff ];
                                        }
                                    }
                                    else {
                                        previousOrientationPoint = sections[ i - 1 ].floorPoints.centerLine[ indexDiff ];
                                    }
                                }
                                else {
                                    previousOrientationPoint = sections[ i - 1 ].bezierCurveLimitedAngle[ indexDiff ];
                                }
                            }
                            else {
                                previousOrientationPoint = points[ 0 ];
                            }
                        }
                        else {
                            previousOrientationPoint = points[ j - indexDiff ];
                        }

                        if( dist > deltaDist ) {
                            Debug.Log( "indexPoint: " + indexPoint );
                            break;
                        }
                    }
                    
                    // Posizione e rotazione iniziale del vagone per punto della curva
                    Vector3 previousDir = this.transform.position - previousOrientationPoint;
                    Vector3 startDir = this.transform.right;
                    Vector3 startPos = this.transform.position;
                    float sumDist = 0.0f;
                    while( sumDist < ( previousPoint - startPos ).magnitude ) {

                        Debug.DrawLine( this.transform.position, previousPoint, Color.red, Time.deltaTime );
                        Debug.DrawLine( this.transform.position, previousOrientationPoint, Color.magenta, Time.deltaTime );

                        // Interpolazione lineare (sulla distanza) per gestire posizione e rotazione frame successivo
                        this.transform.position = Vector3.Lerp( startPos, previousPoint, sumDist / dist ); 
                        this.transform.right = Vector3.Slerp( startDir, previousDir, sumDist / dist );
                        //Debug.DrawRay( this.transform.position, -this.transform.right * 20, Color.red, 1.0f );

                        noise.pitch = Mathf.Abs( speed ) / maxSpeed;

                        sumDist += Time.deltaTime * Mathf.Abs( speed );

                        yield return new WaitForEndOfFrame();
                    }
                }

                // Aggiornamento index una volta raggiunto il punto iniziale della curva
                if( i > 0 ) {
                    indexSection--;
                    indexPoint = sections[ indexSection ].bezierCurveLimitedAngle.Count - 1;
                }
            }
        //}

        Debug.Log( "GoFoward ended" );
    }
}
