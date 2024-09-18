using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MaintenanceGenerator : MonoBehaviour
{
    public class TunnelNode {

        public int id;
        public Vector3 pos;
        public List<TunnelNode> connectedNodes = new();
        public bool isStarting = false;
        public Dictionary<int, Dictionary<Side, Vector3>> intersectionsPoints = new();

        public bool ignoreMeshes = false;
    }

    public class TunnelEdge {
        public string id;
        public TunnelNode node;
        public TunnelNode connectedNode;
        public Dictionary<Side, List<Vector3>> groundVertex = new();
    }

    private HashSet<string> generatedEdgesIds = new();

    public List<Utils.Shape> tunnelWallShape;

    public Vector2 tunnelLenghtRange = new() { x = 10000.0f, y = 50000.0f };

    public float tunnelWidth = 20.0f;
    public float tunnelIntersectionExtension = 5.0f;

    public float tunnelPointsDistance = 500.0f;
    public float tunnelPointsStartingDistance = 150.0f;
    public float tunnelPointsDistanceMultiplier = 1.5f;
    public float turnMaxDistance = 100.0f;
    public Utils.Texturing tunnelGroundTexturing;
    private int seed = 0;
    public bool ready = false;

    public GameObject stationObj;
    public GameObject switchObj;

    public Dictionary<string, List<TunnelNode>> tunnelNodesMap = new();
    private List<TunnelNode> tunnelNodes = new();

    public float firstGenerationLoadingPercent = 0.65f;

    public IEnumerator GenerateMaintenance( GameManager gm ) {

        this.ready = false;

        MetroGenerator metroGenerator = gm.metroGenerator;

        this.seed = metroGenerator.seed;
        Random.InitState( this.seed ); 

        int previousBetaCoeff = 0, betaCoeff;

        for( int i = 0; i < metroGenerator.maintenanceTunnelsStart.Count; i++ ) {

            Vector3 firstPos = metroGenerator.maintenanceTunnelsStart[ i ].Item1;

            Vector3 startDir = metroGenerator.maintenanceTunnelsStart[ i ].Item2;
            Vector3 startPos = firstPos + startDir.normalized * this.tunnelPointsStartingDistance;

            if( metroGenerator.IsPointInsideProibitedArea( startPos, "" ) || CheckTunnelIntersections( firstPos, startPos ) ) {
                continue;
            }

            List<TunnelNode> currentTunnel = new();

            TunnelNode firstNode = new() { id = tunnelNodes.Count,
                                           pos = metroGenerator.maintenanceTunnelsStart[ i ].Item1, 
                                           isStarting = true, };
            tunnelNodes.Add( firstNode );
            currentTunnel.Add( firstNode );

            TunnelNode startNode = new() { id = tunnelNodes.Count,
                                           pos = startPos,
                                           isStarting = false };
            startNode.connectedNodes.Add( firstNode );
            firstNode.connectedNodes.Add( startNode );

            tunnelNodes.Add( startNode );
            currentTunnel.Add( startNode );

            float alpha = Vector3.SignedAngle( Vector3.right, startDir, -Vector3.forward );

            Vector3 mainDir = Vector3.right;
            if( alpha >= 45.0f && alpha < 135.0f ) {
                mainDir = -Vector3.up;
            }
            else if( alpha >= 135.0f || alpha < -135.0f ) {
                mainDir = -Vector3.right;
            }
            else if( alpha < -45.0f && alpha >= -135.0f ) {
                mainDir = Vector3.up;
            }

            float lenghtElapsed = 0.0f;
            Vector3 dir = new( mainDir.x, mainDir.y, mainDir.z );

            float lenght = Random.Range( tunnelLenghtRange.x, tunnelLenghtRange.y );
            for( int k = 0; k < ( int )( lenght / tunnelPointsDistance ); k++ ) {
                
                if( lenghtElapsed > turnMaxDistance ) {
                    betaCoeff = Random.Range( -1, 2 );
                    while( betaCoeff != 0 && betaCoeff == -previousBetaCoeff ) {
                        betaCoeff = Random.Range( -1, 2 );
                    }
                
                    previousBetaCoeff = betaCoeff;
                    
                    dir = Quaternion.AngleAxis( 90.0f * betaCoeff, -Vector3.forward ) * mainDir;
                    lenghtElapsed = 0.0f;
                }

                Vector3 nextPoint = tunnelNodes[ ^1 ].pos + tunnelPointsDistance * dir;
                TunnelNode newNode = new(), forcedNode = null;

                Dictionary<float, TunnelNode> forcedNodesByDistance = new();
                foreach( TunnelNode tunnelNode in this.tunnelNodes ) {

                    Vector3 nextDir = tunnelNode.pos - nextPoint;

                    if( !tunnelNode.isStarting && !currentTunnel.Contains( tunnelNode ) && nextDir.magnitude <= tunnelPointsDistance * this.tunnelPointsDistanceMultiplier && !metroGenerator.IsSegmentIntersectingProibitedArea( tunnelNodes[ ^1 ].pos, tunnelNode.pos, "" ) && !CheckTunnelIntersections( tunnelNodes[ ^1 ].pos, tunnelNode.pos ) ) {
                        float distance = ( tunnelNode.pos - tunnelNodes[ ^1 ].pos ).magnitude;
                        
                        if( !forcedNodesByDistance.ContainsKey( distance ) ) {
                            forcedNodesByDistance.Add( distance, tunnelNode );
                        }
                    }
                }

                if( forcedNodesByDistance.Count > 0 ) {
                    List<float> distances = forcedNodesByDistance.Keys.ToList();
                    distances.Sort();

                    forcedNode = forcedNodesByDistance[ distances[ 0 ] ];
                }

                if( forcedNode == null ) {
                    if( metroGenerator.IsSegmentIntersectingProibitedArea( tunnelNodes[ ^1 ].pos, nextPoint, "" ) ) {
                        break;
                    }
                    else if( CheckTunnelIntersections( tunnelNodes[ ^1 ].pos, nextPoint ) ) {
                        break;
                    }
                }

                lenghtElapsed += tunnelPointsDistance;

                if( forcedNode == null ) {

                    newNode.id = tunnelNodes.Count;
                    newNode.pos = nextPoint;
                    newNode.connectedNodes.Add( tunnelNodes[ ^1 ] );

                    tunnelNodes[ ^1 ].connectedNodes.Add( newNode );
                    tunnelNodes.Add( newNode );
                    currentTunnel.Add( newNode );
                }
                else{
                    forcedNode.connectedNodes.Add( tunnelNodes[ ^1 ] );
                    tunnelNodes[ ^1 ].connectedNodes.Add( forcedNode );
                    currentTunnel.Add( forcedNode );

                    break;
                }
            }

            tunnelNodesMap.Add( "M-" + i.ToString(), currentTunnel );

            gm.maintenanceGenerationPercent = this.firstGenerationLoadingPercent * ( i + 1 ) /  metroGenerator.maintenanceTunnelsStart.Count;
            yield return new WaitForEndOfFrame();
        }

        int nodesCounter = 0;
        foreach( TunnelNode node in tunnelNodes ) {
            CalculateTunnelIntersection( node );

            gm.maintenanceGenerationPercent = this.firstGenerationLoadingPercent + ( ( 1.0f - this.firstGenerationLoadingPercent ) * ( nodesCounter + 1 ) /  tunnelNodes.Count );
            nodesCounter++;
            yield return new WaitForEndOfFrame();
        }

        foreach( string key in tunnelNodesMap.Keys ) {
            foreach( TunnelNode node in tunnelNodesMap[ key ] ) {
                GenerateTunnelEdges( node );
                yield return new WaitForEndOfFrame();
            }
        }

        this.ready = true;
    }

        public bool CheckTunnelIntersections( Vector3 a, Vector3 b ) {
        for( int i = 0; i < tunnelNodes.Count; i++ ) {
            foreach( TunnelNode connectedNode in tunnelNodes[ i ].connectedNodes ) {
                
                Vector3 intersection = new();
                if( connectedNode.id > i ) {
                    bool isIntersecting = MeshGenerator.LineLineIntersect( out intersection, -Vector3.forward, a, b - a, tunnelNodes[ i ].pos, connectedNode.pos - tunnelNodes[ i ].pos, ArrayType.Segment, ArrayType.Segment );

                    if( isIntersecting ) {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void CalculateTunnelIntersection( TunnelNode node ) {
        Dictionary<int, Dictionary<Side, Vector3>> points = new();
        Dictionary<int, Vector3> dirs = new();
        Dictionary<float, TunnelNode> connectedNodes = new();
        Dictionary<int, TunnelNode> orderedConnectedNodes = new();
        if( node.connectedNodes.Count == 1 ) {
            TunnelNode connectedNode = node.connectedNodes[ 0 ];

            Dictionary<Side, Vector3> nodePoints = new();
            Vector3 dir = ( connectedNode.pos - node.pos ).normalized;
            dirs.Add( connectedNode.id, dir );
            dir *= tunnelWidth / 2;
            nodePoints.Add( Side.Right, node.pos + Quaternion.AngleAxis( -90.0f, -Vector3.forward ) * dir );
            nodePoints.Add( Side.Left, node.pos + Quaternion.AngleAxis( 90.0f, -Vector3.forward ) * dir );

            points.Add( connectedNode.id, nodePoints );

            node.ignoreMeshes = true;
        }
        else if( node.connectedNodes.Count > 1 ) {
            foreach( TunnelNode connectedNode in node.connectedNodes ) {
                Vector3 dir = connectedNode.pos - node.pos;
                connectedNodes.Add( Vector3.SignedAngle( Vector3.right, dir, -Vector3.forward ), connectedNode );
            }

            List<float> alphas = connectedNodes.Keys.ToList();
            alphas.Sort();

            if( node.connectedNodes.Count == 2 && Mathf.Approximately( 180.0f, Mathf.Abs( alphas[ 0 ] ) + Mathf.Abs( alphas[ 1 ] ) ) ) {
                node.ignoreMeshes = true;
            }

            foreach( float alpha in alphas ) {
                
                TunnelNode connectedNode = connectedNodes[ alpha ];

                Dictionary<Side, Vector3> nodePoints = new();
                Vector3 dir = ( connectedNode.pos - node.pos ).normalized;
                dirs.Add( connectedNode.id, dir );
                dir *= tunnelWidth / 2;
                nodePoints.Add( Side.Right, ( node.ignoreMeshes ? node.pos : ( node.pos + dir ) ) + Quaternion.AngleAxis( -90.0f, -Vector3.forward ) * dir );
                nodePoints.Add( Side.Left, ( node.ignoreMeshes ? node.pos : ( node.pos + dir ) ) + Quaternion.AngleAxis( 90.0f, -Vector3.forward ) * dir );

                points.Add( connectedNode.id, nodePoints );
                orderedConnectedNodes.Add( connectedNode.id, connectedNode );
            }

            List<int> keys = points.Keys.ToList();
            Dictionary<int, Dictionary<Side, Vector3>> intersections = new();
            for( int i = 0; i < points.Keys.Count; i++ ) {

                int key = keys[ i ];
                int previousKey = keys[ ^1 ];
                if( i > 0 ) {
                    previousKey = keys[ i - 1 ];
                }

                Vector3 d1 = orderedConnectedNodes[ key ].pos - node.pos;
                Vector3 d2 = orderedConnectedNodes[ previousKey ].pos - node.pos;
                Vector3 intersection = Vector3.zero;
                bool intersect = MeshGenerator.LineLineIntersect( out intersection, -Vector3.forward, points[ key ][ Side.Right ], d1.normalized, points[ previousKey ][ Side.Left ], d2.normalized, ArrayType.Ray, ArrayType.Ray );

                if( intersect ) {
                    // Debug.DrawRay( intersection, Vector3.right, Color.yellow, 999 );
                    // Debug.DrawRay( intersection, Vector3.up, Color.yellow, 999 );

                    if( intersections.ContainsKey( key ) ) {
                        Dictionary<Side, Vector3> temp = intersections[ key ];
                        temp.Add( Side.Right, intersection );
                        intersections[ key ] = temp;
                    }
                    else {
                        Dictionary<Side, Vector3> intersectionKey = new() { { Side.Right, intersection }, };
                        intersections.Add( key, intersectionKey );
                    }

                    if( intersections.ContainsKey( previousKey ) ) {
                        Dictionary<Side, Vector3> temp = intersections[ previousKey ];
                        temp.Add( Side.Left, intersection );
                        intersections[ previousKey ] = temp;
                    }
                    else {
                        Dictionary<Side, Vector3> intersectionPreviousKey = new() { { Side.Left, intersection }, };
                        intersections.Add( previousKey, intersectionPreviousKey );
                    }
                }
            }

            foreach( int key in intersections.Keys ) {
                if( points.ContainsKey( key ) ) {

                    if( intersections[ key ].ContainsKey( Side.Right ) ) {
                        points[ key ][ Side.Right ] = intersections[ key ][ Side.Right ];
                    }
                    if( intersections[ key ].ContainsKey( Side.Left ) ) {
                        points[ key ][ Side.Left ] = intersections[ key ][ Side.Left ];
                    }
                }
            }

            List<Vector3> groundPoints = new();
            for( int i = 0; i < points.Keys.Count; i++ ) {

                int key = keys[ i ];
                int previousKey = keys[ ^1 ];
                if( i > 0 ) {
                    previousKey = keys[ i - 1 ];
                }
                
                if( !groundPoints.Contains( points[ key ][ Side.Right ] ) ) {
                    groundPoints.Add( points[ key ][ Side.Right ] );
                }

                if( !groundPoints.Contains( points[ key ][ Side.Left ] ) ) {
                    groundPoints.Add( points[ key ][ Side.Left ] );
                }

                List<Vector3> wallPoints = new() { points[ key ][ Side.Right ] + dirs[ key ].normalized * this.tunnelIntersectionExtension,
                                                   points[ key ][ Side.Right ],
                                                   points[ previousKey ][ Side.Left ],
                                                   points[ previousKey ][ Side.Left ] + dirs[ previousKey ].normalized * this.tunnelIntersectionExtension };

                if( wallPoints[ 1 ] == wallPoints[ 2 ] ) {
                    wallPoints.RemoveAt( 1 ); 
                }

                if( !node.ignoreMeshes ) {
                    InstantiateComposedExtrudedMesh( this.transform, "Maanutenzione - Muro", wallPoints, this.tunnelWallShape, null, 180.0f, false, false, Vector2.zero );
                }

                // for( int k = 1; k < wallPoints.Count; k++ ) {
                //     Debug.DrawLine( wallPoints[ k ], wallPoints[ k - 1 ], Color.cyan, 999 );
                // }
                
            }

            if( !node.ignoreMeshes ) {
                MetroGenerator.InstantiatePoligon( this.transform, "Manutenzione - Pavimento Nodo " + node.id, groundPoints, true, -Vector3.forward, Vector3.right, Vector2.zero, this.tunnelGroundTexturing, true );
            }
        }

        node.intersectionsPoints = points;
    }

    private void GenerateTunnelEdges( TunnelNode node ) {

        if( node.intersectionsPoints.Count > 0 ) {
            foreach( TunnelNode connectedNode in node.connectedNodes ) {

                if( !node.intersectionsPoints.ContainsKey( connectedNode.id ) || !connectedNode.intersectionsPoints.ContainsKey( node.id ) ) {
                    continue;
                }

                if( !generatedEdgesIds.Contains( node.id.ToString() + "-" + connectedNode.id.ToString() ) && !generatedEdgesIds.Contains( connectedNode.id.ToString() + "-" + node.id.ToString() ) ) {

                    TunnelEdge edge = new() { id = node.id.ToString() + "-" + connectedNode.id.ToString(),
                                              node = node,
                                              connectedNode = connectedNode };

                    Dictionary<Side, Vector3> intersectionsPointsStart = node.intersectionsPoints[ connectedNode.id ];
                    Dictionary<Side, Vector3> intersectionsPointsEnd = connectedNode.intersectionsPoints[ node.id ];

                    edge.groundVertex.Add( Side.Right, new List<Vector3>{ intersectionsPointsStart[ Side.Right ], intersectionsPointsEnd[ Side.Left ]  } );
                    edge.groundVertex.Add( Side.Left, new List<Vector3>{ intersectionsPointsStart[ Side.Left], intersectionsPointsEnd[ Side.Right ]  } );

                    Debug.DrawLine( edge.groundVertex[ Side.Left ][ 0 ], edge.groundVertex[ Side.Left ][ 1 ], Color.yellow, 999 );
                    Debug.DrawLine( edge.groundVertex[ Side.Right ][ 0 ], edge.groundVertex[ Side.Right ][ 1 ], Color.magenta, 999 );

                    generatedEdgesIds.Add( edge.id );

                    List<Vector3> orderedGroundVertex = new();
                    orderedGroundVertex.AddRange( edge.groundVertex[ Side.Left ] );
                    orderedGroundVertex.Reverse();
                    orderedGroundVertex.AddRange( edge.groundVertex[ Side.Right ] );

                    MetroGenerator.InstantiatePoligon( this.transform, "Manutenzione - Pavimento Tunnel " + node.id, orderedGroundVertex, true, -Vector3.forward, Vector3.right, Vector2.zero, this.tunnelGroundTexturing, true );
                }
            }
        }

    }

    private List<MeshGenerator.ProceduralMesh> InstantiateComposedExtrudedMesh( Transform parent, string gameObjName, List<Vector3> baseLine, List<Utils.Shape> partialShapes, List<int> partialShapesIndexes, float profileRotationCorrection, bool clockwiseRotation, bool closeMesh, Vector3 uvOffset ) {

        List<MeshGenerator.ProceduralMesh> newMeshes = new();
        MeshGenerator.ProceduralMesh extrudedMesh = new();

        for( int i = 0; i < partialShapes.Count; i++ ) {

            if( i > 0 ) {
                int verticesStructuresLastIndex = newMeshes[ i - 1 ].verticesStructure[ Orientation.Horizontal ].Count - 1;
                baseLine = newMeshes[ i - 1 ].verticesStructure[ Orientation.Horizontal ][ verticesStructuresLastIndex ];

                if( closeMesh ) {
                    baseLine.RemoveAt( baseLine.Count - 1 );
                }

                if( partialShapes[ i ].texturing.materials == partialShapes[ i - 1 ].texturing.materials ) {
                    uvOffset = extrudedMesh.uvMax;
                }
            }

            extrudedMesh = MeshGenerator.GenerateExtrudedMesh( partialShapes[ i ].partialProfile, partialShapes[ i ].scale, null, baseLine, partialShapes[ i ].positionCorrection.x, partialShapes[ i ].positionCorrection.y, clockwiseRotation, closeMesh, partialShapes[ i ].texturing.tiling, uvOffset, profileRotationCorrection, partialShapes[ i ].smoothFactor );
            extrudedMesh.mesh.name = "Procedural Extruded Mesh";

            if( ( partialShapesIndexes != null && partialShapesIndexes.Contains( i ) ) || partialShapesIndexes == null ) {

                GameObject extrudedGameObj = new( gameObjName + " - " + partialShapes[ i ].name );
                extrudedGameObj.transform.parent = parent;
                extrudedGameObj.transform.position = Vector3.zero;
                extrudedGameObj.AddComponent<MeshFilter>();
                extrudedGameObj.AddComponent<MeshRenderer>();
                extrudedGameObj.GetComponent<MeshFilter>().sharedMesh = extrudedMesh.mesh;
                extrudedGameObj.GetComponent<MeshRenderer>().SetMaterials( partialShapes[ i ].texturing.materials );

                extrudedMesh.gameObj = extrudedGameObj;
            }

            newMeshes.Add( extrudedMesh );
        }

        return newMeshes;
    }

    private void OnDrawGizmos() {
        if( tunnelNodes.Count > 0 ) {
            for( int i = 0; i < tunnelNodes.Count; i++ ) {
                foreach( TunnelNode connectedNode in tunnelNodes[ i ].connectedNodes ) {
                    Debug.DrawLine( tunnelNodes[ i ].pos, connectedNode.pos, Color.blue, Time.deltaTime );
                }
            }
        }
    }
}
