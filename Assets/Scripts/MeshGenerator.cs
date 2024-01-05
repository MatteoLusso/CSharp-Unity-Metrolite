using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DLL_MathExt;

public static class MeshGenerator
{
    public class Floor {

        public List<Vector3> railLeftR { get; set; }
        public List<Vector3> railLeftL { get; set; }
        public List<Vector3> railRightR { get; set; }
        public List<Vector3> railRightL { get; set; }

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

    public class ExtrudedMesh{ 
         
         public Mesh mesh { get; set; }
         public List<Vector3> lastProfileVertex { get; set; }
    }

    public class Wall {

    public List<Vector3> rightDown { get; set; }
    public List<Vector3> rightUp { get; set; }

    public List<Vector3> leftDown { get; set; }
    public List<Vector3> leftUp { get; set; }

    }

    public class SpecularBaseLine {
        public List<Vector3> left { get; set; }
        public List<Vector3> right { get; set; }

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

    public static Floor CalculateBidirectionalWithBothSidesPlatformFloorMeshVertex( LineSection section, float centerWidth, float sideWidth, float railsWidth/*, bool floorParabolic,*/ )
    {
        List<Vector3> curve = section.bezierCurveLimitedAngle;
        List<Vector3> controlPoints = section.controlsPoints;

        List<Vector3> railLeftR = new List<Vector3>();
        List<Vector3> railLeftL = new List<Vector3>();
        List<Vector3> railRightR = new List<Vector3>();
        List<Vector3> railRightL = new List<Vector3>();
        
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

        railLeftL.Add( leftLine[ 0 ] - ( dir * ( railsWidth / 2 ) ) );
        railLeftR.Add( leftLine[ 0 ] + ( dir * ( railsWidth / 2 ) ) );
        railRightL.Add( rightLine[ 0 ] + ( dir * ( railsWidth / 2 ) ) );
        railRightR.Add( rightLine[ 0 ] - ( dir * ( railsWidth / 2 ) ) );

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

            railLeftL.Add( leftLine[ i ] - ( dir * ( railsWidth / 2 ) ) );
            railLeftR.Add( leftLine[ i ] + ( dir * ( railsWidth / 2 ) ) );
            railRightL.Add( rightLine[ i ] + ( dir * ( railsWidth / 2 ) ) );
            railRightR.Add( rightLine[ i ] - ( dir * ( railsWidth / 2 ) ) );

            //Debug.DrawLine( railLeftL [ i - 1 ], railLeftL[ i ], Color.red, 999 );
            //Debug.DrawLine( railLeftR [ i - 1 ], railLeftR[ i ], Color.green, 999 );
            //Debug.DrawLine( railRightL [ i - 1 ], railRightL[ i ], Color.red, 999 );
            //Debug.DrawLine( railRightR [ i - 1 ], railRightR[ i ], Color.green, 999 );
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

        railLeftL.Add( leftLine[ curve.Count - 1 ] - ( dir * ( railsWidth / 2 ) ) );
        railLeftR.Add( leftLine[ curve.Count - 1 ] + ( dir * ( railsWidth / 2 ) ) );
        railRightL.Add( rightLine[ curve.Count - 1 ] + ( dir * ( railsWidth / 2 ) ) );
        railRightR.Add( rightLine[ curve.Count - 1 ] - ( dir * ( railsWidth / 2 ) ) );

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

        singleFloor.railLeftL = railLeftL;
        singleFloor.railLeftR = railLeftR;
        singleFloor.railRightL = railRightL;
        singleFloor.railRightR = railRightR;

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

    public static SpecularBaseLine CalculateBaseLinesFromCurve( List<Vector3> curve, List<Vector3> controlPoints, float distance, float angle ) {

        SpecularBaseLine lines = new SpecularBaseLine();
        lines.left = new List<Vector3>();
        lines.right = new List<Vector3>();

        Vector3 curveDir = ( controlPoints[ 1 ] - curve[ 0 ] ).normalized;
        Vector3 leftDir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * curveDir.normalized * distance;
        Vector3 rightDir = -leftDir;

        leftDir = Quaternion.AngleAxis( -angle, curveDir ) * leftDir;
        rightDir = Quaternion.AngleAxis( angle, curveDir ) * rightDir;

        lines.left.Add( curve[ 0 ] + leftDir );
        lines.right.Add( curve[ 0 ] + rightDir );

        for( int i = 1; i < curve.Count - 1; i++ )
        {

            curveDir = ( curve[ i ] - curve[ i - 1 ] ).normalized;
            leftDir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * curveDir.normalized * distance;
            rightDir = -leftDir;

            leftDir = Quaternion.AngleAxis( -angle, curveDir ) * leftDir;
            rightDir = Quaternion.AngleAxis( angle, curveDir ) * rightDir;

            lines.left.Add( curve[ i ] + leftDir );
            lines.right.Add( curve[ i ] + rightDir );

            //Debug.DrawLine( curve[ i ], lines.left[ i ], Color.green, 999 );
            //Debug.DrawLine( curve[ i ], lines.right[ i ], Color.red, 999 );
        }

        curveDir = ( curve[ curve.Count - 1 ] - curve[ curve.Count - 2 ] ).normalized;
        leftDir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * curveDir.normalized * distance;
        rightDir = -leftDir;

        leftDir = Quaternion.AngleAxis( -angle, curveDir ) * leftDir;
        rightDir = Quaternion.AngleAxis( angle, curveDir ) * rightDir;

        lines.left.Add( curve[ curve.Count - 1 ] + leftDir );
        lines.right.Add( curve[ curve.Count - 1 ] + rightDir );

        return lines;
    }

    public static Wall CalculateWallsBaseLines( List<Vector3> curve, List<Vector3> controlPoints, float baseWidth, bool floorParabolic, float sideHeight )
    {
        Wall walls = new Wall();
        walls.leftDown = new List<Vector3>();
        walls.rightDown = new List<Vector3>();

        float zHeightPrev = 0.0f;
        float zHeightNext = 0.0f;

        if( floorParabolic )
        {
            zHeightPrev = curve[ 0 ].z;
            zHeightNext = curve[ 1 ].z;
        }

        float alpha = Mathf.Atan( sideHeight / baseWidth ) * Mathf.Rad2Deg;
        float dirLength = Mathf.Sqrt( Mathf.Pow( sideHeight, 2 ) + Mathf.Pow( baseWidth, 2 ) );

        Vector3 curveDir = ( curve[ 1 ] - curve[ 0 ] ).normalized;
        Vector3 leftDir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ 0 ].x, curve[ 0 ].y, zHeightPrev ) - new Vector3( controlPoints[ 1 ].x, controlPoints[ 1 ].y, zHeightNext ) ).normalized * dirLength;
        leftDir = Quaternion.AngleAxis( alpha, curveDir ) * leftDir;
        Vector3 rightDir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ 0 ].x, curve[ 0 ].y, zHeightPrev ) - new Vector3( controlPoints[ 1 ].x, controlPoints[ 1 ].y, zHeightNext ) ).normalized * dirLength;
        rightDir = Quaternion.AngleAxis( -alpha, curveDir ) * rightDir;

        walls.leftDown.Add( curve[ 0 ] + leftDir );
        walls.rightDown.Add( curve[ 0 ] - rightDir );

        for( int i = 1; i < curve.Count - 1; i++ )
        {
            if( floorParabolic )
            {
                zHeightPrev = curve[ i - 1 ].z;
                zHeightNext = curve[ i + 1 ].z;
            }

            curveDir = ( curve[ i ] - curve[ i - 1 ] ).normalized;

            Debug.DrawRay( curve[ 0 ], curveDir * 10, Color.cyan, 999 );

            leftDir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ i - 1 ].x, curve[ i - 1 ].y, zHeightPrev ) - new Vector3( curve[ i + 1 ].x, curve[ i + 1 ].y, zHeightNext ) ).normalized * dirLength;
            leftDir = Quaternion.AngleAxis( alpha, curveDir ) * leftDir;
            rightDir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ i - 1 ].x, curve[ i - 1 ].y, zHeightPrev ) - new Vector3( curve[ i + 1 ].x, curve[ i + 1 ].y, zHeightNext ) ).normalized * dirLength;
            rightDir = Quaternion.AngleAxis( -alpha, curveDir ) * rightDir;

            Debug.DrawRay( curve[ i ], leftDir, Color.green, 999 );
            Debug.DrawRay( curve[ i ], -rightDir, Color.red, 999 );

            walls.leftDown.Add( curve[ i ] + leftDir );
            walls.rightDown.Add( curve[ i ] - rightDir );
        }

        if( floorParabolic )
        {
            zHeightPrev = curve[ curve.Count - 2 ].z;
            zHeightNext = curve[ curve.Count - 1 ].z;
        }

        curveDir = ( curve[ curve.Count - 1 ] - curve[ curve.Count - 2 ] ).normalized;

        leftDir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ curve.Count - 2 ].x, curve[ curve.Count - 2 ].y, zHeightPrev ) - new Vector3( curve[ curve.Count - 1 ].x, curve[ curve.Count - 1 ].y, zHeightNext ) ).normalized * dirLength;
        leftDir = Quaternion.AngleAxis( alpha, curveDir ) * leftDir;
        rightDir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ curve.Count - 2 ].x, curve[ curve.Count - 2 ].y, zHeightPrev ) - new Vector3( curve[ curve.Count - 1 ].x, curve[ curve.Count - 1 ].y, zHeightNext ) ).normalized * dirLength;
        rightDir = Quaternion.AngleAxis( -alpha, curveDir ) * rightDir;

        walls.leftDown.Add( curve[ curve.Count - 1 ] + leftDir );
        walls.rightDown.Add( curve[ curve.Count - 1 ] - rightDir );

        return walls;
    }

    public static Wall CalculateWallsMeshesVertex( List<Vector3> curve, List<Vector3> controlPoints, float baseWidth, bool floorParabolic, float sideHeight, float wallHeight )
    {
        Wall walls = new Wall();
        walls.leftDown = new List<Vector3>();
        walls.rightDown = new List<Vector3>();
        walls.leftUp = new List<Vector3>();
        walls.rightUp = new List<Vector3>();

        float zHeightPrev = 0.0f;
        float zHeightNext = 0.0f;

        if( floorParabolic )
        {
            zHeightPrev = curve[ 0 ].z;
            zHeightNext = curve[ 1 ].z;
        }

        float alpha = Mathf.Atan( sideHeight / baseWidth ) * Mathf.Rad2Deg;
        float dirLength = Mathf.Sqrt( Mathf.Pow( sideHeight, 2 ) + Mathf.Pow( baseWidth, 2 ) );

        Vector3 leftDir = Quaternion.Euler( alpha, 0.0f, 90.0f ) * ( new Vector3( curve[ 0 ].x, curve[ 0 ].y, zHeightPrev ) - new Vector3( controlPoints[ 1 ].x, controlPoints[ 1 ].y, zHeightNext ) ).normalized * dirLength;
        Vector3 rightDir = Quaternion.Euler( -alpha, 0.0f, 90.0f ) * ( new Vector3( curve[ 0 ].x, curve[ 0 ].y, zHeightPrev ) - new Vector3( controlPoints[ 1 ].x, controlPoints[ 1 ].y, zHeightNext ) ).normalized * dirLength;

        walls.leftDown.Add( curve[ 0 ] - leftDir );
        walls.leftUp.Add( new Vector3( walls.leftDown[ 0 ].x, walls.leftDown[ 0 ].y, walls.leftDown[ 0 ].z - wallHeight ) );
        walls.rightDown.Add( curve[ 0 ] + rightDir );
        walls.rightUp.Add( new Vector3( walls.rightDown[ 0 ].x, walls.rightDown[ 0 ].y, walls.rightDown[ 0 ].z - wallHeight ) );

        for( int i = 1; i < curve.Count - 1; i++ )
        {
            if( floorParabolic )
            {
                zHeightPrev = curve[ i - 1 ].z;
                zHeightNext = curve[ i + 1 ].z;
            }

            leftDir = Quaternion.Euler( alpha, 0.0f, 90.0f ) * ( new Vector3( curve[ i - 1 ].x, curve[ i - 1 ].y, zHeightPrev ) - new Vector3( curve[ i + 1 ].x, curve[ i + 1 ].y, zHeightNext ) ).normalized * dirLength;
            rightDir = Quaternion.Euler( -alpha, 0.0f, 90.0f ) * ( new Vector3( curve[ i - 1 ].x, curve[ i - 1 ].y, zHeightPrev ) - new Vector3( curve[ i + 1 ].x, curve[ i + 1 ].y, zHeightNext ) ).normalized * dirLength;

            walls.leftDown.Add( curve[ i ] - leftDir );
            walls.leftUp.Add( new Vector3( walls.leftDown[ i ].x, walls.leftDown[ i ].y, walls.leftDown[ i ].z - wallHeight ) );
            walls.rightDown.Add( curve[ i ] + rightDir );
            walls.rightUp.Add( new Vector3( walls.rightDown[ i ].x, walls.rightDown[ i ].y, walls.rightDown[ i ].z - wallHeight ) );
        }

        if( floorParabolic )
        {
            zHeightPrev = curve[ curve.Count - 2 ].z;
            zHeightNext = curve[ curve.Count - 1 ].z;
        }

        leftDir = Quaternion.Euler( alpha, 0.0f, 90.0f ) * ( new Vector3( curve[ curve.Count - 2 ].x, curve[ curve.Count - 2 ].y, zHeightPrev ) - new Vector3( curve[ curve.Count - 1 ].x, curve[ curve.Count - 1 ].y, zHeightNext ) ).normalized * dirLength;
        rightDir = Quaternion.Euler( -alpha, 0.0f, 90.0f ) * ( new Vector3( curve[ curve.Count - 2 ].x, curve[ curve.Count - 2 ].y, zHeightPrev ) - new Vector3( curve[ curve.Count - 1 ].x, curve[ curve.Count - 1 ].y, zHeightNext ) ).normalized * dirLength;

        walls.leftDown.Add( curve[ curve.Count - 1 ] - leftDir );
        walls.leftUp.Add( new Vector3( walls.leftDown[ curve.Count - 1 ].x, walls.leftDown[ curve.Count - 1 ].y, walls.leftDown[ curve.Count - 1 ].z - wallHeight ) );
        walls.rightDown.Add( curve[ curve.Count - 1 ] + rightDir );
        walls.rightUp.Add( new Vector3( walls.rightDown[ curve.Count - 1 ].x, walls.rightDown[ curve.Count - 1 ].y, walls.rightDown[ curve.Count - 1 ].z - wallHeight ) );

        return walls;
    }

    public static PlatformSide CalculatePlatformSidesMeshesVertex( List<Vector3> curve, List<Vector3> controlPoints, float floorWidth, bool floorParabolic, float sideHeight, float sideWidth )
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

    public static List<Vector3> CalculateCircularShape( float radius, int points, Vector2 centerCoords, float eccentricity ) {
        List<Vector3> shape = new List<Vector3>();

        float fixedAngle = 360.0f / points;

        float k = Mathf.Sqrt( 1 - Mathf.Pow( eccentricity, 2 ) );
        float a = radius / k;
        float b = a * k;

        float alpha = 0.0f;
        for( int i = 0; i < points; i++ ) {
            //Debug.Log( ">>> alpha: " + alpha );
            float x = centerCoords.x + ( a * Mathf.Cos( alpha * Mathf.Deg2Rad ) );
            float y = centerCoords.y + ( b * Mathf.Sin( alpha * Mathf.Deg2Rad ) );

            shape.Add( new Vector3( 0.0f, x, y ) );

            alpha += fixedAngle;
        }
        shape.Add( shape[ 0 ] );

        return shape;
    }

    public static ExtrudedMesh GenerateExtrudedMesh( List<Vector3> profileVertices, float profileScale, List<Vector3> previousProfileVertices, List<Vector3> baseVertices, float horPosCorrection, bool clockwiseRotation, float textureHorLenght, float textureVertLenght, float verticalRotationCorrection, float smoothFactor ) {
        ExtrudedMesh extrudedMesh = new ExtrudedMesh();

        List<Vector3> lastProfileVertices = new List<Vector3>();

        List<Vector3> profileVerticesScaled = new List<Vector3>();
        foreach( Vector3 profileVertex in profileVertices ) {
            profileVerticesScaled.Add( profileVertex * profileScale );
        }
        
        Mesh mesh = new Mesh();
        mesh.name = "Procedural Mesh";

        int h = profileVerticesScaled.Count;

        int[] triangles = new int[ ( h - 1 ) * ( baseVertices.Count - 1 ) * 6 ];
        Vector3[] vertices = new Vector3[ profileVerticesScaled.Count * baseVertices.Count ];
        Vector2[] uv = new Vector2[ profileVerticesScaled.Count * baseVertices.Count ];
        Vector3[] normals = new Vector3[ profileVerticesScaled.Count * baseVertices.Count ];

        // Array di supporto con le distanze dei punti del profilo e della base calcolate rispetto allo zero (serve per gestire l'UV mapping ripetuto)
        List<float> distancesHor = new List<float>();
        distancesHor.Add( 0.0f );
        List<float> distancesVert = new List<float>();
        distancesVert.Add( 0.0f );

        List<Vector3> profileDirs = new List<Vector3>();
        List<Vector3> baseDirs = new List<Vector3>();
        List<float>baseAlphas = new List<float>();
        for( int i = 1; i < h; i++ ) {

            Vector3 profileDir = profileVerticesScaled[ i ] - profileVerticesScaled[ i - 1 ];
            profileDirs.Add( profileDir );

            distancesVert.Add( distancesVert[ i - 1 ] + profileDir.magnitude );
        }
        for( int i = 1; i < baseVertices.Count; i++ ) { 

            Vector3 baseDir = baseVertices[ i ] - baseVertices[ i - 1 ];
            baseDirs.Add( baseDir );

            Debug.DrawRay( baseVertices[ i - 1 ], baseDirs[ i - 1 ], Color.red, 999 );

            baseAlphas.Add( DLL_MathExt.Angles.SignedAngleTo360Angle( Vector3.SignedAngle( Vector3.right, baseDir, Vector3.forward ) ) );
            //baseAlphas[ i - 1 ] += baseAlphas[ i - 1 ] > 180.0f ? 90.0f : 0.0f;

            Debug.Log( "baseAlpha[ " + ( i - 1 ) + "]: " + baseAlphas[ i - 1 ] );

            distancesHor.Add( distancesHor[ i - 1 ] + baseDir.magnitude );
        }
        baseAlphas.Add( baseAlphas[ baseAlphas.Count - 1 ] );
        //baseAlphas[ baseAlphas.Count - 1 ] -= baseAlphas[ baseAlphas.Count - 1 ] >= 180.0f ? -180.0f : 0.0f;

        // Genero i tutti i profili e aggiungo i vertici alla lista dei vertici
        for( int i = 0; i < baseVertices.Count; i++ ) {
            
            // Alpha è l'angolo che il profilo deve essere ruotato per risultare sempre alla stessa angolazione con il baseDir attuale
            float alpha = baseAlphas[ i ];
            if( i > 0 && i < baseVertices.Count - 1 ) {

                float prevAlpha = baseAlphas[ i - 1 ];

                float deltaAlpha = alpha - prevAlpha;
                int rotDir = deltaAlpha > 0 ? -1 : 1;

                float deltaAlphaAbs = Mathf.Abs( alpha - prevAlpha );
                deltaAlphaAbs = deltaAlphaAbs > 180 ? 360 - deltaAlphaAbs : deltaAlphaAbs;

                alpha += deltaAlphaAbs * smoothFactor * rotDir;
            }

            if( i == 0 && previousProfileVertices != null && previousProfileVertices.Count == profileVerticesScaled.Count ) {
                vertices[ 0 ] = previousProfileVertices [ 0 ];
            }
            else {

                if( i >= baseDirs.Count ) {
                    vertices[ i * h ] = baseVertices[ i ] + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * baseDirs[ baseDirs.Count - 1 ].normalized * horPosCorrection * profileScale );
                }
                else {
                    vertices[ i * h ] = baseVertices[ i ] + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * baseDirs[ i ].normalized * horPosCorrection * profileScale );
                }

                if( i == baseVertices.Count - 1 ) {
                    lastProfileVertices.Add( vertices[ i * h ] );
                }
            }
            //Debug.DrawLine( vertices[ i * h ], baseVertices[ i ], Color.cyan, 999 ); 
            float u = distancesHor[ i ] / textureHorLenght;
            uv[ i * h ] = new Vector2( u, 0 );

            // Il numero di dir orizontali e verticali è minore di 1 rispetto al nomero di vertici che stiamo ciclando, quindi per l'ultimo punto calcolo la normale usando la dire precedente
            int normalHorIndex = i < baseDirs.Count ? i : ( baseDirs.Count - 1 );
            Vector3 normal = Vector3.Cross( profileDirs[ 0 ], baseDirs[ normalHorIndex ] ).normalized;
            //Debug.DrawRay( vertices[ i * h  ], baseDirs[ normalHorIndex ], Color.yellow, 9999 );
            if( !clockwiseRotation ) {
                normal = -normal;
            }
            normals[ i * h ] = normal;

            //normals[ i * h ] = clockwiseRotation ? Vector3.Cross( profileDirs[ 0 ], baseDirs[ normalHorIndex ] ).normalized : Quaternion.AngleAxis( ( Vector3.SignedAngle( , -Vector3.forward, baseDirs[ normalHorIndex ] ) ), baseDirs[ normalHorIndex ] ) * Vector3.Cross( profileDirs[ 0 ], baseDirs[ normalHorIndex ] ).normalized;

            //float beta = Vector3.SignedAngle( Vector3.right, baseDirs[ normalHorIndex ], Vector3.up );
            //Debug.Log( ">>> beta: " + beta );

            for( int j = 0; j < profileDirs.Count; j++ ) {

                if( i == 0 && previousProfileVertices != null && previousProfileVertices.Count == profileVertices.Count ) {
                    vertices[ j + 1 ] = previousProfileVertices [ j + 1 ];
                }
                else {
                    vertices[ ( i * h ) + j + 1 ] = ( vertices[ ( i * h ) + j ] + Quaternion.Euler( 0.0f, /*beta*/0.0f, alpha + verticalRotationCorrection ) * profileDirs[ j ] );

                    if( i == baseVertices.Count - 1 ) {
                        lastProfileVertices.Add( vertices[ ( i * h ) + j + 1 ] );
                    }
                }

                //Debug.DrawLine( vertices[ ( i * h ) + j + 1 ], baseVertices[ i ], Color.cyan, 999 ); 

                float v = distancesVert[ j + 1 ] / textureVertLenght;
                uv[ ( i * h ) + j + 1 ] = new Vector2( u, v );

                int normalVertIndex = ( j + 1 ) < profileDirs.Count ? ( j + 1 ) : ( profileDirs.Count - 1 );
                //normals[ ( i * h ) + j + 1 ] = clockwiseRotation ? Vector3.Cross( profileDirs[ normalVertIndex ], baseDirs[ normalHorIndex ] ).normalized : Quaternion.AngleAxis( 0.0f, baseDirs[ normalHorIndex ] ) * Vector3.Cross( profileDirs[ normalVertIndex ], baseDirs[ normalHorIndex ] ).normalized;
                normal = Vector3.Cross( Quaternion.Euler( 0.0f, /*beta*/0.0f, alpha + verticalRotationCorrection ) * profileDirs[ normalVertIndex ], baseDirs[ normalHorIndex ] ).normalized;
                //Debug.DrawRay( vertices[ ( i * h ) ], profileDirs[ normalVertIndex ], Color.cyan, 9999 );
                if( !clockwiseRotation ) {
                    normal = -normal;
                }
                normals[ ( i * h ) + j + 1 ] = normal;
                
                Debug.DrawRay( vertices[ ( i * h ) + j + 1 ], normals[ ( i * h ) + j + 1 ] * 0.5f, Color.red, 9999 );

                if( i > 0 ) {
                    int arrayIndex = ( 6 * ( ( (  i - 1 ) * ( h - 1 ) ) + j ) );
                    int vertIndex = ( int )( arrayIndex / 6 ) + ( i - 1 );

                    triangles[ arrayIndex + 0 ] = triangles[ arrayIndex + 3 ] = vertIndex;

                    if( clockwiseRotation ) {

                        triangles[ arrayIndex + 1 ] = triangles[ arrayIndex + 5 ] = vertIndex + ( h + 1 ); 
                        triangles[ arrayIndex + 2 ] = vertIndex + h;
                        triangles[ arrayIndex + 4 ] = vertIndex + 1;
                    }
                    else {
                        triangles[ arrayIndex + 1 ] = vertIndex + h;
                        triangles[ arrayIndex + 2 ] = triangles[ arrayIndex + 4 ] = vertIndex + ( h + 1 ); 
                        triangles[ arrayIndex + 5 ] = vertIndex + 1;
                    }
                }
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.normals = normals;

        extrudedMesh.mesh = mesh;
        extrudedMesh.lastProfileVertex = lastProfileVertices;

        return extrudedMesh;
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
