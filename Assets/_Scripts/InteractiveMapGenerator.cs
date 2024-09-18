using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveMapGenerator : MonoBehaviour
{

    public float lineWidth = 50.0f;
    public float maintenanceWidth = 25.0f;
    public Vector3 mapTranslation = new(){ x = 0.0f, y = 0.0f, z = -10.0f };

    public int lineDetailReductionFactor = 25;

    public Utils.Shape tunnelShape;

    public Utils.Texturing lineTexturing;
    public Material maintenanceMaterial;
    public List<LineRenderer> lineObjs = new();
    public List<LineRenderer> maintenanceObjs = new();
    public Vector4 mapBoundaries = new( 0.0f, 0.0f, 0.0f, 0.0f ); //x = xMin, y = xMax, z = yMin, w = yMax

    public bool ready = false;

    public void GenerateMap( GameManager gm ) {

        MetroGenerator metroGenerator = gm.metroGenerator;
        MaintenanceGenerator maintenanceGenerator = gm.maintenanceGenerator;        

        foreach( string lineName in metroGenerator.lines.Keys ) {  
            
            List<Vector3> linePoints = new();

            foreach( LineSection section in metroGenerator.lines[ lineName ] ) {
                linePoints.AddRange( section.bezierCurveLimitedAngle );
            }

            linePoints = BezierCurveCalculator.RecalcultateCurveWithFixedLenght( linePoints, linePoints.Count );

            List<Vector3> linePointsReduced = new();
            for( int i = 0; i < linePoints.Count; i += this.lineDetailReductionFactor ) {
                linePointsReduced.Add( linePoints[ i ] );
            }

            if( linePoints.Count % this.lineDetailReductionFactor != 0 ) {
                linePointsReduced.Add( linePoints[ ^1 ] );
            }

            GameObject mapLineObj = new( "Mappa " + lineName );
            mapLineObj.transform.parent = this.transform;
            mapLineObj.AddComponent<LineRenderer>();
            mapLineObj.layer = LayerMask.NameToLayer( "Map" );

            LineRenderer line = mapLineObj.GetComponent<LineRenderer>();
            line.positionCount = linePointsReduced.Count;
            line.SetPositions( linePointsReduced.ToArray() );
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.useWorldSpace = false;
            line.numCapVertices = 5;
            line.SetMaterials( this.lineTexturing.materials);
            line.alignment = LineAlignment.TransformZ;
            line.textureMode = LineTextureMode.RepeatPerSegment;

            Vector4 dotTiling = line.material.GetVector( "_DotTiling" );
            dotTiling.x *= this.lineDetailReductionFactor;
            line.material.SetVector( "_DotTiling", dotTiling );

            lineObjs.Add( line );

            mapLineObj.transform.Translate( mapTranslation );

            foreach( Vector3 point in linePoints ) {
                this.mapBoundaries = new Vector4( point.x < this.mapBoundaries.x ? point.x : this.mapBoundaries.x,
                                                  point.x > this.mapBoundaries.y ? point.x : this.mapBoundaries.y,
                                                  point.y < this.mapBoundaries.z ? point.y : this.mapBoundaries.z,
                                                  point.y > this.mapBoundaries.w ? point.y : this.mapBoundaries.w );
            }
        }

        foreach( string maintenanceName in maintenanceGenerator.tunnelNodesMap.Keys ) {

            List<Vector3> maintenancePoints = new();

            foreach( MaintenanceGenerator.TunnelNode node in maintenanceGenerator.tunnelNodesMap[ maintenanceName ] ) {
                maintenancePoints.Add( node.pos );
            }

            GameObject mapMaintenanceObj = new( "Mappa " + maintenanceName );
            mapMaintenanceObj.transform.parent = this.transform;
            mapMaintenanceObj.AddComponent<LineRenderer>();
            mapMaintenanceObj.layer = LayerMask.NameToLayer( "Map" );

            LineRenderer maintenance = mapMaintenanceObj.GetComponent<LineRenderer>();
            maintenance.positionCount = maintenancePoints.Count;
            maintenance.SetPositions( maintenancePoints.ToArray() );
            maintenance.startWidth = maintenanceWidth;
            maintenance.endWidth = maintenanceWidth;
            maintenance.useWorldSpace = false;
            maintenance.numCornerVertices = 5;
            maintenance.numCapVertices = 5;
            maintenance.material = maintenanceMaterial;
            maintenance.alignment = LineAlignment.TransformZ;
            maintenance.textureMode = LineTextureMode.Stretch;

            maintenanceObjs.Add( maintenance );

            mapMaintenanceObj.transform.Translate( mapTranslation );

            foreach( Vector3 point in maintenancePoints ) {
                this.mapBoundaries = new Vector4( point.x < this.mapBoundaries.x ? point.x : this.mapBoundaries.x,
                                                  point.x > this.mapBoundaries.y ? point.x : this.mapBoundaries.y,
                                                  point.y < this.mapBoundaries.z ? point.y : this.mapBoundaries.z,
                                                  point.y > this.mapBoundaries.w ? point.y : this.mapBoundaries.w );
            }
        }

        this.ready = true;
    }
}
