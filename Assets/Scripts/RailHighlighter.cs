using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailHighlighter : MonoBehaviour
{
    private GameObject train = null;
    public Material highlightedMaterial;
    public Material originalMaterial;
    public float lightWaveSpeed = 1.0f;
    public float minEmission = -2.0f;
    private float actualEmission;
    public float maxEmission = 1.0f;

    private bool goingUp;

    public string line;
    public int index;
    public SwitchDirection direction;

    // Start is called before the first frame update
    void Start()
    {
        highlightedMaterial = ( Material )Resources.Load( "Materials/Rust Highlighted", typeof( Material ) );
        originalMaterial = this.GetComponent<Renderer>().material;
        //actualEmission = minEmission;
        //goingUp = true;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if( train == null ) {
            train = GameObject.Find( "Train" );
        }
        else { 
            TrainController trainController = train.GetComponent<TrainController>();
            
            if( ( trainController.actualSwitchLine == line && trainController.actualSwitchIndex == index && trainController.actualSwitchDirection == direction ) || 
                ( trainController.previousSwitchLine == line && trainController.previousSwitchIndex == index && trainController.previousSwitchDirection == direction ) || 
                ( trainController.nextSwitchLine == line && trainController.nextSwitchIndex == index && trainController.nextSwitchDirection == direction ) ) {

                this.highlightedMaterial.SetColor("_EmissionColor", Color.Lerp( Color.black, Color.blue, Mathf.PingPong( Time.time, 1.0f ) ) );
                this.highlightedMaterial.EnableKeyword("_EMISSION");
                this.GetComponent<Renderer>().material = this.highlightedMaterial; 

                /*float delta = lightWaveSpeed * Time.deltaTime;
                actualEmission += goingUp ? delta : -delta;

                if( actualEmission > maxEmission ) {
                    goingUp = false;
                }
                else if( actualEmission < minEmission ) {
                    goingUp = true;
                }

                Debug.Log( ">>> actualEmission: " + actualEmission );*/
            }
            else {
                this.GetComponent<Renderer>().sharedMaterial = this.originalMaterial;
            }
        }
    }
}
