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
    public float maxSpeed = 250.0f;
    public float acceleration = 50.0f;
    public float deceleration = 75.0f;
    public float curveDrag = 0.1f;
    public float drag = 5.0f;

    private Dictionary<string, List<LineSection>> lines;
    private AudioSource noise;
    private AudioSource braking;

    private bool goFowardActive = false;
    private bool goBackwardActive = false;
    private float brakingNoiseDecreasingSpeed = 5.0f;
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

        if( Input.GetKey( KeyCode.D ) ) {

            if( mainDir == Direction.Forward || mainDir == Direction.None ) {
                speed += acceleration * Time.fixedDeltaTime;
            }
            else if( mainDir == Direction.Backward ) {
                // In questo caso la speed è negativa
                speed += deceleration * Time.fixedDeltaTime;
            }

            if( speed > maxSpeed ) {
                speed = maxSpeed;
            }
        }
        if( Input.GetKey( KeyCode.A ) ) {

            if( mainDir == Direction.Backward || mainDir == Direction.None ) {
                speed -= acceleration * Time.fixedDeltaTime;
            }
            else if( mainDir == Direction.Forward ) {
                // In questo caso la speed è positiva
                speed -= deceleration * Time.fixedDeltaTime;
            }

            if( speed < -maxSpeed ) {
                speed = -maxSpeed;
            }
        }

        if( Mathf.Abs( speed ) >= acceleration * Time.fixedDeltaTime ) {
            if( mainDir == Direction.Forward ) {
                speed -= drag * Time.deltaTime;
            }
            else if( mainDir == Direction.Backward ) {
                speed += drag * Time.deltaTime;
            }
        }
        else if( Mathf.Abs( speed ) <= acceleration * Time.fixedDeltaTime ) {
            speed = 0.0f;
        }
    }

    private void HandleBrakingNoise() {

        if( ( Input.GetKey( KeyCode.D ) && mainDir == Direction.Backward ) || ( Input.GetKey( KeyCode.A ) && mainDir == Direction.Forward ) ) { 

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
            braking.volume = Mathf.Lerp( braking.volume, 0.0f, brakingNoiseDecreasingSpeed * Time.fixedDeltaTime );

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

                List<Vector3> points = sections[ i ].bezierCurveLimitedAngle;
                
                float deltaDist = 0.0f;
                float dist = 0.0f;
                // Finché non sono abbastanza vicino all'ultimo punto della curva (mi sto muovendo in avanti, quindi "navigo" la lista dei punti in avanti)
                // continuo a cercare il punto sufficientemente lontano dal vagone per raggiungerlo
                while( ( points[ points.Count - 1 ] - this.transform.position ).magnitude > deltaDist ) {
                    // Distanza che sarà percorsa in un frame
                    deltaDist = Time.deltaTime * Mathf.Abs( speed );
                    Vector3 nextPoint = Vector3.zero;

                    // Aggiornamento del movimento attuale del vagone (in questo caso avanti)
                    if( mainDir == Direction.Backward ) {
                        // Se il vagone stava andando indietro, allora devo puntare all'index successivo, ma se sono
                        // già all'ultimo punto della curva passo alla sezione succcessiva
                        if( indexPoint + 1 < points.Count ) {
                            indexPoint++;
                        }
                    }
                    mainDir = Direction.Forward;

                    // Ricerca del punto della curva successivo più lontano della distanza (per frame) percorsa dal vagone, che diventerà
                    // il punto successivo verso cui si dirigerà il vagone
                    for( int j = indexPoint; j < points.Count; j++ ) {
                        dist = ( points[ j ] - this.transform.position ).magnitude;

                        indexPoint = j;
                        nextPoint = points[ j ];
                        if( dist > deltaDist ) {
                            //Debug.DrawLine( this.transform.position, nextPoint, Color.cyan, 1.0f );
                            break;
                        }
                    }

                    Debug.Log( "indexPoint: " + indexPoint );
                    
                    // Posizione e rotazione iniziale del vagone per punto della curva
                    Vector3 nextDir = nextPoint - this.transform.position;
                    Vector3 startDir = this.transform.right;
                    Vector3 startPos = this.transform.position;
                    float sumDist = 0.0f;
                    Debug.DrawLine( startPos, nextPoint, Color.green, 1.0f );
                    while( sumDist < ( nextPoint - startPos ).magnitude ) {

                        // Interpolazione lineare (sulla distanza) per gestire posizione e rotazione frame successivo
                        this.transform.position = Vector3.Lerp( startPos, nextPoint, sumDist / dist ); 
                        this.transform.right = Vector3.Slerp( startDir, nextDir, sumDist / dist );

                        noise.pitch = Mathf.Abs( speed ) / maxSpeed;

                        Debug.DrawRay( this.transform.position, this.transform.right * 20, Color.red, 1.0f );
                        Debug.DrawLine( this.transform.position, nextPoint, Color.cyan, Time.deltaTime );

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

                List<Vector3> points = sections[ i ].bezierCurveLimitedAngle;
                
                float deltaDist = 0.0f;
                float dist = 0.0f;
                // Finché non sono abbastanza vicino al primo punto della curva (mi sto muovendo all'indietro, quindi "navigo" la lista dei punti all'indietro)
                // continuo a cercare il punto sufficientemente lontano dal vagone per raggiungerlo
                while( ( points[ 0 ] - this.transform.position ).magnitude > deltaDist ) {
                    // Distanza che sarà percorsa in un frame
                    deltaDist = Time.deltaTime * Mathf.Abs( speed );
                    Vector3 previousPoint = Vector3.zero;

                    // Aggiornamento del movimento attuale del vagone (in questo caso indietro)
                    if( mainDir == Direction.Forward ) {
                        // Se il vagone stava andando avanti, allora devo puntare all'index precedente, ma se sono
                        // già al primo punto della curva passo alla sezione precedente
                        if( indexPoint - 1 >= 0 ) {
                            indexPoint--;
                        }
                    }
                    mainDir = Direction.Backward;

                    // Ricerca del punto della curva precedente più lontano della distanza (per frame) percorsa dal vagone, che diventerà
                    // il punto successivo verso cui si dirigerà il vagone
                    for( int j = indexPoint; j >= 0; j-- ) {
                        dist = ( points[ j ] - this.transform.position ).magnitude;

                        indexPoint = j;
                        previousPoint = points[ j ];
                        if( dist > deltaDist ) {
                            break;
                        }
                    }

                    Debug.Log( "indexPoint: " + indexPoint );
                    
                    // Posizione e rotazione iniziale del vagone per punto della curva
                    Vector3 previousDir = this.transform.position - previousPoint;
                    Vector3 startDir = this.transform.right;
                    Vector3 startPos = this.transform.position;
                    float sumDist = 0.0f;
                    while( sumDist < ( previousPoint - startPos ).magnitude ) {

                        // Interpolazione lineare (sulla distanza) per gestire posizione e rotazione frame successivo
                        this.transform.position = Vector3.Lerp( startPos, previousPoint, sumDist / dist ); 
                        this.transform.right = Vector3.Slerp( startDir, previousDir, sumDist / dist );

                        noise.pitch = Mathf.Abs( speed ) / maxSpeed;

                        sumDist += Time.deltaTime * Mathf.Abs( speed );

                        yield return new WaitForFixedUpdate();
                    }
                }

                // Aggiornamento index una volta raggiunto il punto iniziale della curva
                if( i >= 0 ) {
                    indexSection--;
                    indexPoint = sections[ indexSection ].bezierCurveLimitedAngle.Count - 1;
                }
            }
        //}

        Debug.Log( "GoFoward ended" );
    }
}
