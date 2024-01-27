using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailHighlighter : MonoBehaviour
{
    private GameObject train = null;
    public Material highlightedMaterial;
    public Material originalMaterial;
    public float lightWaveSpeed = 1.0f;
    public Color lightColor = Color.yellow;
    public LineSection section;
    public SwitchDirection direction;

    public string lineName;
    public int index;

    void Start()
    {
        lineName = section.lineName;
        index = section.sectionIndex;

        highlightedMaterial = ( Material )Resources.Load( "Materials/Rust Highlighted", typeof( Material ) );
        originalMaterial = this.GetComponent<Renderer>().material;
    }

    void LateUpdate()
    {
        if( train == null ) {
            train = GameObject.Find( "Train" );
        }
        else { 
            TrainController trainController = train.GetComponent<TrainController>();
            
            if( ( trainController.actualSwitchLine == lineName && trainController.actualSwitchIndex == index && trainController.actualSwitchDirection == direction ) || 
                ( trainController.previousSwitchLine == lineName && trainController.previousSwitchIndex == index && trainController.previousSwitchDirection == direction ) || 
                ( trainController.nextSwitchLine == lineName && trainController.nextSwitchIndex == index && trainController.nextSwitchDirection == direction ) ) {
                
                this.highlightedMaterial.SetColor( "_EmissionColor", Color.Lerp( Color.black, lightColor, Mathf.PingPong( Time.time, lightWaveSpeed ) ) );
                this.highlightedMaterial.EnableKeyword( "_EMISSION" );
                this.GetComponent<Renderer>().material = this.highlightedMaterial; 
            }
            else {
                this.GetComponent<Renderer>().sharedMaterial = this.originalMaterial;
            }
        }
    }
}
