using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TrainController : MonoBehaviour
{

    private enum Rail
    {
        Center,
        Right,
        Left,
    }

    public float speed = 0.0f;
    public float maxSpeed = 250.0f;
    public float acceleration = 50.0f;
    public float deceleration = 75.0f;
    public float drag = 5.0f;
    public float gimbalLockProtectionDegs = 1.0f;
    public bool debugSpeed = false;

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

    public string keyLine = "Linea 0";
    public int indexPoint = 0;
    public int indexSection = 0;

    private float heightCorrection;

    private Rail railSide = Rail.Right;
    private Direction actualMovement = Direction.None;

    private List<Vector3> actualSectionPoints = new() { Vector3.zero };
    private Vector3 actualPoint = Vector3.zero;
    private Vector3 actualOrientationPoint = Vector3.zero;
    public int actualSwitchIndex = -1;
    public string actualSwitchLine = "Linea 0";
    public SwitchDirection actualSwitchDirection;

    public int nextSwitchIndex = -1;
    public string nextSwitchLine = "Linea 0";
    public SwitchDirection nextSwitchDirection;
    public int previousSwitchIndex = -1;
    public string previousSwitchLine = "Linea 0";
    public SwitchDirection previousSwitchDirection;

    private bool inverseSection = false;
    private bool inverseLine = false;

    //private DynamicIcons[] dynamicIconsCmps;
    private TMP_Text debugText;

    public bool showDebugInfo = false;

    private bool ignoreInputs = false;

    void Start()
    {
        if( debugSpeed ) {
            maxSpeed *= 10;
            acceleration *= 10;
            deceleration *= 10;
            drag *= 10;
        }

        //this.dynamicIconsCmps = ( DynamicIcons[] )GameObject.FindObjectsOfType( typeof( DynamicIcons ) );

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

        MetroGenerator metroGen = GameObject.Find( "MetroGenerator" ).GetComponent<MetroGenerator>(); 
        lines = metroGen.lines;
        heightCorrection = metroGen.trainHeightFromGround;

        Vector3 startPos = Vector3.zero;
        Vector3 startDir = Vector3.zero;
        MeshGenerator.Floor startingFloor = lines[ keyLine ][ indexSection ].floorPoints; 
        if( metroGen.startingBidirectional ) {
            if( Random.Range( 0, 2 ) == 0 ) {
                railSide = Rail.Left;
                startPos = startingFloor.leftLine[ indexPoint ];
                startDir = startingFloor.leftLine[ indexPoint + 1 ] - startPos;
            }
            else {
                railSide = Rail.Right;
                startPos = startingFloor.rightLine[ indexPoint ];
                startDir = startingFloor.rightLine[ indexPoint + 1 ] - startPos;
            }
        }
        else{ 
            railSide = Rail.Center;
            startPos = startingFloor.centerLine[ indexPoint ];
            startDir = startingFloor.centerLine[ indexPoint + 1 ] - startPos;
        }
        this.transform.position = startPos + ( Vector3.forward * heightCorrection );
        this.transform.right = startDir;

        if( showDebugInfo ) {
            this.debugText = GameObject.Find( "Debug Text" ).GetComponent<TMP_Text>();
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        previousSpeed = speed;
        HandleMovement();
        HandleBrakingNoise();
        HandleSwitch();
        //HandleSwitchesIndicators();

        if( showDebugInfo ) {
            ShowDebugInfo();
        }
    }

    private void ShowDebugInfo() {

        string switchDir = "-";
        string switchType = "-";
        if( actualSwitchIndex > -1 ) {
            switchDir = lines[ keyLine ][ actualSwitchIndex ].activeSwitch.ToString();
            switchType = lines[ keyLine ][ actualSwitchIndex ].switchType.ToString();
        }

        this.debugText.text = "DEBUG INFO: \n" +
                              "Line Name: " + keyLine + "\n" +
                              "Section: " + indexSection + "\n" +
                              "Point: " + indexPoint + "\n" +
                              "\n" +
                              "Actual Switch Index: " + actualSwitchIndex + "\n" + 
                              "Actual Switch Type: " + switchType + "\n" +
                              "Actual Switch Direction: " + switchDir + "\n" +
                              "Previous Switch Index: " + this.previousSwitchIndex + "\n" + 
                              //"Previous Switch Type: " + switchType + "\n" +
                              "Previous Switch Direction: " + this.previousSwitchDirection + "\n" +
                              "Next Switch Index: " + this.nextSwitchIndex + "\n" + 
                              //"Next Switch Type: " + switchType + "\n" +
                              "Next Switch Direction: " + this.nextSwitchDirection + "\n" +
                              "\n" +
                              "Speed: " + speed + "\n" +
                              "Direction: " + actualMovement + "\n" +
                              "Side: " + railSide + "\n" +
                              "Inverse: " + inverseLine + "\n";
    }

    private void HandleSwitch() {

        string lineName = keyLine;
        List<LineSection> sections = lines[ lineName ];

        // Determino gli indici 
        this.nextSwitchIndex = sections.Count;
        for( int i = indexSection; i < sections.Count; i++ ) {
            if( sections[ i ].type == Type.Switch && i != indexSection ) {
                this.nextSwitchIndex = i;
                this.nextSwitchLine = lineName;
                this.nextSwitchDirection = lines[ nextSwitchLine ][ nextSwitchIndex ].activeSwitch;
                break;
            }
        }

        this.previousSwitchIndex = -1;
        for( int i = indexSection; i >= 0; i-- ) {
            
            if( i == 0 && sections[ i ].fromSection != null ) {
                this.previousSwitchIndex = sections[ i ].fromSection.sectionIndex;
                this.previousSwitchLine = sections[ i ].fromSection.lineName;

                this.previousSwitchDirection = lines[ previousSwitchLine ][ previousSwitchIndex ].activeSwitch;
                break;
            }
            else if( sections[ i ].type == Type.Switch && i != indexSection ) {
                this.previousSwitchIndex = i;
                this.previousSwitchLine = lineName;

                this.previousSwitchDirection = lines[ previousSwitchLine ][ previousSwitchIndex ].activeSwitch;
                break;
            }
        }

        if( ( ( actualMovement == Direction.Forward && !inverseLine ) || ( actualMovement == Direction.Backward && inverseLine ) ) && actualSwitchIndex < indexSection ) {

            this.actualSwitchIndex = this.nextSwitchIndex;
            lineName = this.nextSwitchLine;
        }
        else if( ( ( actualMovement == Direction.Forward && inverseLine ) || ( actualMovement == Direction.Backward && !inverseLine ) ) && actualSwitchIndex > indexSection ) {

            this.actualSwitchIndex = this.previousSwitchIndex;
            lineName = this.previousSwitchLine;
        }

        this.actualSwitchLine = lineName;
        

        if( !this.ignoreInputs && actualSwitchIndex != indexSection && ( Input.GetButtonDown( "RB" ) || Input.GetKeyDown( KeyCode.F ) ) ) {

            sections = lines[ lineName ];

            SwitchDirection selectedDir;
            if( lineName != keyLine ) {
                if( sections[ actualSwitchIndex ].switchType == SwitchType.MonoToNewMono ) {

                    selectedDir = new SwitchDirection();
                    int sectionCount = -1;

                    if( sections[ actualSwitchIndex ].activeSwitch == SwitchDirection.CenterToEntranceLeft || sections[ actualSwitchIndex ].activeSwitch == SwitchDirection.CenterToExitLeft ) {

                        switch( sections[ actualSwitchIndex ].activeSwitch ) {
                            case SwitchDirection.CenterToEntranceLeft:  selectedDir = SwitchDirection.CenterToExitLeft;
                                                                        sectionCount = sections[ actualSwitchIndex ].floorPoints.centerExitLeft.Count;
                                                                        break;
                            default:                                    selectedDir = SwitchDirection.CenterToEntranceLeft;
                                                                        sectionCount = sections[ actualSwitchIndex ].floorPoints.centerEntranceLeft.Count;
                                                                        break;
                        }

                    }
                    else if( sections[ actualSwitchIndex ].activeSwitch == SwitchDirection.CenterToEntranceRight || sections[ actualSwitchIndex ].activeSwitch == SwitchDirection.CenterToExitRight ) {

                        switch( sections[ actualSwitchIndex ].activeSwitch ) {
                            case SwitchDirection.CenterToEntranceRight: selectedDir = SwitchDirection.CenterToExitRight;
                                                                        sectionCount = sections[ actualSwitchIndex ].floorPoints.centerExitRight.Count;
                                                                        break;
                            default:                                    selectedDir = SwitchDirection.CenterToEntranceRight;
                                                                        sectionCount = sections[ actualSwitchIndex ].floorPoints.centerEntranceRight.Count;
                                                                        break;
                        }

                    }

                    sections[ actualSwitchIndex ].activeSwitch = selectedDir;
                    sections[ actualSwitchIndex ].curvePointsCount = sectionCount;

                }
                else if( sections[ actualSwitchIndex ].switchType == SwitchType.BiToNewBi ) {

                    // Dalla linea figlia mi dirigo verso lo switch della linea madre
                    selectedDir = new SwitchDirection();
                    int sectionCount = -1;

                    Debug.Log( ">>> New line start keys: " );
                    foreach( Side nls in sections[ actualSwitchIndex ].newLinesStarts.Keys ) {
                        Debug.Log( ">>> " + nls );
                    }

                    // Arriviamo da destra
                    if( sections[ actualSwitchIndex ].newLinesStarts.ContainsKey( Side.Right ) && sections[ actualSwitchIndex ].newLinesStarts[ Side.Right ].lineName == keyLine ) {

                        if( railSide == Rail.Right ) {
                            selectedDir = SwitchDirection.RightToEntranceRight;
                            sectionCount = sections[ actualSwitchIndex ].floorPoints.rightEntranceRight.Count;
                        }
                        else if( railSide == Rail.Left ) {
                            selectedDir = SwitchDirection.RightToExitRight;
                            sectionCount = sections[ actualSwitchIndex ].floorPoints.rightExitRight.Count;
                        }
                    }
                    // Arriviamo da sinistra
                    else if( sections[ actualSwitchIndex ].newLinesStarts.ContainsKey( Side.Left ) && sections[ actualSwitchIndex ].newLinesStarts[ Side.Left ].lineName == keyLine ) {

                        if( railSide == Rail.Right ) {
                            selectedDir = SwitchDirection.LeftToExitLeft;
                            sectionCount = sections[ actualSwitchIndex ].floorPoints.leftExitLeft.Count;
                        }
                        else if( railSide == Rail.Left ) {
                            selectedDir = SwitchDirection.LeftToEntranceLeft;
                            sectionCount = sections[ actualSwitchIndex ].floorPoints.leftEntranceLeft.Count;
                        }
                    }

                    sections[ actualSwitchIndex ].activeSwitch = selectedDir;
                    sections[ actualSwitchIndex ].curvePointsCount = sectionCount;

                }
            }
            else if( indexSection != actualSwitchIndex && actualSwitchIndex > -1 && actualSwitchIndex < lines[ lineName ].Count ) {

                selectedDir = sections[ actualSwitchIndex ].activeSwitch;
                int sectionCount = -1;

                if( sections[ actualSwitchIndex ].switchType == SwitchType.BiToBi ) {
                    if( ( ( actualMovement == Direction.Forward && inverseLine == false ) || ( actualMovement == Direction.Backward && inverseLine == true ) )  && railSide == Rail.Right ) {
                        switch( sections[ actualSwitchIndex ].activeSwitch ) {
                            case SwitchDirection.RightToRight:  selectedDir = SwitchDirection.RightToLeft;
                                                                sectionCount = sections[ actualSwitchIndex ].floorPoints.rightLeftLine.Count;
                                                                break;
                            default:                            selectedDir = SwitchDirection.RightToRight;
                                                                sectionCount = sections[ actualSwitchIndex ].floorPoints.rightLine.Count;
                                                                break;
                        }
                    }
                    else if( ( ( actualMovement == Direction.Forward && inverseLine == false ) || ( actualMovement == Direction.Backward && inverseLine == true ) ) && railSide == Rail.Left ) {
                        switch( sections[ actualSwitchIndex ].activeSwitch ) {
                            case SwitchDirection.LeftToLeft:    selectedDir = SwitchDirection.LeftToRight;
                                                                sectionCount = sections[ actualSwitchIndex ].floorPoints.leftRightLine.Count;
                                                                break;
                            default:                            selectedDir = SwitchDirection.LeftToLeft;
                                                                sectionCount = sections[ actualSwitchIndex ].floorPoints.leftLine.Count;
                                                                break;
                        }
                    }
                    else if( ( ( actualMovement == Direction.Backward && !inverseLine ) || ( actualMovement == Direction.Forward && inverseLine ) ) && railSide == Rail.Right ) {
                        switch( sections[ actualSwitchIndex ].activeSwitch ) {
                            case SwitchDirection.RightToRight:  selectedDir = SwitchDirection.LeftToRight;
                                                                sectionCount = sections[ actualSwitchIndex ].floorPoints.leftRightLine.Count;
                                                                break;
                            default:                            selectedDir = SwitchDirection.RightToRight;
                                                                sectionCount = sections[ actualSwitchIndex ].floorPoints.rightLine.Count;
                                                                break;
                        }
                    }
                    else if( ( ( actualMovement == Direction.Backward && !inverseLine ) || ( actualMovement == Direction.Forward && inverseLine ) ) && railSide == Rail.Left ) {
                        switch( sections[ actualSwitchIndex ].activeSwitch ) {
                            case SwitchDirection.LeftToLeft:    selectedDir = SwitchDirection.RightToLeft;
                                                                sectionCount = sections[ actualSwitchIndex ].floorPoints.rightLeftLine.Count;
                                                                break;
                            default:                            selectedDir = SwitchDirection.LeftToLeft;
                                                                sectionCount = sections[ actualSwitchIndex ].floorPoints.leftLine.Count;
                                                                break;
                        }
                    }
                }
                else if( sections[ actualSwitchIndex ].switchType == SwitchType.BiToMono || sections[ actualSwitchIndex ].switchType == SwitchType.MonoToBi ) {
                    if( railSide == Rail.Center ) {
                        switch( sections[ actualSwitchIndex ].activeSwitch ) {
                            case SwitchDirection.RightToCenter: selectedDir = SwitchDirection.LeftToCenter;
                                                                sectionCount = sections[ actualSwitchIndex ].floorPoints.leftCenterLine.Count;
                                                                break;
                            default:                            selectedDir = SwitchDirection.RightToCenter;
                                                                sectionCount = sections[ actualSwitchIndex ].floorPoints.rightCenterLine.Count;
                                                                break;
                        }
                    }
                    else if( railSide == Rail.Right ) {
                        selectedDir = SwitchDirection.RightToCenter;
                        sectionCount = sections[ actualSwitchIndex ].floorPoints.rightCenterLine.Count;                              
                    }
                    else if( railSide == Rail.Left ) {
                        selectedDir = SwitchDirection.LeftToCenter;
                        sectionCount = sections[ actualSwitchIndex ].floorPoints.leftCenterLine.Count;                              
                    }
                }
                else if( sections[ actualSwitchIndex ].switchType == SwitchType.MonoToNewMono ) {

                    if( ( actualMovement == Direction.Forward && !inverseLine ) || ( actualMovement == Direction.Backward && inverseLine ) ) {
            
                        if( sections[ actualSwitchIndex ].newLinesStarts.ContainsKey( Side.Left ) && sections[ actualSwitchIndex ].newLinesStarts.ContainsKey( Side.Right ) ) {
                            switch( sections[ actualSwitchIndex ].activeSwitch ) {
                                case SwitchDirection.CenterToCenter:        selectedDir = SwitchDirection.CenterToEntranceLeft;
                                                                            sectionCount = sections[ actualSwitchIndex ].floorPoints.centerEntranceLeft.Count;
                                                                            break;
                                case SwitchDirection.CenterToEntranceLeft:  selectedDir = SwitchDirection.CenterToEntranceRight;
                                                                            sectionCount = sections[ actualSwitchIndex ].floorPoints.centerEntranceRight.Count;
                                                                            break;
                                default:                                    selectedDir = SwitchDirection.CenterToCenter;
                                                                            sectionCount = sections[ actualSwitchIndex ].floorPoints.centerLine.Count;
                                                                            break;
                            }
                        }
                        else if( sections[ actualSwitchIndex ].newLinesStarts.ContainsKey( Side.Left ) ) {
                            
                            switch( sections[ actualSwitchIndex ].activeSwitch ) {
                                case SwitchDirection.CenterToCenter:    selectedDir = SwitchDirection.CenterToEntranceLeft;
                                                                        sectionCount = sections[ actualSwitchIndex ].floorPoints.centerEntranceLeft.Count;
                                                                        break;
                                default:                                selectedDir = SwitchDirection.CenterToCenter;
                                                                        sectionCount = sections[ actualSwitchIndex ].floorPoints.centerLine.Count;
                                                                        break;
                            }
                        }
                        else if( sections[ actualSwitchIndex ].newLinesStarts.ContainsKey( Side.Right ) ) {
                            switch( sections[ actualSwitchIndex ].activeSwitch ) {
                                case SwitchDirection.CenterToCenter:    selectedDir = SwitchDirection.CenterToEntranceRight;
                                                                        sectionCount = sections[ actualSwitchIndex ].floorPoints.centerEntranceRight.Count;
                                                                        break;
                                default:                                selectedDir = SwitchDirection.CenterToCenter;
                                                                        sectionCount = sections[ actualSwitchIndex ].floorPoints.centerLine.Count;
                                                                        break;
                            }
                        }
                    }
                    else if( ( actualMovement == Direction.Forward && inverseLine ) || ( actualMovement == Direction.Backward && !inverseLine ) ) {
                        if( sections[ actualSwitchIndex ].newLinesStarts.ContainsKey( Side.Left ) && sections[ actualSwitchIndex ].newLinesStarts.ContainsKey( Side.Right ) ) {
                            switch( sections[ actualSwitchIndex ].activeSwitch ) {
                                case SwitchDirection.CenterToCenter:        selectedDir = SwitchDirection.CenterToExitLeft;
                                                                            sectionCount = sections[ actualSwitchIndex ].floorPoints.centerExitLeft.Count;
                                                                            break;
                                case SwitchDirection.CenterToEntranceLeft:  selectedDir = SwitchDirection.CenterToExitRight;
                                                                            sectionCount = sections[ actualSwitchIndex ].floorPoints.centerExitRight.Count;
                                                                            break;
                                default:                                    selectedDir = SwitchDirection.CenterToCenter;
                                                                            sectionCount = sections[ actualSwitchIndex ].floorPoints.centerLine.Count;
                                                                            break;
                            }
                        }
                        else if( sections[ actualSwitchIndex ].newLinesStarts.ContainsKey( Side.Left ) ) {
                            switch( sections[ actualSwitchIndex ].activeSwitch ) {
                                case SwitchDirection.CenterToCenter:    selectedDir = SwitchDirection.CenterToExitLeft;
                                                                        sectionCount = sections[ actualSwitchIndex ].floorPoints.centerExitLeft.Count;
                                                                        break;
                                default:                                selectedDir = SwitchDirection.CenterToCenter;
                                                                        sectionCount = sections[ actualSwitchIndex ].floorPoints.centerLine.Count;
                                                                        break;
                            }
                        }
                        else if( sections[ actualSwitchIndex ].newLinesStarts.ContainsKey( Side.Right ) ) {
                            switch( sections[ actualSwitchIndex ].activeSwitch ) {
                                case SwitchDirection.CenterToCenter:    selectedDir = SwitchDirection.CenterToExitRight;
                                                                        sectionCount = sections[ actualSwitchIndex ].floorPoints.centerExitRight.Count;
                                                                        break;
                                default:                                selectedDir = SwitchDirection.CenterToCenter;
                                                                        sectionCount = sections[ actualSwitchIndex ].floorPoints.centerLine.Count;
                                                                        break;
                            }
                        }                       
                    }
                }
                else if( sections[ actualSwitchIndex ].switchType == SwitchType.BiToNewBi ) {

                    if( ( actualMovement == Direction.Forward && !inverseLine ) || ( actualMovement == Direction.Backward && inverseLine ) ) {

                        if( railSide == Rail.Left && sections[ actualSwitchIndex ].newLinesStarts.ContainsKey( Side.Left ) ) {
                            switch( sections[ actualSwitchIndex ].activeSwitch ) {
                                case SwitchDirection.LeftToLeft:    selectedDir = SwitchDirection.LeftToEntranceLeft;
                                                                    sectionCount = sections[ actualSwitchIndex ].floorPoints.leftEntranceLeft.Count;
                                                                    break;
                                default:                            selectedDir = SwitchDirection.LeftToLeft;
                                                                    sectionCount = sections[ actualSwitchIndex ].floorPoints.leftLine.Count;
                                                                    break;
                            }
                        }
                        else if( railSide == Rail.Right && sections[ actualSwitchIndex ].newLinesStarts.ContainsKey( Side.Right ) ) {
                            switch( sections[ actualSwitchIndex ].activeSwitch ) {
                                case SwitchDirection.RightToRight:  selectedDir = SwitchDirection.RightToEntranceRight;
                                                                    sectionCount = sections[ actualSwitchIndex ].floorPoints.rightEntranceRight.Count;
                                                                    break;
                                default:                            selectedDir = SwitchDirection.RightToRight;
                                                                    sectionCount = sections[ actualSwitchIndex ].floorPoints.rightLine.Count;
                                                                    break;
                            }
                        }
                    }
                    else if( ( actualMovement == Direction.Forward && inverseLine ) || ( actualMovement == Direction.Backward && !inverseLine ) ) {

                        if( railSide == Rail.Left && sections[ actualSwitchIndex ].newLinesStarts.ContainsKey( Side.Left ) ) {
                            switch( sections[ actualSwitchIndex ].activeSwitch ) {
                                case SwitchDirection.LeftToLeft:    selectedDir = SwitchDirection.LeftToExitLeft;
                                                                    sectionCount = sections[ actualSwitchIndex ].floorPoints.leftExitLeft.Count;
                                                                    break;
                                default:                            selectedDir = SwitchDirection.LeftToLeft;
                                                                    sectionCount = sections[ actualSwitchIndex ].floorPoints.leftLine.Count;
                                                                    break;
                            }
                        }
                        else if( railSide == Rail.Right && sections[ actualSwitchIndex ].newLinesStarts.ContainsKey( Side.Right ) ) {
                            switch( sections[ actualSwitchIndex ].activeSwitch ) {
                                case SwitchDirection.RightToRight:  selectedDir = SwitchDirection.RightToExitRight;
                                                                    sectionCount = sections[ actualSwitchIndex ].floorPoints.rightExitRight.Count;
                                                                    break;
                                default:                            selectedDir = SwitchDirection.RightToRight;
                                                                    sectionCount = sections[ actualSwitchIndex ].floorPoints.rightLine.Count;
                                                                    break;
                            }
                        }
                    }
                }

                this.actualSwitchDirection = selectedDir;

                sections[ actualSwitchIndex ].activeSwitch = selectedDir;
                sections[ actualSwitchIndex ].curvePointsCount = sectionCount;
            }
        

            //UpdateSwitchLight( sections[ actualSwitchIndex ] );
        }
    }

    public static void UpdateSwitchLight( LineSection switchSection ) { 
        Dictionary<string, List<Light>> updateLast = new();
        foreach( SwitchDirection activeSwitch in switchSection.switchLights.Keys ) {
            List<GameObject> lights = switchSection.switchLights[ activeSwitch ];
            foreach( GameObject light in lights ) {
                foreach( Transform child in light.transform ) {
                    if( activeSwitch != switchSection.activeSwitch ) {
                        if( child.name == "Red Light" ) {
                            child.GetComponent<Light>().intensity = 15.0f;
                        }
                        else if( child.name == "Green Light" ) {
                            child.GetComponent<Light>().intensity = 0.0f;
                        }
                    }
                    else {
                        if( child.name == "Red Light" ) {
                            if( updateLast.ContainsKey( "OFF" ) ){
                                updateLast[ "OFF" ].Add( child.GetComponent<Light>() );
                            }
                            else {
                                updateLast.Add( "OFF", new List<Light>{ child.GetComponent<Light>() } );
                            }
                            
                            //child.GetComponent<Light>().intensity = 1.0f;
                        }
                        else if( child.name == "Green Light" ) {
                            if( updateLast.ContainsKey( "ON" ) ){
                                updateLast[ "ON" ].Add( child.GetComponent<Light>() );
                            }
                            else {
                                updateLast.Add( "ON", new List<Light>{ child.GetComponent<Light>() } );
                            }
                            //child.GetComponent<Light>().intensity = 0.0f;
                        }
                    }
                }
            }
        }

        foreach( string status in updateLast.Keys ) {
            if( status == "OFF" ) {
                foreach( Light switchLight in updateLast[ "OFF" ] ) {
                    switchLight.GetComponent<Light>().intensity = 0.0f;

                }
            }
            else if( status == "ON" ) {
                foreach( Light switchLight in updateLast[ "ON" ] ) {
                    switchLight.GetComponent<Light>().intensity = 15.0f;
                }
            }
        }
    }

    public void IgnoreInputs( bool ignore ) {
        this.ignoreInputs = ignore;
    } 

    private void HandleMovement() {

        if( speed > 1.0f && !goFowardActive )
        {
            noise.Play();
            StartCoroutine( nameof( GoForward ) );
            goFowardActive = true;
        }
        else if( speed <= 1.0f && goFowardActive) {
            noise.Stop();
            StopCoroutine( nameof( GoForward ) );
            goFowardActive = false;
        }

        if( speed < -1.0f && !goBackwardActive )
        {
            noise.Play();
            StartCoroutine( nameof( GoBackward ) );
            goBackwardActive = true;
        }
        else if( speed >= -1.0f && goBackwardActive) {
            noise.Stop();
            StopCoroutine( nameof( GoBackward ) );
            goBackwardActive = false;
        }

        if( !this.ignoreInputs && ( Input.GetKey( KeyCode.D ) || Input.GetAxis( "RT" ) > deadZoneTriggerRight ) ) {

            float rightTriggerPression = 1.0f;
            if( Input.GetAxis( "RT" ) > deadZoneTriggerRight ) {
                rightTriggerPression = Input.GetAxis( "RT" );
            }

            ////Debug.Log( "rightTriggerPression: " + rightTriggerPression );

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
        if( !this.ignoreInputs && ( Input.GetKey( KeyCode.A ) || Input.GetAxis( "LT" ) > deadZoneTriggerLeft ) ) {

            float leftTriggerPression = 1.0f;
            if( Input.GetAxis( "LT" ) > deadZoneTriggerLeft ) {
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

        noise.pitch = Mathf.Abs( speed ) / maxSpeed * Time.timeScale;
        noise.volume = Time.timeScale;
    }

    private void HandleBrakingNoise() {

        if( !this.ignoreInputs && ( ( ( Input.GetKey( KeyCode.D ) || Input.GetAxis( "RT" ) > deadZoneTriggerRight ) && mainDir == Direction.Backward ) || ( ( Input.GetKey( KeyCode.A ) || Input.GetAxis( "LT" ) > deadZoneTriggerLeft ) && mainDir == Direction.Forward ) ) ) { 

            ////Debug.Log( "Braking" );
            if( !braking.isPlaying ) {
                braking.volume = Mathf.Abs( speed ) / maxSpeed * Time.timeScale;
                braking.time = 0.0f;
                braking.Play();
            }
            else {
                braking.volume = Mathf.Lerp( 0.0f, 1.0f, Mathf.Abs( speed ) / maxSpeed ) * Time.timeScale;
            }
        }
        else{
            braking.volume = Mathf.Lerp( braking.volume, 0.0f, brakingNoiseDecreasingSpeed * Time.deltaTime ) * Time.timeScale;

            if( braking.isPlaying && braking.volume <= 0.0f ) {
                ////Debug.Log( "Stop Braking" );
                braking.Stop();
            }
        }

        braking.pitch = Time.timeScale;
    }

    private List<Vector3> getPoints( LineSection section ) {

        List<Vector3> points = null;
        if( section.type == Type.Switch ) {

            section.ignoreSwitch = false;

            if( section.switchType == SwitchType.BiToNewBi && railSide == Rail.Right && !section.newLinesStarts.ContainsKey( Side.Right ) ) {
                points = section.floorPoints.rightLine;
                section.ignoreSwitch = true;
            } 
            else if( section.switchType == SwitchType.BiToNewBi && railSide == Rail.Left && !section.newLinesStarts.ContainsKey( Side.Left ) ) {
                points = section.floorPoints.leftLine;
                section.ignoreSwitch = true;
            }
            else {
                switch ( section.activeSwitch ) {
                    case SwitchDirection.RightToRight:          points = section.floorPoints.rightLine; 
                                                                break;
                    case SwitchDirection.RightToLeft:           points = section.floorPoints.rightLeftLine; 
                                                                break;
                    case SwitchDirection.LeftToLeft:            points = section.floorPoints.leftLine; 
                                                                break;
                    case SwitchDirection.LeftToRight:           points = section.floorPoints.leftRightLine; 
                                                                break;
                    case SwitchDirection.RightToCenter:         points = section.floorPoints.rightCenterLine; 
                                                                break;
                    case SwitchDirection.LeftToCenter:          points = section.floorPoints.leftCenterLine; 
                                                                break; 
                    case SwitchDirection.CenterToCenter:        points = section.floorPoints.centerLine; 
                                                                break;
                    case SwitchDirection.CenterToEntranceLeft:  points = section.floorPoints.centerEntranceLeft; 
                                                                break;
                    case SwitchDirection.CenterToEntranceRight: points = section.floorPoints.centerEntranceRight; 
                                                                break;
                    case SwitchDirection.CenterToExitLeft:      points = section.floorPoints.centerExitLeft; 
                                                                break;
                    case SwitchDirection.CenterToExitRight:     points = section.floorPoints.centerExitRight; 
                                                                break;
                    case SwitchDirection.LeftToEntranceLeft:    points = section.floorPoints.leftEntranceLeft; 
                                                                break;
                    case SwitchDirection.RightToEntranceRight:  points = section.floorPoints.rightEntranceRight; 
                                                                break;
                    case SwitchDirection.LeftToExitLeft:        points = section.floorPoints.leftExitLeft; 
                                                                break;
                    case SwitchDirection.RightToExitRight:      points = section.floorPoints.rightExitRight; 
                                                                break;
                }
            }
        }
        else {
            if( section.bidirectional ) {
            
                if( railSide == Rail.Left ) {
                    points = section.floorPoints.leftLine;
                }
                else if( railSide == Rail.Right ) {
                    points = section.floorPoints.rightLine;
                }
            }
            else {
                points = section.floorPoints.centerLine;
            }
        }

        return points;
    }
    
    private void CalculateNextPoint( Direction dir, List<Vector3> points, List<LineSection> sections, int actualIndex, float dist, float deltaDist, Vector3 nextOrientationPoint, Vector3 nextPoint ) {
        
        if( dir == Direction.Forward ) {
            int startingIndex = indexPoint;
            // Ricerca del punto della curva successivo più lontano della distanza (per frame) percorsa dal vagone, che diventerà
            // il punto successivo verso cui si dirigerà il vagone
            for( int j = startingIndex; j < points.Count; j++ ) {
                int indexDiff = j - startingIndex;
                //Debug.Log( "j: " + j );
                dist = ( ( points[ j ] + ( Vector3.forward * heightCorrection ) ) - this.transform.position ).magnitude;
                //Debug.Log( "dist: " + dist );

                if( j + indexDiff >= points.Count ) {
                    //Debug.Log( "Cerco punto in sezione successiva" );
                    if( actualIndex + 1 < sections.Count && sections[ actualIndex ].type != Type.Switch && sections[ actualIndex + 1 ].type != Type.Switch ) {
                        indexDiff = j + indexDiff - ( points.Count - 1 );

                        if( sections[ actualIndex + 1 ].bidirectional ) {
                            if( railSide == Rail.Left ) {
                                nextOrientationPoint = sections[ actualIndex + 1 ].floorPoints.leftLine[ indexDiff ]  + ( Vector3.forward * heightCorrection );
                            }
                            else if( railSide == Rail.Right ) {
                                nextOrientationPoint = sections[ actualIndex + 1 ].floorPoints.rightLine[ indexDiff ] + ( Vector3.forward * heightCorrection );
                            }
                        }
                        else {
                            nextOrientationPoint = sections[ actualIndex + 1 ].floorPoints.centerLine[ indexDiff ] + ( Vector3.forward * heightCorrection );
                        }
                    }
                    else {
                        nextOrientationPoint = points[ points.Count - 1 ] + ( Vector3.forward * heightCorrection );
                    }
                }
                else {
                    nextOrientationPoint = points[ j + indexDiff ] + ( Vector3.forward * heightCorrection );
                }

                if( dist > deltaDist ) {
                    indexPoint = j;
                    nextPoint = points[ j ] + ( Vector3.forward * heightCorrection );
                    //Debug.Log( "Indice nextPoint trovato: " + indexPoint );
                    break;
                }
            }
        }
    }

    IEnumerator GoForward()
    {

        bool endOfTheLine = false;

        while( !endOfTheLine ) {
        
            actualMovement = Direction.Forward;
            List<LineSection> sections = lines[ keyLine ];

// >>> STANDARD LINE NAVIGATION
            if( !inverseLine ) {
                // Ciclo le sezioni della linea in avanti
                for( int i = indexSection; i < sections.Count; i++ ) {
                    
                    // Normal line - Normal section

                    
                    List<Vector3> points = getPoints( sections[ i ] );
                    actualSectionPoints = points;
                    
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
                        for( int j = startingIndex; j < points.Count; j++ ) {
                            int indexDiff = j - startingIndex;
                            //Debug.Log( "j: " + j );
                            dist = ( points[ j ] + ( Vector3.forward * heightCorrection ) - this.transform.position ).magnitude;
                            //Debug.Log( "dist: " + dist );

                            if( j + indexDiff >= points.Count ) {
                                //Debug.Log( "Cerco punto in sezione successiva" );
                                if( i + 1 < sections.Count && sections[ i ].type != Type.Switch && sections[ i + 1 ].type != Type.Switch ) {
                                    indexDiff = j + indexDiff - ( points.Count - 1 );

                                    LineSection nextSection = sections[ i + 1 ];
                                    List<Vector3> navigationPoints = new();

                                    //if( sections[ i + 1 ].type == Type.Tunnel ) {
                                        if( sections[ i + 1 ].bidirectional ) {
                                            if( railSide == Rail.Left ) {
                                                navigationPoints = nextSection.floorPoints.leftLine;
                                            }
                                            else if( railSide == Rail.Right ) {
                                                navigationPoints = nextSection.floorPoints.rightLine;
                                            }
                                        }
                                        else {
                                            navigationPoints = nextSection.floorPoints.centerLine;
                                        }

                                        if( indexDiff >= navigationPoints.Count ) {
                                            nextOrientationPoint = navigationPoints[ navigationPoints.Count - 1 ] + ( Vector3.forward * heightCorrection );
                                        }
                                        else {
                                            nextOrientationPoint = navigationPoints[ indexDiff ] + ( Vector3.forward * heightCorrection );
                                        }

                                    //}
                                    //else {
                                        //nextOrientationPoint = sections[ i + 1 ].bezierCurveLimitedAngle[ indexDiff ];
                                    //}
                                }
                                else {
                                    nextOrientationPoint = points[ points.Count - 1 ] + ( Vector3.forward * heightCorrection );
                                }
                            }
                            else {
                                nextOrientationPoint = points[ j + indexDiff ] + ( Vector3.forward * heightCorrection );
                            }

                            if( dist > deltaDist ) {
                                indexPoint = j;
                                nextPoint = points[ j ] + ( Vector3.forward * heightCorrection );
                                //Debug.Log( "Indice nextPoint trovato: " + indexPoint );
                                break;
                            }
                        }

                        if( nextPoint == Vector3.zero ) {
                            ////Debug.Log( "Sezione terminata" );
                            break;
                        }

                        actualPoint = nextPoint;
                        actualOrientationPoint = nextOrientationPoint;
                        
                        // Posizione e rotazione iniziale del vagone per punto della curva
                        Vector3 nextDir = nextOrientationPoint - this.transform.position;
                        Vector3 startDir = this.transform.right;
                        Vector3 startPos = this.transform.position;
                        float segmentDist = ( nextPoint - startPos ).magnitude;
                        float sumDist = 0.0f;

                        Quaternion rot = Quaternion.FromToRotation( startDir, nextDir );

                        //Quaternion startRot = Quaternion.LookRotation( startDir, Vector3.forward );
                        //Quaternion nextRot = Quaternion.LookRotation( nextDir, Vector3.forward );
                        
                        while( sumDist < segmentDist ) {
                            Debug.DrawLine( this.transform.position, nextPoint, Color.red, Time.deltaTime );
                            Debug.DrawLine( this.transform.position, nextOrientationPoint, Color.magenta, Time.deltaTime );

                            // Interpolazione lineare (sulla distanza) per gestire posizione e rotazione frame successivo
                            this.transform.position = Vector3.Lerp( startPos, nextPoint, sumDist / dist );
                            //this.transform.right = Quaternion.Slerp( startRot, nextRot, sumDist / dist ) * startDir; 

                            // Protezione blocco cardanico
                            Quaternion previousRot = this.transform.localRotation;
                            this.transform.right = Vector3.Slerp( startDir, nextDir, sumDist / dist );
                            if( ( this.transform.localEulerAngles.z < ( -180.0f + gimbalLockProtectionDegs ) && this.transform.localEulerAngles.z > ( 180.0f - gimbalLockProtectionDegs ) ) || ( this.transform.localEulerAngles.z > -gimbalLockProtectionDegs && this.transform.localEulerAngles.z < gimbalLockProtectionDegs ) ) {
                                this.transform.localRotation = previousRot;
                                //Debug.Log( ">>>>>> Protezione blocco cardanico attiva" );
                            }

                            sumDist += Time.deltaTime * Mathf.Abs( speed );

                            if( sumDist < segmentDist ) {
                                yield return null;
                            }
                            else{
                                ////Debug.Log( "Next Point raggiunto" );
                                break;
                            }
                        }

                        ////Debug.Log( "While segmento terminato" );
                    }

                    if( sections[ i ].type == Type.Switch ) {
                        if( sections[ i ].switchType == SwitchType.BiToBi ) {
                            if( railSide == Rail.Right && sections[ i ].activeSwitch == SwitchDirection.RightToLeft ) {
                                railSide = Rail.Left;
                            }
                            else if( railSide == Rail.Left && sections[ i ].activeSwitch == SwitchDirection.LeftToRight ) {
                                railSide = Rail.Right;
                            }
                        }
                        else if( sections[ i ].switchType == SwitchType.BiToMono ) {
                            railSide = Rail.Center;
                        }
                        else if( sections[ i ].switchType == SwitchType.MonoToBi ) {
                            if( sections[ i ].activeSwitch == SwitchDirection.LeftToCenter ) {
                                railSide = Rail.Left;
                            }
                            else if( sections[ i ].activeSwitch == SwitchDirection.RightToCenter ) {
                                railSide = Rail.Right;
                            }
                        }
                        else if( sections[ i ].switchType == SwitchType.MonoToNewMono ) {
                            railSide = Rail.Center;

                            if( sections[ i ].activeSwitch == SwitchDirection.CenterToEntranceRight || sections[ i ].activeSwitch == SwitchDirection.CenterToEntranceLeft ) {

                                Side newSide = Side.Left;
                                if( sections[ i ].activeSwitch == SwitchDirection.CenterToEntranceRight ) {
                                    newSide = Side.Right;
                                }

                                keyLine = sections[ i ].newLinesStarts[ newSide ].lineName;
                                indexPoint = 0;
                                indexSection = 0;
                                actualSwitchIndex = -1;
                                inverseSection = false;
                                inverseLine = false;

                                break;
                            }
                        }
                        else if( sections[ i ].switchType == SwitchType.BiToNewBi ) {

                            if( !sections[ i ].ignoreSwitch ) {
                                if( sections[ i ].activeSwitch == SwitchDirection.LeftToEntranceLeft || sections[ i ].activeSwitch == SwitchDirection.RightToEntranceRight ) {

                                    Side newSide = Side.Left;
                                    railSide = Rail.Left;
                                    if( sections[ i ].activeSwitch == SwitchDirection.RightToEntranceRight ) {
                                        newSide = Side.Right;
                                        railSide = Rail.Right;
                                    }

                                    keyLine = sections[ i ].newLinesStarts[ newSide ].lineName;
                                    indexPoint = 0;
                                    indexSection = 0;
                                    actualSwitchIndex = -1;
                                    inverseSection = false;
                                    inverseLine = false;

                                    break;
                                }
                            }
                        }
                    }
                    

                    if( i < sections.Count ) {
                        indexSection++;
                        List<Vector3> nextPoints = getPoints( sections[ indexSection ] );

                        // Movimento forward: il primo punto della sezione successiva più vicino deve essere quello con indice 0, altrimenti devo ciclare la lista dei punti al contrario
                        if( ( this.transform.position - nextPoints[ 0 ] ).magnitude <= ( this.transform.position - nextPoints[ nextPoints.Count - 1 ] ).magnitude ) {
                            inverseSection = false;
                            indexPoint = 0;
                        }
                        else {
                            inverseSection = true;
                            indexPoint = sections[ indexSection ].curvePointsCount - 1;
                        }
                    }
                    else {
                        endOfTheLine = true;
                    }
                }
            }

    // >>>>>>>>>> INVERSE LINE NAVIGATION (FORWARD) <<<<<<<<<<

            else {
                // Ciclo le sezioni della linea all'indietro
                for( int i = indexSection; i >= 0; i-- ) {


                    List<Vector3> points = getPoints( sections[ i ] );
                    actualSectionPoints = points;
                    
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
                        if( mainDir == Direction.Backward ) {
                            // Se il vagone stava andando avanti, allora devo puntare all'index precedente, ma se sono
                            // già al primo punto della curva passo alla sezione precedente
                            if( indexPoint - 1 >= 0 ) {
                                indexPoint--;
                            }
                        }
                        mainDir = Direction.Forward;

                        int startingIndex = indexPoint;
                        // Ricerca del punto della curva precedente più lontano della distanza (per frame) percorsa dal vagone, che diventerà
                        // il punto successivo verso cui si dirigerà il vagone
                        for( int j = indexPoint; j >= 0; j-- ) {
                            int indexDiff = startingIndex - j;
                            dist = ( points[ j ] + ( Vector3.forward * heightCorrection ) - this.transform.position ).magnitude;

                            if( j - indexDiff < 0 ) {
                                if( i - 1 >= 0 && sections[ i ].type != Type.Switch && sections[ i - 1 ].type != Type.Switch ) {
                                    //indexDiff = ( sections[ i - 1 ].bezierCurveLimitedAngle.Count - 1 ) + ( j - indexDiff );

                                    LineSection previousSection = sections[ i - 1 ];
                                    List<Vector3> navigationPoints = new();

                                    if( sections[ i - 1 ].type == Type.Tunnel ) {
                                        if( sections[ i - 1 ].bidirectional ) {
                                            if( railSide == Rail.Left ) {
                                                indexDiff = ( sections[ i - 1 ].floorPoints.leftLine.Count - 1 ) + ( j - indexDiff );
                                                previousOrientationPoint = sections[ i - 1 ].floorPoints.leftLine[ indexDiff ] + ( Vector3.forward * heightCorrection );
                                            }
                                            else if( railSide == Rail.Right ) {
                                                indexDiff = ( sections[ i - 1 ].floorPoints.rightLine.Count - 1 ) + ( j - indexDiff );
                                                previousOrientationPoint = sections[ i - 1 ].floorPoints.rightLine[ indexDiff ] + ( Vector3.forward * heightCorrection );
                                            }
                                            
                                        }
                                        else {
                                            indexDiff = ( sections[ i - 1 ].floorPoints.centerLine.Count - 1 ) + ( j - indexDiff );
                                            previousOrientationPoint = sections[ i - 1 ].floorPoints.centerLine[ indexDiff ] + ( Vector3.forward * heightCorrection );
                                        }

                                        if( sections[ i + 1 ].bidirectional ) {
                                            if( railSide == Rail.Left ) {
                                                navigationPoints = previousSection.floorPoints.leftLine;
                                            }
                                            else if( railSide == Rail.Right ) {
                                                navigationPoints = previousSection.floorPoints.rightLine;
                                            }
                                        }
                                        else {
                                            navigationPoints = previousSection.floorPoints.centerLine;
                                        }

                                        if( indexDiff >= navigationPoints.Count ) {
                                            previousOrientationPoint = navigationPoints[ 0 ] + ( Vector3.forward * heightCorrection );
                                        }
                                        else {
                                            previousOrientationPoint = navigationPoints[ indexDiff ] + ( Vector3.forward * heightCorrection );
                                        }
                                    }
                                    else {
                                        previousOrientationPoint = sections[ i - 1 ].bezierCurveLimitedAngle[ indexDiff ];
                                    }
                                }
                                else {
                                    previousOrientationPoint = points[ 0 ] + ( Vector3.forward * heightCorrection );
                                }
                            }
                            else {
                                previousOrientationPoint = points[ j - indexDiff ] + ( Vector3.forward * heightCorrection );
                            }

                            if( dist > deltaDist ) {
                                indexPoint = j;
                                previousPoint = points[ j ] + ( Vector3.forward * heightCorrection );
                                break;
                            }
                        }
                        
                        if( previousPoint == Vector3.zero ) {
                            break;
                        }

                        actualPoint = previousPoint;
                        actualOrientationPoint = previousOrientationPoint;

                        // Posizione e rotazione iniziale del vagone per punto della curva
                        Vector3 previousDir = previousOrientationPoint - this.transform.position;
                        Vector3 startDir = this.transform.right;
                        Vector3 startPos = this.transform.position;
                        float segmentDist = ( previousPoint - startPos ).magnitude;
                        float sumDist = 0.0f;
                        while( sumDist < segmentDist ) {

                            Debug.DrawLine( this.transform.position, previousPoint, Color.red, Time.deltaTime );
                            Debug.DrawLine( this.transform.position, previousOrientationPoint, Color.magenta, Time.deltaTime );

                            // Interpolazione lineare (sulla distanza) per gestire posizione e rotazione frame successivo
                            this.transform.position = Vector3.Lerp( startPos, previousPoint, sumDist / dist ); 

                            // Protezione blocco cardanico
                            Quaternion previousRot = this.transform.localRotation;
                            this.transform.right = Vector3.Slerp( startDir, previousDir, sumDist / dist );
                            if( ( this.transform.localEulerAngles.z < ( -180.0f + gimbalLockProtectionDegs ) && this.transform.localEulerAngles.z > ( 180.0f - gimbalLockProtectionDegs ) ) || ( this.transform.localEulerAngles.z > -gimbalLockProtectionDegs && this.transform.localEulerAngles.z < gimbalLockProtectionDegs ) ) {
                                this.transform.localRotation = previousRot;
                                Debug.Log( ">>>>>> Protezione blocco cardanico attiva" );
                            }

                            sumDist += Time.deltaTime * Mathf.Abs( speed );

                            if( sumDist < segmentDist ) {
                                yield return null;
                            }
                            else{
                                Debug.Log( "Next Point raggiunto" );
                                break;
                            }
                        }
                    }

                    /* 
                    >>>>> INIZIO - GESTIONE SWITCH INTERNI ALLA LINEA STESSA
                    >>>>>
                    >>>>> FORWARD - INVERSE
                    >>>>>
                    >>>>> N.B.: Qui gestiamo anche il cambio di linea dello scambio BiToNewBi
                    */

                    if( sections[ i ].type == Type.Switch ) {
                        if( sections[ i ].switchType == SwitchType.BiToBi ) {
                            if( railSide == Rail.Right && sections[ i ].activeSwitch == SwitchDirection.LeftToRight ) {
                            railSide = Rail.Left;
                            }
                            else if( railSide == Rail.Left && sections[ i ].activeSwitch == SwitchDirection.RightToLeft ) {
                                railSide = Rail.Right;
                            }
                        }
                        else if( sections[ i ].switchType == SwitchType.BiToMono ) {
                            if( sections[ i ].activeSwitch == SwitchDirection.LeftToCenter ) {
                                railSide = Rail.Left;
                            }
                            else if( sections[ i ].activeSwitch == SwitchDirection.RightToCenter ) {
                                railSide = Rail.Right;
                            }
                        }
                        else if( sections[ i ].switchType == SwitchType.MonoToBi ) {
                            railSide = Rail.Center;
                        }
                        else if( sections[ i ].switchType == SwitchType.MonoToNewMono ) {
                            railSide = Rail.Center;

                            if( sections[ i ].activeSwitch == SwitchDirection.CenterToExitRight || sections[ i ].activeSwitch == SwitchDirection.CenterToExitLeft ) {

                                Side newSide = Side.Left;
                                if( sections[ i ].activeSwitch == SwitchDirection.CenterToExitRight ) {
                                    newSide = Side.Right;
                                }

                                keyLine = sections[ i ].newLinesStarts[ newSide ].lineName;
                                indexPoint = 0;
                                indexSection = 0;
                                actualSwitchIndex = -1;
                                inverseSection = false;
                                inverseLine = false;

                                break;
                            }
                        }
                        else if( sections[ i ].switchType == SwitchType.BiToNewBi ) {

                            if( !sections[ i ].ignoreSwitch ) {
                                if( sections[ i ].activeSwitch == SwitchDirection.LeftToExitLeft || sections[ i ].activeSwitch == SwitchDirection.RightToExitRight ) {

                                    Side newSide = Side.Left;
                                    railSide = Rail.Right;
                                    if( sections[ i ].activeSwitch == SwitchDirection.RightToExitRight ) {
                                        newSide = Side.Right;
                                        railSide = Rail.Left;
                                    }

                                    keyLine = sections[ i ].newLinesStarts[ newSide ].lineName;
                                    indexPoint = 0;
                                    indexSection = 0;
                                    actualSwitchIndex = -1;
                                    inverseSection = false;
                                    inverseLine = false;

                                    break;
                                }
                            }
                        }
                        
                    }

                    /* 
                    >>>>> FINE - GESTIONE SWITCH INTERNI ALLA LINEA STESSA
                    */

                    // Aggiornamento index una volta raggiunto il punto iniziale della curva
                    if( i > 0 ) {

                        indexSection--;
                        List<Vector3> nextPoints = getPoints( sections[ indexSection ] );
                        // Movimento backward: il primo punto della sezione successiva più vicino deve essere quello con indice (Count - 1), altrimenti devo ciclare la lista dei punti al contrario
                        if( ( this.transform.position - nextPoints[ nextPoints.Count - 1 ] ).magnitude <= ( this.transform.position - nextPoints[ 0 ] ).magnitude ) {
                            inverseSection = false;
                            indexPoint = sections[ indexSection ].curvePointsCount - 1;
                        }
                        else {
                            inverseSection = true;
                            indexPoint = 0;
                        }

                        //Debug.Log( ">>>>>>> inverseSection: " + inverseSection );
                        //Debug.Log( ">>>>>>> Previous indexPoint: " + indexPoint );
                    }
                    else {
                        if( sections[ 0 ].fromSection != null ) {
                            keyLine = sections[ 0 ].fromSection.lineName;
                            indexSection = sections[ 0 ].fromSection.sectionIndex;

                            if( sections[ 0 ].fromSection.switchType == SwitchType.MonoToNewMono ) {
                                switch( sections[ 0 ].fromSection.activeSwitch ) {
                                    case SwitchDirection.CenterToEntranceRight: inverseSection = false;
                                                                                inverseLine = true;
                                                                                indexPoint = sections[ 0 ].fromSection.floorPoints.centerEntranceRight.Count - 1;
                                                                                break;
                                    case SwitchDirection.CenterToEntranceLeft:  inverseSection = false;
                                                                                inverseLine = true;
                                                                                indexPoint = sections[ 0 ].fromSection.floorPoints.centerEntranceLeft.Count - 1;
                                                                                break;

                                    case SwitchDirection.CenterToExitRight: inverseSection = false;
                                                                            inverseLine = false;
                                                                            indexPoint = 0;
                                                                            break;
                                    case SwitchDirection.CenterToExitLeft:  inverseSection = false;
                                                                            inverseLine = false;
                                                                            indexPoint = 0;
                                                                            break;
                                }
                            }
                            else if( sections[ 0 ].fromSection.switchType == SwitchType.BiToNewBi ) {

                                if( sections[ 0 ].fromSection.newLinesStarts.ContainsKey( Side.Right ) && sections[ 0 ].fromSection.newLinesStarts[ Side.Right ].lineName == sections[ 0 ].lineName ) { 
                                    
                                    if( railSide == Rail.Right && sections[ 0 ].fromSection.activeSwitch == SwitchDirection.RightToEntranceRight ) {
                                        inverseSection = false;
                                        inverseLine = true;
                                        indexPoint = sections[ 0 ].fromSection.floorPoints.rightEntranceRight.Count - 1;
                                        railSide = Rail.Right;
                                    }
                                    else if( railSide == Rail.Left && sections[ 0 ].fromSection.activeSwitch == SwitchDirection.RightToExitRight ) {
                                        inverseSection = false;
                                        inverseLine = false;
                                        indexPoint = 0;
                                        railSide = Rail.Right;
                                    }
                                    else {
                                        // Implementare logica stop automatico treno qui
                                    }
                                }
                                else if( sections[ 0 ].fromSection.newLinesStarts.ContainsKey( Side.Left ) && sections[ 0 ].fromSection.newLinesStarts[ Side.Left ].lineName == sections[ 0 ].lineName ) { 
                                    if( railSide == Rail.Left && sections[ 0 ].fromSection.activeSwitch == SwitchDirection.LeftToEntranceLeft ) {
                                        inverseSection = false;
                                        inverseLine = true;
                                        indexPoint = sections[ 0 ].fromSection.floorPoints.leftEntranceLeft.Count - 1;
                                        railSide = Rail.Left;
                                    }
                                    else if( railSide == Rail.Right && sections[ 0 ].fromSection.activeSwitch == SwitchDirection.LeftToExitLeft ) {
                                        inverseSection = false;
                                        inverseLine = false;
                                        indexPoint = 0;
                                        railSide = Rail.Left;
                                    }
                                    else {
                                        // Implementare logica stop automatico treno qui
                                    }
                                }
                            }
                        }
                        else {
                            endOfTheLine = true;
                        }
                    }
                }
            }
        }
    }

    IEnumerator GoBackward()
    {
        bool endOfTheLine = false;

        while( !endOfTheLine ) {

            actualMovement = Direction.Backward;
            List<LineSection> sections = lines[ keyLine ];

            if( !inverseLine ) {
                // Ciclo le sezioni della linea all'indietro
                for( int i = indexSection; i >= 0; i-- ) {

                    List<Vector3> points = getPoints( sections[ i ] );
                    actualSectionPoints = points;
                    
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
                            dist = ( points[ j ] + ( Vector3.forward * heightCorrection ) - this.transform.position ).magnitude;

                            if( j - indexDiff < 0 ) {
                                if( i - 1 >= 0 && sections[ i ].type != Type.Switch && sections[ i - 1 ].type != Type.Switch ) {
                                    //indexDiff = ( sections[ i - 1 ].bezierCurveLimitedAngle.Count - 1 ) + ( j - indexDiff );

                                    //if( sections[ i - 1 ].type == Type.Tunnel ) {
                                        if( sections[ i - 1 ].bidirectional ) {
                                            if( railSide == Rail.Left ) {
                                                indexDiff = ( sections[ i - 1 ].floorPoints.leftLine.Count - 1 ) + ( j - indexDiff );
                                                previousOrientationPoint = sections[ i - 1 ].floorPoints.leftLine[ indexDiff ] + ( Vector3.forward * heightCorrection );
                                            }
                                            else if( railSide == Rail.Right ) {
                                                indexDiff = ( sections[ i - 1 ].floorPoints.rightLine.Count - 1 ) + ( j - indexDiff );
                                                previousOrientationPoint = sections[ i - 1 ].floorPoints.rightLine[ indexDiff ] + ( Vector3.forward * heightCorrection );
                                            }
                                            
                                        }
                                        else {
                                            indexDiff = ( sections[ i - 1 ].floorPoints.centerLine.Count - 1 ) + ( j - indexDiff );
                                            previousOrientationPoint = sections[ i - 1 ].floorPoints.centerLine[ indexDiff ] + ( Vector3.forward * heightCorrection );
                                        }
                                    //}
                                    //else {
                                        //previousOrientationPoint = sections[ i - 1 ].bezierCurveLimitedAngle[ indexDiff ];
                                    //}
                                }
                                else {
                                    previousOrientationPoint = points[ 0 ] + ( Vector3.forward * heightCorrection );
                                }
                            }
                            else {
                                previousOrientationPoint = points[ j - indexDiff ] + ( Vector3.forward * heightCorrection );
                            }

                            if( dist > deltaDist ) {
                                indexPoint = j;
                                previousPoint = points[ j ] + ( Vector3.forward * heightCorrection );
                                break;
                            }
                        }
                        
                        if( previousPoint == Vector3.zero ) {
                            break;
                        }

                        actualPoint = previousPoint;
                        actualOrientationPoint = previousOrientationPoint;

                        // Posizione e rotazione iniziale del vagone per punto della curva
                        Vector3 previousDir = this.transform.position - previousOrientationPoint;
                        Vector3 startDir = this.transform.right;
                        Vector3 startPos = this.transform.position;
                        float segmentDist = ( previousPoint - startPos ).magnitude;
                        float sumDist = 0.0f;
                        while( sumDist < segmentDist ) {

                            Debug.DrawLine( this.transform.position, previousPoint, Color.red, Time.deltaTime );
                            Debug.DrawLine( this.transform.position, previousOrientationPoint, Color.magenta, Time.deltaTime );

                            // Interpolazione lineare (sulla distanza) per gestire posizione e rotazione frame successivo
                            this.transform.position = Vector3.Lerp( startPos, previousPoint, sumDist / dist ); 
                            // Protezione blocco cardanico
                            Quaternion previousRot = this.transform.localRotation;
                            this.transform.right = Vector3.Slerp( startDir, previousDir, sumDist / dist );
                            if( ( this.transform.localEulerAngles.z < ( -180.0f + gimbalLockProtectionDegs ) && this.transform.localEulerAngles.z > ( 180.0f - gimbalLockProtectionDegs ) ) || ( this.transform.localEulerAngles.z > -gimbalLockProtectionDegs && this.transform.localEulerAngles.z < gimbalLockProtectionDegs ) ) {
                                this.transform.localRotation = previousRot;
                                Debug.Log( ">>>>>> Protezione blocco cardanico attiva" );
                            }

                            sumDist += Time.deltaTime * Mathf.Abs( speed );

                            if( sumDist < segmentDist ) {
                                yield return null;
                            }
                            else{
                                ////Debug.Log( "Next Point raggiunto" );
                                break;
                            }
                        }
                    }


                    // GESTIONE SCAMBI DELLA LINEA STESSA
                    if( sections[ i ].type == Type.Switch ) {
                        if( sections[ i ].switchType == SwitchType.BiToBi ) {
                            if( railSide == Rail.Right && sections[ i ].activeSwitch == SwitchDirection.LeftToRight ) {
                            railSide = Rail.Left;
                            }
                            else if( railSide == Rail.Left && sections[ i ].activeSwitch == SwitchDirection.RightToLeft ) {
                                railSide = Rail.Right;
                            }
                        }
                        else if( sections[ i ].switchType == SwitchType.BiToMono ) {
                            if( sections[ i ].activeSwitch == SwitchDirection.LeftToCenter ) {
                                railSide = Rail.Left;
                            }
                            else if( sections[ i ].activeSwitch == SwitchDirection.RightToCenter ) {
                                railSide = Rail.Right;
                            }
                        }
                        else if( sections[ i ].switchType == SwitchType.MonoToBi ) {
                            railSide = Rail.Center;
                        }
                        // GESTIONE SCAMBIO MONODIREZIONALE DA CUI PARTE UNA NUOVA LINEA MONODIREZIONALE
                        else if( sections[ i ].switchType == SwitchType.MonoToNewMono ) {
                            railSide = Rail.Center;

                            if( sections[ i ].activeSwitch == SwitchDirection.CenterToExitRight || sections[ i ].activeSwitch == SwitchDirection.CenterToExitLeft ) {

                                Side newSide = Side.Left;
                                if( sections[ i ].activeSwitch == SwitchDirection.CenterToExitRight ) {
                                    newSide = Side.Right;
                                }

                                keyLine = sections[ i ].newLinesStarts[ newSide ].lineName;
                                indexPoint = 0;
                                indexSection = 0;
                                actualSwitchIndex = -1;
                                inverseSection = false;
                                inverseLine = true;

                                break;
                            }
                        }
                        else if( sections[ i ].switchType == SwitchType.BiToNewBi ) {

                            if( !sections[ i ].ignoreSwitch ) {
                                if( sections[ i ].activeSwitch == SwitchDirection.LeftToExitLeft || sections[ i ].activeSwitch == SwitchDirection.RightToExitRight ) {

                                    Side newSide = Side.Left;
                                    railSide = Rail.Right;
                                    if( sections[ i ].activeSwitch == SwitchDirection.RightToExitRight ) {
                                        newSide = Side.Right;
                                        railSide = Rail.Left;
                                    }

                                    keyLine = sections[ i ].newLinesStarts[ newSide ].lineName;
                                    indexPoint = 0;
                                    indexSection = 0;
                                    actualSwitchIndex = -1;
                                    inverseSection = false;
                                    inverseLine = true;

                                    break;
                                }
                            }
                        }
                    }

                    //Debug.Log( "railSide: " + railSide );

                    // Aggiornamento index una volta raggiunto il punto iniziale della curva
                    if( i > 0 ) {

                        indexSection--;
                        List<Vector3> nextPoints = getPoints( sections[ indexSection ] );
                        // Movimento backward: il primo punto della sezione successiva più vicino deve essere quello con indice (Count - 1), altrimenti devo ciclare la lista dei punti al contrario
                        if( ( this.transform.position - nextPoints[ nextPoints.Count - 1 ] ).magnitude <= ( this.transform.position - nextPoints[ 0 ] ).magnitude ) {
                            inverseSection = false;
                            indexPoint = sections[ indexSection ].curvePointsCount - 1;
                        }
                        else {
                            inverseSection = true;
                            indexPoint = 0;
                        }

                        //Debug.Log( ">>>>>>> inverseSection: " + inverseSection );
                        //Debug.Log( ">>>>>>> Previous indexPoint: " + indexPoint );
                    }
                    else {
                        // GESTIONE SCAMBIO QUANDO DA UNA LINEA FIGLIA SI TORNA ALLO SCAMBIO DELLA LINEA CHE L'HA GENERATA
                        if( sections[ 0 ].fromSection != null ) {
                            keyLine = sections[ 0 ].fromSection.lineName;
                            indexSection = sections[ 0 ].fromSection.sectionIndex;

                            if( sections[ 0 ].fromSection.switchType == SwitchType.MonoToNewMono ) {

                                switch( sections[ 0 ].fromSection.activeSwitch ) {
                                    case SwitchDirection.CenterToEntranceRight: inverseSection = false;
                                                                                inverseLine = false;
                                                                                indexPoint = sections[ 0 ].fromSection.floorPoints.centerEntranceRight.Count - 1;
                                                                                break;
                                    case SwitchDirection.CenterToEntranceLeft:  inverseSection = false;
                                                                                inverseLine = false;
                                                                                indexPoint = sections[ 0 ].fromSection.floorPoints.centerEntranceLeft.Count - 1;
                                                                                break;

                                    case SwitchDirection.CenterToExitRight:     inverseSection = false;
                                                                                inverseLine = true;
                                                                                indexPoint = 0;
                                                                                break;
                                    case SwitchDirection.CenterToExitLeft:      inverseSection = false;
                                                                                inverseLine = true;
                                                                                indexPoint = 0;
                                                                                break;
                                }
                            }
                            else if( sections[ 0 ].fromSection.switchType == SwitchType.BiToNewBi ) {

                                if( sections[ 0 ].fromSection.newLinesStarts.ContainsKey( Side.Right ) && sections[ 0 ].fromSection.newLinesStarts[ Side.Right ].lineName == sections[ 0 ].lineName ) { 
                                    
                                    if( railSide == Rail.Right && sections[ 0 ].fromSection.activeSwitch == SwitchDirection.RightToEntranceRight ) {
                                        inverseSection = false;
                                        inverseLine = false;
                                        indexPoint = sections[ 0 ].fromSection.floorPoints.rightEntranceRight.Count - 1;
                                        railSide = Rail.Right;
                                    }
                                    else if( railSide == Rail.Left && sections[ 0 ].fromSection.activeSwitch == SwitchDirection.RightToExitRight ) {
                                        inverseSection = false;
                                        inverseLine = true;
                                        indexPoint = 0;
                                        railSide = Rail.Right;
                                    }
                                    else {
                                        // Implementare logica stop automatico treno qui
                                    }
                                }
                                else if( sections[ 0 ].fromSection.newLinesStarts.ContainsKey( Side.Left ) && sections[ 0 ].fromSection.newLinesStarts[ Side.Left ].lineName == sections[ 0 ].lineName ) { 
                                    if( railSide == Rail.Left && sections[ 0 ].fromSection.activeSwitch == SwitchDirection.LeftToEntranceLeft ) {
                                        inverseSection = false;
                                        inverseLine = false;
                                        indexPoint = sections[ 0 ].fromSection.floorPoints.leftEntranceLeft.Count - 1;
                                        railSide = Rail.Left;
                                    }
                                    else if( railSide == Rail.Right && sections[ 0 ].fromSection.activeSwitch == SwitchDirection.LeftToExitLeft ) {
                                        inverseSection = false;
                                        inverseLine = true;
                                        indexPoint = 0;
                                        railSide = Rail.Left;
                                    }
                                    else {
                                        // Implementare logica stop automatico treno qui
                                    }
                                }
                            }
                        }
                        else {
                            endOfTheLine = true;
                        }
                    }
                }
            }

            // >>> INVERSE LINE <<<
            else {
                for( int i = indexSection; i < sections.Count; i++ ) {

                    // >>> NORMAL SECTION <<<
                    List<Vector3> points = getPoints( sections[ i ] );
                    actualSectionPoints = points;
                    
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
                        if( mainDir == Direction.Forward ) {

                            // Se il vagone stava andando indietro, allora devo puntare all'index successivo, ma se sono
                            // già all'ultimo punto della curva passo alla sezione succcessiva
                            if( indexPoint + 1 < points.Count ) {
                                indexPoint++;
                            }
                        }
                        mainDir = Direction.Backward;

                        int startingIndex = indexPoint;
                        // Ricerca del punto della curva successivo più lontano della distanza (per frame) percorsa dal vagone, che diventerà
                        // il punto successivo verso cui si dirigerà il vagone
                        for( int j = startingIndex; j < points.Count; j++ ) {
                            int indexDiff = j - startingIndex;
                            //Debug.Log( "j: " + j );
                            dist = ( points[ j ] + ( Vector3.forward * heightCorrection ) - this.transform.position ).magnitude;
                            //Debug.Log( "dist: " + dist );

                            if( j + indexDiff >= points.Count ) {
                                //Debug.Log( "Cerco punto in sezione successiva" );
                                if( i + 1 < sections.Count && sections[ i ].type != Type.Switch && sections[ i + 1 ].type != Type.Switch ) {
                                    indexDiff = j + indexDiff - ( points.Count - 1 );

                                    if( sections[ i + 1 ].type == Type.Tunnel ) {
                                        if( sections[ i + 1 ].bidirectional ) {
                                            if( railSide == Rail.Left ) {
                                                nextOrientationPoint = sections[ i + 1 ].floorPoints.leftLine[ indexDiff ]  + ( Vector3.forward * heightCorrection );
                                            }
                                            else if( railSide == Rail.Right ) {
                                                nextOrientationPoint = sections[ i + 1 ].floorPoints.rightLine[ indexDiff ] + ( Vector3.forward * heightCorrection );
                                            }
                                        }
                                        else {
                                            nextOrientationPoint = sections[ i + 1 ].floorPoints.centerLine[ indexDiff ] + ( Vector3.forward * heightCorrection );
                                        }
                                    }
                                    else {
                                        nextOrientationPoint = sections[ i + 1 ].bezierCurveLimitedAngle[ indexDiff ];
                                    }
                                }
                                else {
                                    nextOrientationPoint = points[ points.Count - 1 ] + ( Vector3.forward * heightCorrection );
                                }
                            }
                            else {
                                nextOrientationPoint = points[ j + indexDiff ] + ( Vector3.forward * heightCorrection );
                            }

                            if( dist > deltaDist ) {
                                indexPoint = j;
                                nextPoint = points[ j ] + ( Vector3.forward * heightCorrection );
                                //Debug.Log( "Indice nextPoint trovato: " + indexPoint );
                                break;
                            }
                        }

                        if( nextPoint == Vector3.zero ) {
                            ////Debug.Log( "Sezione terminata" );
                            break;
                        }

                        actualPoint = nextPoint;
                        actualOrientationPoint = nextOrientationPoint;
                        
                        // Posizione e rotazione iniziale del vagone per punto della curva
                        Vector3 nextDir = this.transform.position - nextOrientationPoint;
                        Vector3 startDir = this.transform.right;
                        Vector3 startPos = this.transform.position;
                        float segmentDist = ( nextPoint - startPos ).magnitude;
                        float sumDist = 0.0f;
                        while( sumDist < segmentDist ) {
                            Debug.DrawLine( this.transform.position, nextPoint, Color.red, Time.deltaTime );
                            Debug.DrawLine( this.transform.position, nextOrientationPoint, Color.magenta, Time.deltaTime );

                            // Interpolazione lineare (sulla distanza) per gestire posizione e rotazione frame successivo
                            this.transform.position = Vector3.Lerp( startPos, nextPoint, sumDist / dist ); 
                            // Protezione blocco cardanico
                            Quaternion previousRot = this.transform.localRotation;
                            this.transform.right = Vector3.Slerp( startDir, nextDir, sumDist / dist );
                            if( ( this.transform.localEulerAngles.z < ( -180.0f + gimbalLockProtectionDegs ) && this.transform.localEulerAngles.z > ( 180.0f - gimbalLockProtectionDegs ) ) || ( this.transform.localEulerAngles.z > -gimbalLockProtectionDegs && this.transform.localEulerAngles.z < gimbalLockProtectionDegs ) ) {
                                this.transform.localRotation = previousRot;
                                Debug.Log( ">>>>>> Protezione blocco cardanico attiva" );
                            }

                            sumDist += Time.deltaTime * Mathf.Abs( speed );

                            if( sumDist < segmentDist ) {
                                yield return null;
                            }
                            else{
                                ////Debug.Log( "Next Point raggiunto" );
                                break;
                            }
                        }

                        ////Debug.Log( "While segmento terminato" );
                    }

                    if( sections[ i ].type == Type.Switch ) {
                        if( sections[ i ].switchType == SwitchType.BiToBi ) {
                            if( railSide == Rail.Right && sections[ i ].activeSwitch == SwitchDirection.RightToLeft ) {
                                railSide = Rail.Left;
                            }
                            else if( railSide == Rail.Left && sections[ i ].activeSwitch == SwitchDirection.LeftToRight ) {
                                railSide = Rail.Right;
                            }
                        }
                        else if( sections[ i ].switchType == SwitchType.BiToMono ) {
                            railSide = Rail.Center;
                        }
                        else if( sections[ i ].switchType == SwitchType.MonoToBi ) {
                            if( sections[ i ].activeSwitch == SwitchDirection.LeftToCenter ) {
                                railSide = Rail.Left;
                            }
                            else if( sections[ i ].activeSwitch == SwitchDirection.RightToCenter ) {
                                railSide = Rail.Right;
                            }
                        }
                        else if( sections[ i ].switchType == SwitchType.MonoToNewMono ) {
                            railSide = Rail.Center;

                            if( sections[ i ].activeSwitch == SwitchDirection.CenterToEntranceRight || sections[ i ].activeSwitch == SwitchDirection.CenterToEntranceLeft ) {

                                Side newSide = Side.Left;
                                if( sections[ i ].activeSwitch == SwitchDirection.CenterToEntranceRight ) {
                                    newSide = Side.Right;
                                }

                                keyLine = sections[ i ].newLinesStarts[ newSide ].lineName;
                                indexPoint = 0;
                                indexSection = 0;
                                actualSwitchIndex = -1;
                                inverseSection = false;
                                inverseLine = true;

                                break;
                            }
                        }
                        else if( sections[ i ].switchType == SwitchType.BiToNewBi ) {

                            if( !sections[ i ].ignoreSwitch ) {
                                if( sections[ i ].activeSwitch == SwitchDirection.LeftToEntranceLeft || sections[ i ].activeSwitch == SwitchDirection.RightToEntranceRight ) {

                                    Side newSide = Side.Left;
                                    railSide = Rail.Left;
                                    if( sections[ i ].activeSwitch == SwitchDirection.RightToEntranceRight ) {
                                        newSide = Side.Right;
                                        railSide = Rail.Right;
                                    }

                                    keyLine = sections[ i ].newLinesStarts[ newSide ].lineName;
                                    indexPoint = 0;
                                    indexSection = 0;
                                    actualSwitchIndex = -1;
                                    inverseSection = false;
                                    inverseLine = true;

                                    break;
                                }
                            }
                        }
                    }

                    if( i < sections.Count ) {
                        indexSection++;
                        List<Vector3> nextPoints = getPoints( sections[ indexSection ] );

                        // Movimento forward: il primo punto della sezione successiva più vicino deve essere quello con indice 0, altrimenti devo ciclare la lista dei punti al contrario
                        if( ( this.transform.position - nextPoints[ 0 ] ).magnitude <= ( this.transform.position - nextPoints[ nextPoints.Count - 1 ] ).magnitude ) {
                            inverseSection = false;
                            indexPoint = 0;
                        }
                        else {
                            inverseSection = true;
                            indexPoint = sections[ indexSection ].curvePointsCount - 1;
                        }

                        //Debug.Log( ">>>>>>> inverseSection: " + inverseSection );
                        //Debug.Log( ">>>>>>> Previous indexPoint: " + indexPoint );
                    }
                    else {
                        endOfTheLine = true;
                    }
                }
        
            }
        }
    }

    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red;
    //     LineSection segment = lines[ keyLine ][ indexSection];
    //     if( segment.type == Type.Switch ) {
            
    //         if( segment.switchType == SwitchType.MonoToNewMono ) {

    //             if( segment.activeSwitch == SwitchDirection.CenterToEntranceRight) {
    //                 Gizmos.DrawWireSphere( segment.floorPoints.centerEntranceRight[ 0 ], 10 );
    //             }
    //             else if( segment.activeSwitch == SwitchDirection.CenterToExitRight) {
    //                 Gizmos.DrawWireSphere( segment.floorPoints.centerExitRight[ 0 ], 10 );
    //             }
    //             else if( segment.activeSwitch == SwitchDirection.CenterToEntranceLeft) {
    //                 Gizmos.DrawWireSphere( segment.floorPoints.centerEntranceLeft[ 0 ], 10 );
    //             }
    //             else if( segment.activeSwitch == SwitchDirection.CenterToExitLeft) {
    //                 Gizmos.DrawWireSphere( segment.floorPoints.centerExitLeft[ 0 ], 10 );
    //             }
    //         }
    //     }

    //     if( actualSectionPoints != null ) {
    //         for( int i = 1; i < actualSectionPoints.Count; i++ ) {
    //             Gizmos.color = Color.yellow;
    //             Gizmos.DrawSphere( actualSectionPoints[ i ], 0.5f );
    //         }

    //         Gizmos.color = Color.green;
    //         Gizmos.DrawSphere( actualPoint, 1.0f );
    //         Gizmos.color = Color.red;
    //         Gizmos.DrawSphere( actualOrientationPoint, 0.75f );
    //     }
    // }
}
