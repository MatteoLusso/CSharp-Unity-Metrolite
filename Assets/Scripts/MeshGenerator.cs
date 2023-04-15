using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static Vector3[,] ConvertListsToMatrix_2xM( List<Vector3> up, List<Vector3> down )
    {
        if( up.Count == down.Count )
        {
            Vector3[ , ] VertexMatrix = new Vector3[ up.Count, down.Count ];

            for( int row = 0; row < 2; row++ )
            {
               for( int col = 0; col < up.Count; col++ )
               {
                    if( row == 0 )
                    {
                        VertexMatrix[ row, col ] = up[ col ];
                    }
                    else
                    {
                        VertexMatrix[ row, col ] = down[ col ];
                    }
               } 
            }

            return VertexMatrix;

        }
        else
        {
            return null;
        }
    }

    public static List<List<Vector3>> CalculateFloorMeshVertex( List<Vector3> curve, List<Vector3> controlPoints, float floorWidth, bool floorParabolic )
    {

        List<Vector3> rightPoints = new List<Vector3>();
        List<Vector3> leftPoints = new List<Vector3>();

        float zHeightPrev = 0.0f;
        float zHeightNext = 0.0f;

        if( floorParabolic )
        {
            zHeightPrev = curve[ 0 ].z;
            zHeightNext = curve[ 1 ].z;
        }

        //Vector3 dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3(curve[ 0 ].x, curve[ 0 ].y, zHeightPrev) - new Vector3(curve[ 1 ].x, curve[ 1 ].y, zHeightNext ) ).normalized * tunnelWidth;
        Vector3 dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ 0 ].x, curve[ 0 ].y, zHeightPrev ) - new Vector3( controlPoints[ 1 ].x, controlPoints[ 1 ].y, zHeightNext ) ).normalized * floorWidth;

        leftPoints.Add( curve[ 0 ] + dir );
        rightPoints.Add(curve[ 0 ] - dir );

        for( int i = 1; i < curve.Count - 1; i++ )
        {
            if( floorParabolic )
            {
                zHeightPrev = curve[ i - 1 ].z;
                zHeightNext = curve[ i + 1 ].z;
            }

            dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ i - 1 ].x, curve[ i - 1 ].y, zHeightPrev ) - new Vector3( curve[ i + 1 ].x, curve[ i + 1 ].y, zHeightNext ) ).normalized * floorWidth;

            leftPoints.Add( curve[ i ] + dir );
            rightPoints.Add(curve[ i ] - dir );
        }

        if( floorParabolic )
        {
            zHeightPrev = curve[ curve.Count - 2 ].z;
            zHeightNext = curve[ curve.Count - 1 ].z;
        }

        dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ curve.Count - 2 ].x, curve[ curve.Count - 2 ].y, zHeightPrev ) - new Vector3( curve[ curve.Count - 1 ].x, curve[ curve.Count - 1 ].y, zHeightNext ) ).normalized * floorWidth;
        leftPoints.Add( curve[ curve.Count - 1 ] + dir );
        rightPoints.Add( curve[ curve.Count - 1 ] - dir );

        List<List<Vector3>> rightAndLeftPoints = new List<List<Vector3>>();
        rightAndLeftPoints.Add( rightPoints ); // Indice 0 >>> destra
        rightAndLeftPoints.Add( leftPoints ); // Indice 1 >>> sinistra

        return rightAndLeftPoints;
    }

    public static Mesh GenerateFloorMesh( List<Vector3> curve, Vector3[ , ] vertMatrix, float textureHorLenght )
    {
        //float curveLenght = BezierCurveCalculator.CalculateCurveLenght( curve );
        //float deltaLCurve = curveLenght / curve.Count;

        Mesh floorMesh = new Mesh();

        Vector3[] vertices = new Vector3[ curve.Count * 2 ];
        int[] edges = new int[( curve.Count - 1) * 6 ];
        Vector2[] uvs = new Vector2[ vertices.Length ];
        float meshPercent = 0.0f;
        for( int i = 0; i < curve.Count; i++ )
        {
            vertices[ i * 2 ] = vertMatrix[ 0, i ];
            vertices[ ( i * 2 ) + 1 ] = vertMatrix[ 1, i ];

            // deltaLText rappresenta la distanza della curva che una singola ripetizione di texture deve coprire,
            // considerando la lunghezza dell'intera curva e dividendola per il numero di punti ho la 
            // distanza (costante) fra ogni punto deltaLCurve. In questo modo, dividendo deltaLCurve per deltaLText
            // ottengo quante volte la texture deve ripetersi per ogni segmento e moltiplico per i in modo da mappare la texture
            // sulla curva indipendentemente dalla lunghezza della stessa.
            
            if( i > 0 ) {
                meshPercent += ( float )( ( curve[ i ] - curve[ i - 1 ]).magnitude / textureHorLenght );
            }
            //float meshPercent = i * ( float )( deltaLCurve / deltaLText );
            uvs[ ( i * 2 ) ] = new Vector2( meshPercent, 0 );
            uvs[ ( i * 2 ) + 1 ] = new Vector2( meshPercent, 1 );

            if( i < curve.Count - 1 )
            {
                edges[ ( i * 6 ) + 0 ] = ( i * 2 );
                edges[ ( i * 6 ) + 1 ] = edges[ ( i * 6 ) + 4 ] = ( i * 2 ) + 2;
                edges[ ( i * 6 ) + 2 ] = edges[ ( i * 6 ) + 3 ] = ( i * 2 ) + 1;
                edges[ ( i * 6 ) + 5 ] = ( i * 2 ) + 3;
            }
        }

        floorMesh.Clear();
        floorMesh.vertices = vertices;
        floorMesh.triangles = edges;
        floorMesh.uv = uvs;

        floorMesh.RecalculateNormals();

        return floorMesh;
    }
}
