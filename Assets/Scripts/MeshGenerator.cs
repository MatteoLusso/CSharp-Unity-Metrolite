using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public class Floor {

        public List<Vector3> rightR { get; set; }
        public List<Vector3> rightLine { get; set; }
        public List<Vector3> rightL { get; set; }

        public List<Vector3> centerR { get; set; }
        public List<Vector3> centerLine { get; set; }
        public List<Vector3> centerL { get; set; }

        public List<Vector3> leftR { get; set; }
        public List<Vector3> leftLine { get; set; }
        public List<Vector3> leftL { get; set; }

        public List<Vector3> leftRightLine { get; set; }
        public List<Vector3> rightLeftLine { get; set; }

        public List<Vector3> rightCenterL { get; set; }
        public List<Vector3> rightCenterLine { get; set; }
        public List<Vector3> rightCenterR { get; set; }

        public List<Vector3> leftCenterL { get; set; }
        public List<Vector3> leftCenterLine { get; set; }
        public List<Vector3> leftCenterR { get; set; }

        public List<Vector3> leftCenterNewLine { get; set; }
        public List<Vector3> rightCenterNewLine { get; set; }

        public List<Vector3> centerEntranceRight { get; set; }
        public List<Vector3> centerExitRight { get; set; }
        public List<Vector3> centerEntranceLeft { get; set; }
        public List<Vector3> centerExitLeft { get; set; }

        public List<Vector3> rightEntranceRight { get; set; }
        public List<Vector3> rightExitRight { get; set; }
        public List<Vector3> leftEntranceLeft { get; set; }
        public List<Vector3> leftExitLeft { get; set; }
    }

    public class PlatformSide {

        public List<Vector3> rightDown { get; set; }
        public List<Vector3> rightUp { get; set; }

        public List<Vector3> leftDown { get; set; }
        public List<Vector3> leftUp { get; set; }

        public List<Vector3> rightFloorLeft { get; set; }
        public List<Vector3> rightFloorRight { get; set; }

        public List<Vector3> leftFloorLeft { get; set; }
        public List<Vector3> leftFloorRight { get; set; }

    }

    public static Vector3[ , ] ConvertListsToMatrix_2xM( List<Vector3> up, List<Vector3> down )
    {
        if( up.Count == down.Count )
        {
            Vector3[ , ] vertexMatrix = new Vector3[ up.Count, down.Count ];

            for( int row = 0; row < 2; row++ )
            {
               for( int col = 0; col < up.Count; col++ )
               {
                    if( row == 0 )
                    {
                        vertexMatrix[ row, col ] = up[ col ];
                    }
                    else
                    {
                        vertexMatrix[ row, col ] = down[ col ];
                    }
               } 
            }

            return vertexMatrix;

        }
        else
        {
            return null;
        }
    }

    public static Floor CalculateBidirectionalWithBothSidesPlatformFloorMeshVertex( LineSection section, float centerWidth, float sideWidth/*, bool floorParabolic,*/ )
    {
        List<Vector3> curve = section.bezierCurveLimitedAngle;
        List<Vector3> controlPoints = section.controlsPoints;
        
        List<Vector3> leftR = new List<Vector3>();
        List<Vector3> leftLine = new List<Vector3>();
        List<Vector3> leftL = new List<Vector3>();

        List<Vector3> centerR = new List<Vector3>();
        List<Vector3> centerL = new List<Vector3>();

        List<Vector3> rightR = new List<Vector3>();
        List<Vector3> rightLine = new List<Vector3>();
        List<Vector3> rightL = new List<Vector3>();

        /*float zHeightPrev = 0.0f;
        float zHeightNext = 0.0f;

        if( floorParabolic )
        {
            zHeightPrev = curve[ 0 ].z;
            zHeightNext = curve[ 1 ].z;
        }*/

        Vector3 dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ 0 ].x, curve[ 0 ].y, 0.0f/*zHeightPrev*/ ) - new Vector3( controlPoints[ 1 ].x, controlPoints[ 1 ].y, 0.0f/*zHeightNext*/ ) ).normalized;

        leftL.Add( curve[ 0 ] - ( dir * ( ( centerWidth / 2 ) + sideWidth ) ) );
        leftLine.Add( curve[ 0 ] - ( dir * ( ( centerWidth / 2 ) + ( sideWidth / 2 ) ) ) );
        
        centerL.Add( curve[ 0 ] - ( dir * ( centerWidth / 2 ) ) );
        centerR.Add( curve[ 0 ] + ( dir * ( centerWidth / 2 ) ) );

        rightLine.Add( curve[ 0 ] + ( dir * ( ( centerWidth / 2 ) + ( sideWidth / 2 ) ) ) );
        rightR.Add( curve[ 0 ] + ( dir * ( ( centerWidth / 2 ) + sideWidth ) ) );

        for( int i = 1; i < curve.Count - 1; i++ )
        {
            /*if( floorParabolic )
            {
                zHeightPrev = curve[ i - 1 ].z;
                zHeightNext = curve[ i + 1 ].z;
            }*/

            dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ i - 1 ].x, curve[ i - 1 ].y, 0.0f/*zHeightPrev*/ ) - new Vector3( curve[ i + 1 ].x, curve[ i + 1 ].y, 0.0f/*zHeightNext*/ ) ).normalized;

            leftL.Add( curve[ i ] - ( dir * ( ( centerWidth / 2 ) + sideWidth ) ) );
            leftLine.Add( curve[ i ] - ( dir * ( ( centerWidth / 2 ) + ( sideWidth / 2 ) ) ) );
            
            centerL.Add( curve[ i ] - ( dir * ( centerWidth / 2 ) ) );
            centerR.Add( curve[ i ] + ( dir * ( centerWidth / 2 ) ) );

            rightLine.Add( curve[ i ] + ( dir * ( ( centerWidth / 2 ) + ( sideWidth / 2 ) ) ) );
            rightR.Add( curve[ i ] + ( dir * ( ( centerWidth / 2 ) + sideWidth ) ) );
        }

        /*if( floorParabolic )
        {
            zHeightPrev = curve[ curve.Count - 2 ].z;
            zHeightNext = curve[ curve.Count - 1 ].z;
        }*/

        dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ curve.Count - 2 ].x, curve[ curve.Count - 2 ].y, 0.0f/*zHeightPrev*/ ) - new Vector3( curve[ curve.Count - 1 ].x, curve[ curve.Count - 1 ].y, 0.0f/*zHeightNext*/ ) ).normalized;
        
        leftL.Add( curve[ curve.Count - 1 ] - ( dir * ( ( centerWidth / 2 ) + sideWidth ) ) );
        leftLine.Add( curve[ curve.Count - 1 ] - ( dir * ( ( centerWidth / 2 ) + ( sideWidth / 2 ) ) ) );
        
        centerL.Add( curve[ curve.Count - 1 ] - ( dir * ( centerWidth / 2 ) ) );
        centerR.Add( curve[ curve.Count - 1 ] + ( dir * ( centerWidth / 2 ) ) );

        rightLine.Add( curve[ curve.Count - 1 ] + ( dir * ( ( centerWidth / 2 ) + ( sideWidth / 2 ) ) ) );
        rightR.Add( curve[ curve.Count - 1 ] + ( dir * ( ( centerWidth / 2 ) + sideWidth ) ) );

        Floor singleFloor = new Floor();
        singleFloor.leftL = leftL;
        singleFloor.leftLine = leftLine;
        singleFloor.leftR = centerL;

        singleFloor.centerL = centerL;
        singleFloor.centerLine = curve;
        singleFloor.centerR = centerR;

        singleFloor.rightL = centerR;
        singleFloor.rightLine = rightLine;
        singleFloor.rightR = rightR;

        return singleFloor;
    }

    public static Floor CalculateBidirectionalWithCentralPlatformFloorMeshVertex( LineSection section, float centerWidth, float sideWidth, float stationLenght, float stationExtensionLenght, float stationExtensionHeight, int stationExtensionCurvePoints )
    {
        List<Vector3> curve = section.bezierCurveLimitedAngle;
        
        List<Vector3> leftR = new List<Vector3>();
        List<Vector3> leftLine = new List<Vector3>();
        List<Vector3> leftL = new List<Vector3>();

        List<Vector3> centerR = new List<Vector3>();
        List<Vector3> centerL = new List<Vector3>();

        List<Vector3> rightR = new List<Vector3>();
        List<Vector3> rightLine = new List<Vector3>();
        List<Vector3> rightL = new List<Vector3>();


        Vector3 dir = ( new Vector3( curve[ 1 ].x, curve[ 1 ].y, 0.0f ) - new Vector3( curve[ 0 ].x, curve[ 0 ].y, 0.0f ) ).normalized;
        Vector3 orthogonalDir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * dir;

        // Calcolo punti controllo
        Vector3 lb0, lb1, lb2, lb3, rb0, rb1, rb2, rb3;

        rb0 = curve[ 0 ] - ( orthogonalDir  * ( ( centerWidth / 2 ) + ( sideWidth / 2 ) ) );
        rb1 = rb0 + ( dir * ( stationExtensionLenght / 2 ) );
        rb2 = rb1 - ( orthogonalDir * ( stationExtensionHeight / 2 ) );
        rb3 = rb2 + ( dir * ( stationExtensionLenght / 2 ) );

        lb0 = curve[ 0 ] + ( orthogonalDir  * ( ( centerWidth / 2 ) + ( sideWidth / 2 ) ) );
        lb1 = lb0 + ( dir * ( stationExtensionLenght / 2 ) );
        lb2 = lb1 + ( orthogonalDir * ( stationExtensionHeight / 2 ) );
        lb3 = lb2 + ( dir * ( stationExtensionLenght / 2 ) );

        Debug.DrawLine( rb0, rb1, Color.red, Mathf.Infinity );
        Debug.DrawLine( rb1, rb2, Color.red, Mathf.Infinity );
        Debug.DrawLine( rb2, rb3, Color.red, Mathf.Infinity );

        Debug.DrawLine( lb0, lb1, Color.red, Mathf.Infinity );
        Debug.DrawLine( lb1, lb2, Color.red, Mathf.Infinity );
        Debug.DrawLine( lb2, lb3, Color.red, Mathf.Infinity );

        leftL.Add( lb0 + ( orthogonalDir * ( sideWidth / 2 ) ) );
        leftLine.AddRange( BezierCurveCalculator.CalculateBezierCurve( new List<Vector3>{ lb0, lb1, lb2, lb3 }, stationExtensionCurvePoints ) );
        leftR.Add( lb0 - ( orthogonalDir * ( sideWidth / 2 ) ) );
        
        rightL.Add( rb0 + ( orthogonalDir * ( sideWidth / 2 ) ) );
        rightLine.AddRange( BezierCurveCalculator.CalculateBezierCurve( new List<Vector3>{ rb0, rb1, rb2, rb3 }, stationExtensionCurvePoints ) );
        rightR.Add( rb0 - ( orthogonalDir * ( sideWidth / 2 ) ) );

        for( int i = 1; i < stationExtensionCurvePoints - 1; i++ ) {

            Vector3 orthogonalDirLeft = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( leftLine[ i - 1 ].x, leftLine[ i - 1 ].y, 0.0f ) - new Vector3( leftLine[ i + 1 ].x, leftLine[ i + 1 ].y, 0.0f ) ).normalized;
            leftL.Add( leftLine[ i ] - ( orthogonalDirLeft * ( sideWidth / 2 ) ) );
            leftR.Add( leftLine[ i ] + ( orthogonalDirLeft * ( sideWidth / 2 ) ) );

            Vector3 orthogonalDirRight = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( rightLine[ i - 1 ].x, rightLine[ i - 1 ].y, 0.0f ) - new Vector3( rightLine[ i + 1 ].x, rightLine[ i + 1 ].y, 0.0f ) ).normalized;
            rightL.Add( rightLine[ i ] - ( orthogonalDirRight * ( sideWidth / 2 ) ) );
            rightR.Add( rightLine[ i ] + ( orthogonalDirRight * ( sideWidth / 2 ) ) );
        }

        leftL.Add( leftLine[ ^1 ] + ( orthogonalDir * ( sideWidth / 2 ) ) );
        leftR.Add( leftLine[ ^1 ] - ( orthogonalDir * ( sideWidth / 2 ) ) );
        
        rightL.Add( rightLine[ ^1 ] + ( orthogonalDir * ( sideWidth / 2 ) ) );
        rightR.Add( rightLine[ ^1 ] - ( orthogonalDir * ( sideWidth / 2 ) ) );

        // Calcolo punti controllo finali
        Vector3 lb0Inv, lb1Inv, lb2Inv, lb3Inv, rb0Inv, rb1Inv, rb2Inv, rb3Inv;

        lb0Inv = leftLine[ ^1 ] + ( dir * stationLenght );
        rb0Inv = rightLine[ ^1 ] + ( dir * stationLenght );

        lb1Inv = lb0Inv  + ( dir * ( stationExtensionLenght / 2 ) );
        lb2Inv = lb1Inv - ( orthogonalDir * ( stationExtensionHeight / 2 ) );
        lb3Inv = lb2Inv + ( dir * ( stationExtensionLenght / 2 ) );

        rb1Inv = rb0Inv + ( dir * ( stationExtensionLenght / 2 ) );
        rb2Inv = rb1Inv + ( orthogonalDir * ( stationExtensionHeight / 2 ) );
        rb3Inv = rb2Inv + ( dir * ( stationExtensionLenght / 2 ) );

        Debug.DrawLine( rb0Inv, rb1Inv, Color.red, Mathf.Infinity );
        Debug.DrawLine( rb1Inv, rb2Inv, Color.red, Mathf.Infinity );
        Debug.DrawLine( rb2Inv, rb3Inv, Color.red, Mathf.Infinity );

        Debug.DrawLine( lb0Inv, lb1Inv, Color.red, Mathf.Infinity );
        Debug.DrawLine( lb1Inv, lb2Inv, Color.red, Mathf.Infinity );
        Debug.DrawLine( lb2Inv, lb3Inv, Color.red, Mathf.Infinity );

        leftLine.AddRange( BezierCurveCalculator.CalculateBezierCurve( new List<Vector3>{ lb0Inv, lb1Inv, lb2Inv, lb3Inv }, stationExtensionCurvePoints ) );
        rightLine.AddRange( BezierCurveCalculator.CalculateBezierCurve( new List<Vector3>{ rb0Inv, rb1Inv, rb2Inv, rb3Inv }, stationExtensionCurvePoints ) );

        for( int i = stationExtensionCurvePoints; i < ( 2 * stationExtensionCurvePoints ) - 1; i++ ) {
            Vector3 orthogonalDirLeft = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( leftLine[ i - 1 ].x, leftLine[ i - 1 ].y, 0.0f ) - new Vector3( leftLine[ i + 1 ].x, leftLine[ i + 1 ].y, 0.0f ) ).normalized;
            leftL.Add( leftLine[ i ] - ( orthogonalDirLeft * ( sideWidth / 2 ) ) );
            leftR.Add( leftLine[ i ] + ( orthogonalDirLeft * ( sideWidth / 2 ) ) );

            Vector3 orthogonalDirRight = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( rightLine[ i - 1 ].x, rightLine[ i - 1 ].y, 0.0f ) - new Vector3( rightLine[ i + 1 ].x, rightLine[ i + 1 ].y, 0.0f ) ).normalized;
            rightL.Add( rightLine[ i ] - ( orthogonalDirRight * ( sideWidth / 2 ) ) );
            rightR.Add( rightLine[ i ] + ( orthogonalDirRight * ( sideWidth / 2 ) ) );
        }

        leftL.Add( leftLine[ ^1 ] + ( orthogonalDir * ( sideWidth / 2 ) ) );
        leftR.Add( leftLine[ ^1 ] - ( orthogonalDir * ( sideWidth / 2 ) ) );
        
        rightL.Add( rightLine[ ^1 ] + ( orthogonalDir * ( sideWidth / 2 ) ) );
        rightR.Add( rightLine[ ^1 ] - ( orthogonalDir * ( sideWidth / 2 ) ) );

        /*for( int j = 0; j < ( 2 * stationExtensionCurvePoints ); j++ ) {
            Debug.DrawLine( rightL[ j - 1 ], rightL[ j ], Color.cyan, Mathf.Infinity );
            Debug.DrawLine( rightR[ j - 1 ], rightR[ j ], Color.cyan, Mathf.Infinity );

            Debug.DrawLine( leftL[ j - 1 ], leftL[ j ], Color.green, Mathf.Infinity );
            Debug.DrawLine( leftR[ j - 1 ], leftR[ j ], Color.green, Mathf.Infinity );

            Debug.DrawLine( leftLine[ j - 1 ], leftLine[ j ], Color.green, Mathf.Infinity );
            Debug.DrawLine( rightLine[ j - 1 ], rightLine[ j ], Color.cyan, Mathf.Infinity );
        }*/

        Floor singleFloor = new Floor();
        singleFloor.leftL = leftL;
        singleFloor.leftLine = leftLine;
        singleFloor.leftR = leftR;

        singleFloor.centerL = leftR;
        singleFloor.centerLine = curve;
        singleFloor.centerR = rightL;

        singleFloor.rightL = rightL;
        singleFloor.rightLine = rightLine;
        singleFloor.rightR = rightR;

        return singleFloor;
    }

    public static Floor CalculateMonodirectionalFloorMeshVertex( List<Vector3> curve, List<Vector3> controlPoints, float floorWidth, bool floorParabolic )
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
        Vector3 dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ 0 ].x, curve[ 0 ].y, zHeightPrev ) - new Vector3( controlPoints[ 1 ].x, controlPoints[ 1 ].y, zHeightNext ) ).normalized * ( floorWidth / 2 );

        leftPoints.Add( curve[ 0 ] - dir );
        rightPoints.Add(curve[ 0 ] + dir );

        for( int i = 1; i < curve.Count - 1; i++ )
        {
            if( floorParabolic )
            {
                zHeightPrev = curve[ i - 1 ].z;
                zHeightNext = curve[ i + 1 ].z;
            }

            dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ i - 1 ].x, curve[ i - 1 ].y, zHeightPrev ) - new Vector3( curve[ i + 1 ].x, curve[ i + 1 ].y, zHeightNext ) ).normalized * ( floorWidth / 2 );

            leftPoints.Add( curve[ i ] - dir );
            rightPoints.Add(curve[ i ] + dir );
        }

        if( floorParabolic )
        {
            zHeightPrev = curve[ curve.Count - 2 ].z;
            zHeightNext = curve[ curve.Count - 1 ].z;
        }

        dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ curve.Count - 2 ].x, curve[ curve.Count - 2 ].y, zHeightPrev ) - new Vector3( curve[ curve.Count - 1 ].x, curve[ curve.Count - 1 ].y, zHeightNext ) ).normalized * ( floorWidth / 2 );
        leftPoints.Add( curve[ curve.Count - 1 ] - dir );
        rightPoints.Add( curve[ curve.Count - 1 ] + dir );

        Floor singleFloor = new Floor();
        singleFloor.centerL = leftPoints;
        singleFloor.centerLine = curve;
        singleFloor.centerR = rightPoints;

        return singleFloor;
    }

    public static PlatformSide CalculateMonodirectionalPlatformSidesMeshesVertex( List<Vector3> curve, List<Vector3> controlPoints, float floorWidth, bool floorParabolic, float sideHeight, float sideWidth )
    {
        PlatformSide platformSide = new PlatformSide();
        platformSide.leftDown = new List<Vector3>();
        platformSide.rightDown = new List<Vector3>();
        platformSide.leftUp = new List<Vector3>();
        platformSide.rightUp = new List<Vector3>();
        platformSide.leftFloorLeft = new List<Vector3>();
        platformSide.leftFloorRight = new List<Vector3>();
        platformSide.rightFloorLeft = new List<Vector3>();
        platformSide.rightFloorRight = new List<Vector3>();

        float zHeightPrev = 0.0f;
        float zHeightNext = 0.0f;

        if( floorParabolic )
        {
            zHeightPrev = curve[ 0 ].z;
            zHeightNext = curve[ 1 ].z;
        }

        List<Vector3> dirs = new List<Vector3>();
        Vector3 dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ 0 ].x, curve[ 0 ].y, zHeightPrev ) - new Vector3( controlPoints[ 1 ].x, controlPoints[ 1 ].y, zHeightNext ) ).normalized * ( floorWidth / 2 );
        dirs.Add( dir.normalized );

        platformSide.leftDown.Add( curve[ 0 ] - dir );
        platformSide.rightDown.Add(curve[ 0 ] + dir );

        for( int i = 1; i < curve.Count - 1; i++ )
        {
            if( floorParabolic )
            {
                zHeightPrev = curve[ i - 1 ].z;
                zHeightNext = curve[ i + 1 ].z;
            }

            dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ i - 1 ].x, curve[ i - 1 ].y, zHeightPrev ) - new Vector3( curve[ i + 1 ].x, curve[ i + 1 ].y, zHeightNext ) ).normalized * ( floorWidth / 2 );
            dirs.Add( dir.normalized );

            platformSide.leftDown.Add( curve[ i ] - dir );
            platformSide.rightDown.Add(curve[ i ] + dir );
        }

        if( floorParabolic )
        {
            zHeightPrev = curve[ curve.Count - 2 ].z;
            zHeightNext = curve[ curve.Count - 1 ].z;
        }

        dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ curve.Count - 2 ].x, curve[ curve.Count - 2 ].y, zHeightPrev ) - new Vector3( curve[ curve.Count - 1 ].x, curve[ curve.Count - 1 ].y, zHeightNext ) ).normalized * ( floorWidth / 2 );
        dirs.Add( dir.normalized );

        platformSide.leftDown.Add( curve[ curve.Count - 1 ] - dir );
        platformSide.rightDown.Add( curve[ curve.Count - 1 ] + dir );

        for( int i = 0; i < platformSide.leftDown.Count; i++ ) {

            platformSide.leftUp.Add( new Vector3( platformSide.leftDown[ i ].x, platformSide.leftDown[ i ].y, platformSide.leftDown[ i ].z - sideHeight ) ); 
            platformSide.leftFloorRight.Add( platformSide.leftUp[ i ] );
            platformSide.leftFloorLeft.Add( platformSide.leftUp[ i ] - dirs[ i ] * sideWidth );

            platformSide.rightUp.Add( new Vector3( platformSide.rightDown[ i ].x, platformSide.rightDown[ i ].y, platformSide.rightDown[ i ].z - sideHeight ) );
            platformSide.rightFloorLeft.Add( platformSide.rightUp[ i ] );
            platformSide.rightFloorRight.Add( platformSide.rightUp[ i ] + dirs[ i ] * sideWidth );
        }

        return platformSide;
    }

    public static Mesh GenerateFloorMesh( List<Vector3> curve, Vector3[ , ] vertMatrix, float textureHorLenght, float textureVertLenght )
    {
        Mesh floorMesh = new Mesh();

        Vector3[] vertices = new Vector3[ curve.Count * 2 ];
        int[] edges = new int[( curve.Count - 1 ) * 6 ];
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
                meshPercent += ( float )( ( curve[ i ] - curve[ i - 1 ] ).magnitude / textureHorLenght );
            }

            uvs[ ( i * 2 ) ] = new Vector2( meshPercent, 0 );
            uvs[ ( i * 2 ) + 1 ] = new Vector2( meshPercent, textureVertLenght );

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
