using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchPath : ScriptableObject
{
    private float switchLenght;
    private float centerWidth;
    private float tunnelWidth;
    private float switchLightDistance;
    private float switchLightHeight;
    private Vector3 switchLightRotation;
    private GameObject switchLight;
    private int curvePointsNumber;

    public static SwitchPath CreateInstance( float switchLenght, float centerWidth, float tunnelWidth, float switchLightDistance, float switchLightHeight, int curvePointsNumber, Vector3 switchLightRotation, GameObject switchLight ) {
        
        var switchPathScript = ScriptableObject.CreateInstance<SwitchPath>();
        switchPathScript.switchLenght = switchLenght;
        switchPathScript.centerWidth = centerWidth;
        switchPathScript.tunnelWidth = tunnelWidth;
        switchPathScript.switchLightDistance = switchLightDistance;
        switchPathScript.switchLightHeight = switchLightHeight;
        switchPathScript.curvePointsNumber = curvePointsNumber;
        switchPathScript.switchLightRotation = switchLightRotation;
        switchPathScript.switchLight = switchLight;

        return switchPathScript;
    }

    public LineSection generateBiToBiSwitch( int index, List<LineSection> sections, Vector3 startingDir, Vector3 startingPoint, GameObject sectionGameObj ) {

        LineSection section = new LineSection();
        section.type = Type.Switch;
        section.switchType = SwitchType.BiToBi;
        section.bidirectional = true;

        MeshGenerator.Floor switchFloor = new MeshGenerator.Floor();
        Vector3 switchDir = startingDir.normalized;

        Dictionary<SwitchDirection, List<GameObject>> switchLights = new Dictionary<SwitchDirection, List<GameObject>>();

        Vector3 c0 = startingPoint;
        Vector3 r0 = c0 + Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * switchDir * ( ( this.centerWidth / 2 ) + ( this.tunnelWidth / 2 ) ); 
        Vector3 l0 = c0 + Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir * ( (this.centerWidth / 2 ) + ( this.tunnelWidth / 2 ) );

        Vector3 lb = l0 + switchDir * ( this.switchLenght / 2 ); 
        Vector3 rb = r0 + switchDir * ( this.switchLenght / 2 ); 

        Vector3 c1 = c0 + switchDir * this.switchLenght;
        Vector3 l1 = l0 + switchDir * this.switchLenght; 
        Vector3 r1 = r0 + switchDir * this.switchLenght;

        GameObject lightR0 = Instantiate( this.switchLight, r0 + ( Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * startingDir.normalized * this.switchLightDistance ) - Vector3.forward * this.switchLightHeight, Quaternion.Euler( this.switchLightRotation.x + Vector3.SignedAngle( startingDir, Vector3.right, -Vector3.forward ) - 180.0f , this.switchLightRotation.y, this.switchLightRotation.z ) );
        lightR0.transform.parent = sectionGameObj.transform;
        lightR0.name = "Semaforo R0";
        GameObject lightR1 = Instantiate( this.switchLight, r1 + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * startingDir.normalized * this.switchLightDistance ) - Vector3.forward * this.switchLightHeight, Quaternion.Euler( this.switchLightRotation.x + Vector3.SignedAngle( startingDir, Vector3.right, -Vector3.forward ), this.switchLightRotation.y, this.switchLightRotation.z ) );
        lightR1.transform.parent = sectionGameObj.transform;
        lightR1.name = "Semaforo R1";
        GameObject lightL0 = Instantiate( this.switchLight, l0 + ( Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * startingDir.normalized * this.switchLightDistance ) - Vector3.forward * this.switchLightHeight, Quaternion.Euler( this.switchLightRotation.x + Vector3.SignedAngle( startingDir, Vector3.right, -Vector3.forward ) - 180.0f, this.switchLightRotation.y, this.switchLightRotation.z ) );
        lightL0.transform.parent = sectionGameObj.transform;
        lightL0.name = "Semaforo L0";
        GameObject lightL1 = Instantiate( this.switchLight, l1 + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * startingDir.normalized * this.switchLightDistance ) - Vector3.forward * this.switchLightHeight, Quaternion.Euler( this.switchLightRotation.x + Vector3.SignedAngle( startingDir, Vector3.right, -Vector3.forward ), this.switchLightRotation.y, this.switchLightRotation.z ) );
        lightL1.transform.parent = sectionGameObj.transform;
        lightL1.name = "Semaforo L1";
        switchLights.Add( SwitchDirection.Right, new List<GameObject>{ lightR0, lightR1 } );
        switchLights.Add( SwitchDirection.RightToLeft, new List<GameObject>{ lightR0, lightL1 } );
        switchLights.Add( SwitchDirection.Left, new List<GameObject>{ lightL0, lightL1 } );
        switchLights.Add( SwitchDirection.LeftToRight, new List<GameObject>{ lightL0, lightR1 } );
        section.switchLights = switchLights;

        List<Vector3> rightLeftLine = BezierCurveCalculator.CalculateBezierCurve( new List<Vector3>{ r0, rb, lb, l1 }, this.curvePointsNumber );
        List<Vector3> leftRightLine = BezierCurveCalculator.CalculateBezierCurve( new List<Vector3>{ l0, lb, rb, r1 }, this.curvePointsNumber );

        switchFloor.leftRightLine = leftRightLine;
        switchFloor.rightLeftLine = rightLeftLine;

        switchFloor.leftLine = new List<Vector3>{ l0, lb, l1 };
        switchFloor.centerLine = new List<Vector3>{ c0, c1 };
        switchFloor.rightLine = new List<Vector3>{ r0, rb, r1 };

        List<Vector3> nextStartingDirections = new List<Vector3>();
        nextStartingDirections.Add( startingDir );
        section.nextStartingDirections = nextStartingDirections;

        List<Vector3> nextStartingPoints = new List<Vector3>();
        nextStartingPoints.Add( c1 );
        section.nextStartingPoints = nextStartingPoints;

        section.nextStartingDirections = nextStartingDirections; 
        section.nextStartingPoints =  nextStartingPoints;
        section.floorPoints = switchFloor;

        section.activeSwitch = SwitchDirection.Right;
        section.curvePointsCount = switchFloor.rightLine.Count;
        section.floorPoints = switchFloor;

        return section;
    }

    public LineSection generateBiToMonoSwitch( int index, List<LineSection> sections, Vector3 startingDir, Vector3 startingPoint, GameObject sectionGameObj ) {

        LineSection section = new LineSection();
        section.type = Type.Switch;
        section.switchType = SwitchType.BiToMono;
        section.bidirectional =  false;

        MeshGenerator.Floor switchFloor = new MeshGenerator.Floor();
        Vector3 switchDir = startingDir.normalized;

        Dictionary<SwitchDirection, List<GameObject>> switchLights = new Dictionary<SwitchDirection, List<GameObject>>();
        Vector3 c0, r0, l0, cb, rb, lb, c1, r1, l1;
        List<Vector3> nextStartingPoints = new List<Vector3>();
        c0 = startingPoint;
        cb = c0 + switchDir * ( this.switchLenght / 2 );
        c1 = c0 + switchDir * this.switchLenght;

        int variant = Random.Range( 0, 3 );
        if( variant == 0 ) { // Allineamento centrale
            r0 = c0 + Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * switchDir * ( ( this.centerWidth / 2 ) + ( this.tunnelWidth / 2 ) ); 
            l0 = c0 + Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir * ( (this.centerWidth / 2 ) + ( this.tunnelWidth / 2 ) );

            cb = c0 + switchDir * ( this.switchLenght / 2 );
            lb = l0 + switchDir * ( this.switchLenght / 2 ); 
            rb = r0 + switchDir * ( this.switchLenght / 2 ); 

            GameObject lightR0 = Instantiate( this.switchLight, r0 + ( Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * startingDir.normalized * this.switchLightDistance ) - Vector3.forward * this.switchLightHeight, Quaternion.Euler( this.switchLightRotation.x + Vector3.SignedAngle( startingDir, Vector3.right, -Vector3.forward ) - 180.0f , this.switchLightRotation.y, this.switchLightRotation.z ) );
            lightR0.transform.parent = sectionGameObj.transform;
            lightR0.name = "Semaforo R0";
            GameObject lightL0 = Instantiate( this.switchLight, l0 + ( Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * startingDir.normalized * this.switchLightDistance ) - Vector3.forward * this.switchLightHeight, Quaternion.Euler( this.switchLightRotation.x + Vector3.SignedAngle( startingDir, Vector3.right, -Vector3.forward ) - 180.0f, this.switchLightRotation.y, this.switchLightRotation.z ) );
            lightL0.transform.parent = sectionGameObj.transform;
            lightL0.name = "Semaforo L0";
            GameObject lightC1 = Instantiate( this.switchLight, c1 + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * startingDir.normalized * this.switchLightDistance ) - Vector3.forward * this.switchLightHeight, Quaternion.Euler( this.switchLightRotation.x + Vector3.SignedAngle( startingDir, Vector3.right, -Vector3.forward ), this.switchLightRotation.y, this.switchLightRotation.z ) );
            lightC1.transform.parent = sectionGameObj.transform;
            lightC1.name = "Semaforo C1";
            switchLights.Add( SwitchDirection.RightToCenter, new List<GameObject>{ lightR0, lightC1 } );
            switchLights.Add( SwitchDirection.LeftToCenter, new List<GameObject>{ lightL0, lightC1 } );
            section.switchLights = switchLights;

            List<Vector3> rightCenterLine = BezierCurveCalculator.CalculateBezierCurve( new List<Vector3>{ r0, rb, cb, c1 }, this.curvePointsNumber );
            List<Vector3> leftCenterLine = BezierCurveCalculator.CalculateBezierCurve( new List<Vector3>{ l0, lb, cb, c1 }, this.curvePointsNumber );

            switchFloor.rightCenterLine = rightCenterLine;
            switchFloor.centerLine = new List<Vector3>{ c0, cb, c1 };
            switchFloor.leftCenterLine = leftCenterLine;

            nextStartingPoints.Add( c1 );
            section.nextStartingPoints = nextStartingPoints;
        }
        else if( variant == 1 ) { // Allineamento sinistro
            r0 = c0 + Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * switchDir * ( ( this.centerWidth / 2 ) + ( this.tunnelWidth / 2 ) ); 
            l0 = c0 + Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir * ( (this.centerWidth / 2 ) + ( this.tunnelWidth / 2 ) );
            l1 = l0 + switchDir * this.switchLenght;

            cb = c0 + switchDir * ( this.switchLenght / 2 );
            lb = l0 + switchDir * ( this.switchLenght / 2 ); 
            rb = r0 + switchDir * ( this.switchLenght / 2 ); 

            GameObject lightR0 = Instantiate( this.switchLight, r0 + ( Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * startingDir.normalized * this.switchLightDistance ) - Vector3.forward * this.switchLightHeight, Quaternion.Euler( this.switchLightRotation.x + Vector3.SignedAngle( startingDir, Vector3.right, -Vector3.forward ) - 180.0f , this.switchLightRotation.y, this.switchLightRotation.z ) );
            lightR0.transform.parent = sectionGameObj.transform;
            lightR0.name = "Semaforo R0";
            GameObject lightL0 = Instantiate( this.switchLight, l0 + ( Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * startingDir.normalized * this.switchLightDistance ) - Vector3.forward * this.switchLightHeight, Quaternion.Euler( this.switchLightRotation.x + Vector3.SignedAngle( startingDir, Vector3.right, -Vector3.forward ) - 180.0f, this.switchLightRotation.y, this.switchLightRotation.z ) );
            lightL0.transform.parent = sectionGameObj.transform;
            lightL0.name = "Semaforo L0";
            GameObject lightL1 = Instantiate( this.switchLight, l1 + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * startingDir.normalized * this.switchLightDistance ) - Vector3.forward * this.switchLightHeight, Quaternion.Euler( this.switchLightRotation.x + Vector3.SignedAngle( startingDir, Vector3.right, -Vector3.forward ), this.switchLightRotation.y, this.switchLightRotation.z ) );
            lightL1.transform.parent = sectionGameObj.transform;
            lightL1.name = "Semaforo L1";
            switchLights.Add( SwitchDirection.RightToCenter, new List<GameObject>{ lightR0, lightL1 } );
            switchLights.Add( SwitchDirection.LeftToCenter, new List<GameObject>{ lightL0, lightL1 } );
            section.switchLights = switchLights;

            List<Vector3> rightCenterLine = BezierCurveCalculator.CalculateBezierCurve( new List<Vector3>{ r0, rb, lb, l1 }, this.curvePointsNumber );
            List<Vector3> leftCenterLine = new List<Vector3>{ l0, lb, l1 };

            switchFloor.rightCenterLine = rightCenterLine;
            switchFloor.centerLine = new List<Vector3>{ c0, cb, c1 };
            switchFloor.leftCenterLine = leftCenterLine;

            nextStartingPoints.Add( l1 );
            section.nextStartingPoints = nextStartingPoints;
        }
        else if( variant == 2 ) { // Allineamento destro
            r0 = c0 + Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * switchDir * ( ( this.centerWidth / 2 ) + ( this.tunnelWidth / 2 ) ); 
            l0 = c0 + Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir * ( (this.centerWidth / 2 ) + ( this.tunnelWidth / 2 ) );
            r1 = r0 + switchDir * this.switchLenght;

            cb = c0 + switchDir * ( this.switchLenght / 2 );
            lb = l0 + switchDir * ( this.switchLenght / 2 ); 
            rb = r0 + switchDir * ( this.switchLenght / 2 ); 

            GameObject lightR0 = Instantiate( this.switchLight, r0 + ( Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * startingDir.normalized * this.switchLightDistance ) - Vector3.forward * this.switchLightHeight, Quaternion.Euler( this.switchLightRotation.x + Vector3.SignedAngle( startingDir, Vector3.right, -Vector3.forward ) - 180.0f , this.switchLightRotation.y, this.switchLightRotation.z ) );
            lightR0.transform.parent = sectionGameObj.transform;
            lightR0.name = "Semaforo R0";
            GameObject lightL0 = Instantiate( this.switchLight, l0 + ( Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * startingDir.normalized * this.switchLightDistance ) - Vector3.forward * this.switchLightHeight, Quaternion.Euler( this.switchLightRotation.x + Vector3.SignedAngle( startingDir, Vector3.right, -Vector3.forward ) - 180.0f, this.switchLightRotation.y, this.switchLightRotation.z ) );
            lightL0.transform.parent = sectionGameObj.transform;
            lightL0.name = "Semaforo L0";
            GameObject lightR1 = Instantiate( this.switchLight, r1 + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * startingDir.normalized * this.switchLightDistance ) - Vector3.forward * this.switchLightHeight, Quaternion.Euler( this.switchLightRotation.x + Vector3.SignedAngle( startingDir, Vector3.right, -Vector3.forward ), this.switchLightRotation.y, this.switchLightRotation.z ) );
            lightR1.transform.parent = sectionGameObj.transform;
            lightR1.name = "Semaforo R1";
            switchLights.Add( SwitchDirection.RightToCenter, new List<GameObject>{ lightR0, lightR1 } );
            switchLights.Add( SwitchDirection.LeftToCenter, new List<GameObject>{ lightL0, lightR1 } );
            section.switchLights = switchLights;

            List<Vector3> rightCenterLine = new List<Vector3>{ r0, rb, r1 };
            List<Vector3> leftCenterLine = BezierCurveCalculator.CalculateBezierCurve( new List<Vector3>{ l0, lb, rb, r1 }, this.curvePointsNumber );

            switchFloor.rightCenterLine = rightCenterLine;
            switchFloor.centerLine = new List<Vector3>{ c0, cb, c1 };
            switchFloor.leftCenterLine = leftCenterLine;

            nextStartingPoints.Add( r1 );
            section.nextStartingPoints = nextStartingPoints;
        }

        List<Vector3> nextStartingDirections = new List<Vector3>();
        nextStartingDirections.Add( startingDir );
        section.nextStartingDirections = nextStartingDirections;

        section.nextStartingDirections = nextStartingDirections; 
        section.nextStartingPoints =  nextStartingPoints;
        section.floorPoints = switchFloor;

        section.activeSwitch = SwitchDirection.RightToCenter;
        section.curvePointsCount = switchFloor.rightCenterLine.Count;
        section.floorPoints = switchFloor;

        return section;
    }

    public LineSection generateMonoToBiSwitch( int index, List<LineSection> sections, Vector3 startingDir, Vector3 startingPoint, GameObject sectionGameObj ) {

        LineSection section = new LineSection();
        section.type = Type.Switch;
        section.switchType = SwitchType.MonoToBi;
        section.bidirectional =  true;

        MeshGenerator.Floor switchFloor = new MeshGenerator.Floor();
        Vector3 switchDir = startingDir.normalized;

         Dictionary<SwitchDirection, List<GameObject>> switchLights = new Dictionary<SwitchDirection, List<GameObject>>();
        Vector3 c0, r0, l0, cb, rb, lb, c1, r1, l1;
        List<Vector3> nextStartingPoints = new List<Vector3>();

        int variant = Random.Range( 0, 3 );
        if( variant == 0 ) { // Allineamento centrale
            c0 = startingPoint;
            cb = c0 + switchDir * ( this.switchLenght / 2 );
            c1 = c0 + switchDir * this.switchLenght;

            r1 = c1 + Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * switchDir * ( ( this.centerWidth / 2 ) + ( this.tunnelWidth / 2 ) ); 
            l1 = c1 + Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir * ( (this.centerWidth / 2 ) + ( this.tunnelWidth / 2 ) );

            lb = l1 - switchDir * ( this.switchLenght / 2 ); 
            rb = r1 - switchDir * ( this.switchLenght / 2 ); 

            GameObject lightR1 = Instantiate( this.switchLight, r1 + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * startingDir.normalized * this.switchLightDistance ) - Vector3.forward * this.switchLightHeight, Quaternion.Euler( this.switchLightRotation.x + Vector3.SignedAngle( startingDir, Vector3.right, -Vector3.forward ) , this.switchLightRotation.y, this.switchLightRotation.z ) );
            lightR1.transform.parent = sectionGameObj.transform;
            lightR1.name = "Semaforo R1";
            GameObject lightL1 = Instantiate( this.switchLight, l1 + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * startingDir.normalized * this.switchLightDistance ) - Vector3.forward * this.switchLightHeight, Quaternion.Euler( this.switchLightRotation.x + Vector3.SignedAngle( startingDir, Vector3.right, -Vector3.forward ), this.switchLightRotation.y, this.switchLightRotation.z ) );
            lightL1.transform.parent = sectionGameObj.transform;
            lightL1.name = "Semaforo L1";
            GameObject lightC0 = Instantiate( this.switchLight, c0 + ( Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * startingDir.normalized * this.switchLightDistance ) - Vector3.forward * this.switchLightHeight, Quaternion.Euler( this.switchLightRotation.x + Vector3.SignedAngle( startingDir, Vector3.right, -Vector3.forward ) - 180.0f, this.switchLightRotation.y, this.switchLightRotation.z ) );
            lightC0.transform.parent = sectionGameObj.transform;
            lightC0.name = "Semaforo C0";
            switchLights.Add( SwitchDirection.RightToCenter, new List<GameObject>{ lightC0, lightR1 } );
            switchLights.Add( SwitchDirection.LeftToCenter, new List<GameObject>{ lightC0, lightL1 } );
            section.switchLights = switchLights;

            List<Vector3> rightCenterLine = BezierCurveCalculator.CalculateBezierCurve( new List<Vector3>{ c0, cb, rb, r1 }, this.curvePointsNumber );
            List<Vector3> leftCenterLine = BezierCurveCalculator.CalculateBezierCurve( new List<Vector3>{ c0, cb, lb, l1 }, this.curvePointsNumber );

            switchFloor.rightCenterLine = rightCenterLine;
            switchFloor.centerLine = new List<Vector3>{ c0, cb, c1 };
            switchFloor.leftCenterLine = leftCenterLine;

            nextStartingPoints.Add( c1 );
            section.nextStartingPoints = nextStartingPoints;
        }
        else if( variant == 1 ) { // Allineamento sinistro
            l0 = startingPoint;
            c0 = l0 + Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * switchDir * ( ( this.centerWidth / 2 ) + ( this.tunnelWidth / 2 ) );
            r0 = l0 + Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * switchDir * ( this.centerWidth + this.tunnelWidth );

            c1 = c0 + switchDir * this.switchLenght;
            r1 = c1 + Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * switchDir * ( ( this.centerWidth / 2 ) + ( this.tunnelWidth / 2 ) ); 
            l1 = c1 + Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir * ( (this.centerWidth / 2 ) + ( this.tunnelWidth / 2 ) );

            cb = c0 + switchDir * ( this.switchLenght / 2 ) ;
            lb = l1 - switchDir * ( this.switchLenght / 2 ); 
            rb = r1 - switchDir * ( this.switchLenght / 2 ); 

            GameObject lightR1 = Instantiate( this.switchLight, r1 + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * startingDir.normalized * this.switchLightDistance ) - Vector3.forward * this.switchLightHeight, Quaternion.Euler( this.switchLightRotation.x + Vector3.SignedAngle( startingDir, Vector3.right, -Vector3.forward ) , this.switchLightRotation.y, this.switchLightRotation.z ) );
            lightR1.transform.parent = sectionGameObj.transform;
            lightR1.name = "Semaforo R1";
            GameObject lightL1 = Instantiate( this.switchLight, l1 + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * startingDir.normalized * this.switchLightDistance ) - Vector3.forward * this.switchLightHeight, Quaternion.Euler( this.switchLightRotation.x + Vector3.SignedAngle( startingDir, Vector3.right, -Vector3.forward ), this.switchLightRotation.y, this.switchLightRotation.z ) );
            lightL1.transform.parent = sectionGameObj.transform;
            lightL1.name = "Semaforo L1";
            GameObject lightL0 = Instantiate( this.switchLight, l0 + ( Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * startingDir.normalized * this.switchLightDistance ) - Vector3.forward * this.switchLightHeight, Quaternion.Euler( this.switchLightRotation.x + Vector3.SignedAngle( startingDir, Vector3.right, -Vector3.forward ) - 180.0f, this.switchLightRotation.y, this.switchLightRotation.z ) );
            lightL0.transform.parent = sectionGameObj.transform;
            lightL0.name = "Semaforo L0";
            switchLights.Add( SwitchDirection.RightToCenter, new List<GameObject>{ lightL0, lightR1 } );
            switchLights.Add( SwitchDirection.LeftToCenter, new List<GameObject>{ lightL0, lightL1 } );
            section.switchLights = switchLights;

            List<Vector3> rightCenterLine = BezierCurveCalculator.CalculateBezierCurve( new List<Vector3>{ l0, lb, rb, r1 }, this.curvePointsNumber );
            List<Vector3> leftCenterLine = new List<Vector3>{ l0, lb, l1 };

            switchFloor.rightCenterLine = rightCenterLine;
            switchFloor.centerLine = new List<Vector3>{ c0, cb, c1 };
            switchFloor.leftCenterLine = leftCenterLine;

            nextStartingPoints.Add( c1 );
            section.nextStartingPoints = nextStartingPoints;
        }
        else if( variant == 2 ) { // Allineamento sinistro
            r0 = startingPoint;
            c0 = r0 + Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir * ( ( this.centerWidth / 2 ) + ( this.tunnelWidth / 2 ) );
            l0 = r0 + Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir * ( this.centerWidth + this.tunnelWidth );

            c1 = c0 + switchDir * this.switchLenght;
            r1 = c1 + Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * switchDir * ( ( this.centerWidth / 2 ) + ( this.tunnelWidth / 2 ) ); 
            l1 = c1 + Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir * ( (this.centerWidth / 2 ) + ( this.tunnelWidth / 2 ) );

            cb = c0 + switchDir * ( this.switchLenght / 2 ) ;
            lb = l1 - switchDir * ( this.switchLenght / 2 ); 
            rb = r1 - switchDir * ( this.switchLenght / 2 ); 

            GameObject lightR1 = Instantiate( this.switchLight, r1 + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * startingDir.normalized * this.switchLightDistance ) - Vector3.forward * this.switchLightHeight, Quaternion.Euler( this.switchLightRotation.x + Vector3.SignedAngle( startingDir, Vector3.right, -Vector3.forward ) , this.switchLightRotation.y, this.switchLightRotation.z ) );
            lightR1.transform.parent = sectionGameObj.transform;
            lightR1.name = "Semaforo R1";
            GameObject lightL1 = Instantiate( this.switchLight, l1 + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * startingDir.normalized * this.switchLightDistance ) - Vector3.forward * this.switchLightHeight, Quaternion.Euler( this.switchLightRotation.x + Vector3.SignedAngle( startingDir, Vector3.right, -Vector3.forward ), this.switchLightRotation.y, this.switchLightRotation.z ) );
            lightL1.transform.parent = sectionGameObj.transform;
            lightL1.name = "Semaforo L1";
            GameObject lightR0 = Instantiate( this.switchLight, r0 + ( Quaternion.Euler( 0.0f, 0.0f, -90.0f ) * startingDir.normalized * this.switchLightDistance ) - Vector3.forward * this.switchLightHeight, Quaternion.Euler( this.switchLightRotation.x + Vector3.SignedAngle( startingDir, Vector3.right, -Vector3.forward ) - 180.0f, this.switchLightRotation.y, this.switchLightRotation.z ) );
            lightR0.transform.parent = sectionGameObj.transform;
            lightR0.name = "Semaforo R0";
            switchLights.Add( SwitchDirection.RightToCenter, new List<GameObject>{ lightR0, lightR1 } );
            switchLights.Add( SwitchDirection.LeftToCenter, new List<GameObject>{ lightR0, lightL1 } );
            section.switchLights = switchLights;

            List<Vector3> rightCenterLine = new List<Vector3>{ r0, rb, r1 };
            List<Vector3> leftCenterLine = BezierCurveCalculator.CalculateBezierCurve( new List<Vector3>{ r0, rb, lb, l1 }, this.curvePointsNumber );

            switchFloor.rightCenterLine = rightCenterLine;
            switchFloor.centerLine = new List<Vector3>{ c0, cb, c1 };
            switchFloor.leftCenterLine = leftCenterLine;

            nextStartingPoints.Add( c1 );
            section.nextStartingPoints = nextStartingPoints;
        }

        List<Vector3> nextStartingDirections = new List<Vector3>();
        nextStartingDirections.Add( startingDir );
        section.nextStartingDirections = nextStartingDirections;

        section.nextStartingDirections = nextStartingDirections; 
        section.nextStartingPoints =  nextStartingPoints;
        section.floorPoints = switchFloor;

        section.activeSwitch = SwitchDirection.RightToCenter;
        section.curvePointsCount = switchFloor.rightCenterLine.Count;
        section.floorPoints = switchFloor;

        return section;
    }
}
