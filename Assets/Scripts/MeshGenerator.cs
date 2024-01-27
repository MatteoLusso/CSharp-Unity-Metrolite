using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DLL_MathExt;

public static class MeshGenerator
{
    public class Floor {

        public List<Vector3> railLeftR { get; set; }
        public List<Vector3> railLeftL { get; set; }
        public List<Vector3> railCenterR { get; set; }
        public List<Vector3> railCenterL { get; set; }
        public List<Vector3> railRightR { get; set; }
        public List<Vector3> railRightL { get; set; }
        public List<Vector3> railSwitchLeftCenterR { get; set; }
        public List<Vector3> railSwitchLeftCenterL { get; set; }
        public List<Vector3> railSwitchRightCenterR { get; set; }
        public List<Vector3> railSwitchRightCenterL { get; set; }
        public List<Vector3> railSwitchLeftRightR { get; set; }
        public List<Vector3> railSwitchLeftRightL { get; set; }
        public List<Vector3> railSwitchRightLeftR { get; set; }
        public List<Vector3> railSwitchRightLeftL { get; set; }

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

        public List<Vector3> switchBiNewGroundBaseLine { get; set; }
        public List<Vector3> switchBiNewGroundBez0Line { get; set; }
        public List<Vector3> switchBiNewGroundBez1Line { get; set; }
        public List<Vector3> switchBiNewGroundBez2Line { get; set; }
        public List<Vector3> switchBiNewGroundLimitPoints0 { get; set; }
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
         public GameObject gameObj { get; set; }
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

    public static Mesh GenerateSwitchNewBiGround( NewLineSide side, List<Vector3> points0, List<Vector3> points1, int start0, int end0, List<Vector3> points2, int minIndex1, int maxIndex1, Vector3 switchDir, bool clockwiseRotation, Vector2 textureTilting, float tunnelWidth, float centerWidth ) {
        
        Mesh groundMesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uv = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();

        int verticesCounter = 0;
        float u, v;

        List<Vector3> groundLimitPoints0 = new List<Vector3>();
        groundLimitPoints0.Add( points0[ 0 ] + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f * ( side == NewLineSide.Left ? -1 : 1 ) ) * switchDir.normalized * tunnelWidth ) );

        for( int i = 0; i <= start0; i++ ) {
            groundLimitPoints0.Add( points0[ i ] );
        }

        for( int i = maxIndex1; i >= minIndex1; i-- ) {
            groundLimitPoints0.Add( points2[ i ] );
        }

        groundLimitPoints0.Add( points2[ minIndex1 ] + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f * ( side == NewLineSide.Left ? -1 : 1 ) ) * switchDir.normalized * tunnelWidth ) );
        groundLimitPoints0.Add( groundLimitPoints0[ 0 ] + ( switchDir.normalized * ( groundLimitPoints0[ groundLimitPoints0.Count - 1 ] - groundLimitPoints0[ 0 ] ).magnitude / 2 ) );

        vertices.AddRange( groundLimitPoints0 );

        for( int i = 0; i < groundLimitPoints0.Count - 1; i++ ){
            
            if( i < groundLimitPoints0.Count - 2 ) {

                triangles.Add( verticesCounter + i );
                if( clockwiseRotation ) {
                    triangles.Add( verticesCounter + i + 1 );
                    triangles.Add( verticesCounter + groundLimitPoints0.Count - 1 );
                }
                else {
                    triangles.Add( verticesCounter + groundLimitPoints0.Count - 1 );
                    triangles.Add( verticesCounter + i + 1 );
                }


            }

            normals.Add( -Vector3.forward );
            u = MeshGenerator.CalculateDistanceFromPointToLine( groundLimitPoints0[ i ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
            v = MeshGenerator.CalculateDistanceFromPointToLine( groundLimitPoints0[ i ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
            uv.Add( new Vector2( u, v ) );
            //Debug.Log( ">>> ( u, v ): ( " + u + ", " + v + " )" );

            //Debug.DrawLine( groundLimitPoints0[ i ], groundLimitPoints0[ i + 1 ], Color.magenta, Mathf.Infinity );
            //Debug.DrawLine( groundLimitPoints0[ i ], groundLimitPoints0[ groundLimitPoints0.Count - 1 ], Color.magenta, Mathf.Infinity );
        }
        //Debug.DrawLine( groundLimitPoints0[ groundLimitPoints0.Count - 1 ], groundLimitPoints0[ 0 ], Color.magenta, Mathf.Infinity );
        normals.Add( -Vector3.forward );
        u = MeshGenerator.CalculateDistanceFromPointToLine( groundLimitPoints0[ groundLimitPoints0.Count - 1 ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
        v = MeshGenerator.CalculateDistanceFromPointToLine( groundLimitPoints0[ groundLimitPoints0.Count - 1 ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
        uv.Add( new Vector2( u, v ) );

        verticesCounter = vertices.Count;

        /////////////////////

        List<Vector3> up1 = new List<Vector3>();
        List<Vector3> down1 = new List<Vector3>();

        for( int i = start0; i < points0.Count - end0; i++ ) {
            up1.Add( points0[ i ] );
        }
        
        for( int i = minIndex1 + maxIndex1 - 1; i < points0.Count - 1 - start0 - end0 + minIndex1 + maxIndex1; i++ ) {
            down1.Add( points2[ i ] );
        }

        for( int i = 0; i < down1.Count - 1; i++ ){
            
            vertices.Add( down1[ i ] );
            vertices.Add( up1[ i ] );

            triangles.Add( verticesCounter + ( i * 2 ) );
            if( clockwiseRotation ) {
                triangles.Add( verticesCounter + ( i * 2 ) + 1 );
                triangles.Add( verticesCounter + ( i * 2 ) + 2 );
            }
            else {
                triangles.Add( verticesCounter + ( i * 2 ) + 2 );
                triangles.Add( verticesCounter + ( i * 2 ) + 1 );
            }

            triangles.Add( verticesCounter + ( i * 2 ) + 2 );
            if( clockwiseRotation ) {
                triangles.Add( verticesCounter + ( i * 2 ) + 1 );
                triangles.Add( verticesCounter + ( i * 2 ) + 3 );
            }
            else {
                triangles.Add( verticesCounter + ( i * 2 ) + 3 );
                triangles.Add( verticesCounter + ( i * 2 ) + 1 );
            }

            normals.Add( -Vector3.forward );
            normals.Add( -Vector3.forward );

            u = MeshGenerator.CalculateDistanceFromPointToLine( down1[ i ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
            v = MeshGenerator.CalculateDistanceFromPointToLine( down1[ i ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
            uv.Add( new Vector2( u, v ) );

            u = MeshGenerator.CalculateDistanceFromPointToLine( up1[ i ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
            v = MeshGenerator.CalculateDistanceFromPointToLine( up1[ i ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
            uv.Add( new Vector2( u, v ) );

            // Debug.DrawLine( up1[ i ], up1[ i + 1 ], Color.red, Mathf.Infinity );
            // Debug.DrawLine( down1[ i ], down1[ i + 1 ], Color.red, Mathf.Infinity );
            // Debug.DrawLine( down1[ i ], up1[ i + 1 ], Color.red, Mathf.Infinity );
        }

        vertices.Add( down1[ down1.Count - 1 ] );
        vertices.Add( up1[ up1.Count - 1 ] );

        normals.Add( -Vector3.forward );
        normals.Add( -Vector3.forward );

        u = MeshGenerator.CalculateDistanceFromPointToLine( down1[ down1.Count - 1 ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
        v = MeshGenerator.CalculateDistanceFromPointToLine( down1[ down1.Count - 1 ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
        uv.Add( new Vector2( u, v ) );

        u = MeshGenerator.CalculateDistanceFromPointToLine( up1[ up1.Count - 1 ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
        v = MeshGenerator.CalculateDistanceFromPointToLine( up1[ up1.Count - 1 ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
        uv.Add( new Vector2( u, v ) );

        verticesCounter = vertices.Count;

        /////////////////////

        List<Vector3> groundLimitPoints2 = new List<Vector3>();

        for( int i = 0; i <= end0; i++ ) {
            groundLimitPoints2.Add( points1[ i ] );
        }
        
        for( int i = ( points0.Count - 1 ) - start0 - end0 + ( 2 * maxIndex1 ) - 1; i > ( points0.Count - 1 ) - start0 - end0 + maxIndex1 - 1; i-- ) {
            groundLimitPoints2.Add( points2[ i ] );
        }

        for( int i = ( points0.Count - 1 ) - end0; i < points0.Count; i++ ) {
            groundLimitPoints2.Add( points0[ i ] );
        }

        groundLimitPoints2.Add( groundLimitPoints2[ 0 ] - ( switchDir.normalized * ( tunnelWidth + ( centerWidth / 2 ) ) ) );

        vertices.AddRange( groundLimitPoints2 );

        for( int i = 0; i < groundLimitPoints2.Count - 1; i++ ){

            triangles.Add( verticesCounter + i );
            if( clockwiseRotation ) {
                triangles.Add( verticesCounter + i + 1 );
                triangles.Add( verticesCounter + groundLimitPoints2.Count - 1 );
            }
            else {
                triangles.Add( verticesCounter + groundLimitPoints2.Count - 1 );
                triangles.Add( verticesCounter + i + 1 );
            }

            normals.Add( -Vector3.forward );

            u = MeshGenerator.CalculateDistanceFromPointToLine( groundLimitPoints2[ i ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
            v = MeshGenerator.CalculateDistanceFromPointToLine( groundLimitPoints2[ i ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
            uv.Add( new Vector2( u, v ) );

            //Debug.DrawLine( groundLimitPoints2[ i ], groundLimitPoints2[ i + 1 ], Color.magenta, Mathf.Infinity );
            //Debug.DrawLine( groundLimitPoints2[ i ], groundLimitPoints2[ groundLimitPoints2.Count - 1 ], Color.magenta, Mathf.Infinity );
        }
        //Debug.DrawLine( groundLimitPoints2[ groundLimitPoints2.Count - 1 ], groundLimitPoints2[ 0 ], Color.magenta, Mathf.Infinity );

        normals.Add( -Vector3.forward );

        u = MeshGenerator.CalculateDistanceFromPointToLine( groundLimitPoints2[ groundLimitPoints2.Count - 1 ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
        v = MeshGenerator.CalculateDistanceFromPointToLine( groundLimitPoints2[ groundLimitPoints2.Count - 1 ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
        uv.Add( new Vector2( u, v ) );

        verticesCounter = vertices.Count;

        /////////////////////

        List<Vector3> up3 = new List<Vector3>();
        List<Vector3> down3 = new List<Vector3>();

        for( int i = end0; i < points1.Count - start0; i++ ) {
            up3.Add( points1[ i ] );
        }
        
        for( int i = minIndex1 + ( 2 * ( maxIndex1 - 1 ) ) + points0.Count - start0 - end0 - 1; i < points2.Count - maxIndex1 + 2; i++ ) {
            down3.Add( points2[ i ] );
        }

        for( int i = 0; i < up3.Count - 1; i++ ){

            vertices.Add( down3[ i ] );
            vertices.Add( up3[ i ] );
            
            triangles.Add( verticesCounter + ( i * 2 ) );
            if( clockwiseRotation ) {
                triangles.Add( verticesCounter + ( i * 2 ) + 1 );
                triangles.Add( verticesCounter + ( i * 2 ) + 2 );
            }
            else {
                triangles.Add( verticesCounter + ( i * 2 ) + 2 );
                triangles.Add( verticesCounter + ( i * 2 ) + 1 );
            }

            triangles.Add( verticesCounter + ( i * 2 ) + 2 );
            if( clockwiseRotation ) {
                triangles.Add( verticesCounter + ( i * 2 ) + 1 );
                triangles.Add( verticesCounter + ( i * 2 ) + 3 );
            }
            else {
                triangles.Add( verticesCounter + ( i * 2 ) + 3 );
                triangles.Add( verticesCounter + ( i * 2 ) + 1 );
            }

            normals.Add( -Vector3.forward );
            normals.Add( -Vector3.forward );

            u = MeshGenerator.CalculateDistanceFromPointToLine( down3[ i ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
            v = MeshGenerator.CalculateDistanceFromPointToLine( down3[ i ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
            uv.Add( new Vector2( u, v ) );

            u = MeshGenerator.CalculateDistanceFromPointToLine( up3[ i ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
            v = MeshGenerator.CalculateDistanceFromPointToLine( up3[ i ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
            uv.Add( new Vector2( u, v ) );

            //Debug.DrawLine( up3[ i ], up3[ i + 1 ], Color.red, Mathf.Infinity );
            //Debug.DrawLine( down3[ i ], down3[ i + 1 ], Color.red, Mathf.Infinity );
            //Debug.DrawLine( down3[ i ], up3[ i + 1 ], Color.red, Mathf.Infinity );
        }

        vertices.Add( down3[ down3.Count - 1 ] );
        vertices.Add( up3[ up3.Count - 1 ] );

        normals.Add( -Vector3.forward );
        normals.Add( -Vector3.forward );

        u = MeshGenerator.CalculateDistanceFromPointToLine( down3[ down3.Count - 1 ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
        v = MeshGenerator.CalculateDistanceFromPointToLine( down3[ down3.Count - 1 ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
        uv.Add( new Vector2( u, v ) );

        u = MeshGenerator.CalculateDistanceFromPointToLine( up3[ up3.Count - 1 ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
        v = MeshGenerator.CalculateDistanceFromPointToLine( up3[ up3.Count - 1 ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
        uv.Add( new Vector2( u, v ) );

        verticesCounter = vertices.Count;

        /////////////////////

        List<Vector3> groundLimitPoints4 = new List<Vector3>();

        groundLimitPoints4.Add( points2[ 0 ] + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f * ( side == NewLineSide.Left ? -1 : 1 )  ) * switchDir.normalized * tunnelWidth ) );
        groundLimitPoints4.Add( points2[ 0 ] );

        for( int i = points2.Count - 1; i > ( points2.Count - 1 ) - maxIndex1 + minIndex1; i-- ) {
            groundLimitPoints4.Add( points2[ i ] );
        }

        for( int i = ( points1.Count - 1 ) - start0; i <= points1.Count - 1; i++ ) {
            groundLimitPoints4.Add( points1[ i ] );
        }

        groundLimitPoints4.Add( groundLimitPoints4[ groundLimitPoints4.Count - 1 ] + ( Quaternion.Euler( 0.0f, 0.0f, 90.0f * ( side == NewLineSide.Left ? -1 : 1 ) ) * switchDir.normalized * tunnelWidth ) );
        groundLimitPoints4.Add( groundLimitPoints4[ groundLimitPoints4.Count - 1 ] - ( switchDir.normalized * ( groundLimitPoints4[ groundLimitPoints4.Count - 1 ] - groundLimitPoints4[ 0 ] ).magnitude / 2 ) );

        vertices.AddRange( groundLimitPoints4 );

        for( int i = 0; i < groundLimitPoints4.Count - 1; i++ ){

            triangles.Add( verticesCounter + i );
            if( clockwiseRotation ) {
                triangles.Add( verticesCounter + i + 1 );
                triangles.Add( verticesCounter + groundLimitPoints4.Count - 1 );
            }
            else {
                triangles.Add( verticesCounter + groundLimitPoints4.Count - 1 );
                triangles.Add( verticesCounter + i + 1 );
            }

            normals.Add( -Vector3.forward );

            u = MeshGenerator.CalculateDistanceFromPointToLine( groundLimitPoints4[ i ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
            v = MeshGenerator.CalculateDistanceFromPointToLine( groundLimitPoints4[ i ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
            uv.Add( new Vector2( u, v ) );

            //Debug.DrawLine( groundLimitPoints4[ i ], groundLimitPoints4[ i + 1 ], Color.magenta, Mathf.Infinity );
            //Debug.DrawLine( groundLimitPoints4[ i ], groundLimitPoints4[ groundLimitPoints4.Count - 1 ], Color.magenta, Mathf.Infinity );
        }
        //Debug.DrawLine( groundLimitPoints4[ groundLimitPoints4.Count - 1 ], groundLimitPoints4[ 0 ], Color.magenta, Mathf.Infinity );

        normals.Add( -Vector3.forward );

        u = MeshGenerator.CalculateDistanceFromPointToLine( groundLimitPoints4[ groundLimitPoints4.Count - 1 ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
        v = MeshGenerator.CalculateDistanceFromPointToLine( groundLimitPoints4[ groundLimitPoints4.Count - 1 ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
        uv.Add( new Vector2( u, v ) );

        verticesCounter = vertices.Count;

        /////////////////////

        List<Vector3> up5 = new List<Vector3>();
        List<Vector3> down5 = new List<Vector3>();

        up5.Add( groundLimitPoints0[ groundLimitPoints0.Count - 3 ] );
        up5.Add( groundLimitPoints4[ 2 ] );
        down5.Add( groundLimitPoints0[ groundLimitPoints0.Count - 2 ] );
        down5.Add( groundLimitPoints4[ 0 ] );

        for( int i = 0; i < up5.Count - 1; i++ ) {

            vertices.Add( down5[ i ] );
            vertices.Add( up5[ i ] );

            triangles.Add( verticesCounter + ( i * 2 ) );
            if( clockwiseRotation ) {
                triangles.Add( verticesCounter + ( i * 2 ) + 1 );
                triangles.Add( verticesCounter + ( i * 2 ) + 2 );
            }
            else {
                triangles.Add( verticesCounter + ( i * 2 ) + 2 );
                triangles.Add( verticesCounter + ( i * 2 ) + 1 );
            }

            triangles.Add( verticesCounter + ( i * 2 ) + 2 );
            if( clockwiseRotation ) {
                triangles.Add( verticesCounter + ( i * 2 ) + 1 );
                triangles.Add( verticesCounter + ( i * 2 ) + 3 );
            }
            else {
                triangles.Add( verticesCounter + ( i * 2 ) + 3 );
                triangles.Add( verticesCounter + ( i * 2 ) + 1 );
            }

            normals.Add( -Vector3.forward );
            normals.Add( -Vector3.forward );

            u = CalculateDistanceFromPointToLine( down5[ i ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
            v = CalculateDistanceFromPointToLine( down5[ i ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
            uv.Add( new Vector2( u, v ) );

            u = CalculateDistanceFromPointToLine( up5[ i ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
            v = CalculateDistanceFromPointToLine( up5[ i ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
            uv.Add( new Vector2( u, v ) );

            //Debug.DrawLine( up5[ i ], up5[ i + 1 ], Color.red, Mathf.Infinity );
            //Debug.DrawLine( down5[ i ], down5[ i + 1 ], Color.red, Mathf.Infinity );
            //Debug.DrawLine( down5[ i ], up5[ i + 1 ], Color.red, Mathf.Infinity );
        }

        vertices.Add( down5[ down5.Count - 1 ] );
        vertices.Add( up5[ up5.Count - 1 ] );

        normals.Add( -Vector3.forward );
        normals.Add( -Vector3.forward );

        u = CalculateDistanceFromPointToLine( down5[ down5.Count - 1 ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
        v = CalculateDistanceFromPointToLine( down5[ down5.Count - 1 ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
        uv.Add( new Vector2( u, v ) );

        u = CalculateDistanceFromPointToLine( up5[ up5.Count - 1 ], groundLimitPoints0[ 0 ], Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * switchDir.normalized ) / textureTilting.x;
        v = CalculateDistanceFromPointToLine( up5[ up5.Count - 1 ], groundLimitPoints0[ 0 ], switchDir.normalized ) / textureTilting.y;
        uv.Add( new Vector2( u, v ) );

        /////////////////////

        groundMesh.vertices = vertices.ToArray();
        groundMesh.triangles = triangles.ToArray();
        groundMesh.normals = normals.ToArray();
        groundMesh.uv = uv.ToArray();

        return groundMesh;
    }

    public static float CalculateDistanceFromPointToLine(Vector3 point, Vector3 linePoint, Vector3 lineDirection)
    {
        // Calcola il vettore dalla retta al punto
        Vector3 pointToLine = point - linePoint;

        // Calcola la distanza utilizzando la formula
        float distance = Vector3.Cross( pointToLine, lineDirection ).magnitude;

        return distance;
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

        Vector3 dir;

        for( int i = 0; i < curve.Count; i++ )
        {
            /*if( floorParabolic )
            {
                zHeightPrev = curve[ i - 1 ].z;
                zHeightNext = curve[ i + 1 ].z;
            }*/

            if( i == 0 ) { 
               dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( controlPoints[ 1 ].x, controlPoints[ 1 ].y, 0.0f/*zHeightNext*/ ) - new Vector3( curve[ 0 ].x, curve[ 0 ].y, 0.0f/*zHeightPrev*/ ) ).normalized;
            }
            else {
                dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ i ].x, curve[ i ].y, 0.0f/*zHeightPrev*/ ) - new Vector3( curve[ i - 1 ].x, curve[ i - 1 ].y, 0.0f/*zHeightNext*/ ) ).normalized;
            }

            leftL.Add( curve[ i ] + ( dir * ( ( centerWidth / 2 ) + sideWidth ) ) );
            //Debug.DrawRay( leftL[ i ], -Vector3.forward * 10, Color.yellow, 999 );
            leftLine.Add( curve[ i ] + ( dir * ( ( centerWidth / 2 ) + ( sideWidth / 2 ) ) ) );
            
            centerL.Add( curve[ i ] + ( dir * ( centerWidth / 2 ) ) );
            //Debug.DrawRay( centerL[ i ], -Vector3.forward * 10, Color.blue, 999 );
            centerR.Add( curve[ i ] - ( dir * ( centerWidth / 2 ) ) );
            //Debug.DrawRay( centerR[ i ], -Vector3.forward * 10, Color.yellow, 999 );

            rightLine.Add( curve[ i ] - ( dir * ( ( centerWidth / 2 ) + ( sideWidth / 2 ) ) ) );
            rightR.Add( curve[ i ] - ( dir * ( ( centerWidth / 2 ) + sideWidth ) ) );
            //Debug.DrawRay( rightR[ i ], -Vector3.forward * 10, Color.blue, 999 );

            railLeftL.Add( leftLine[ i ] + ( dir * ( railsWidth / 2 ) ) );
            //Debug.DrawRay( railLeftL[ i ], -Vector3.forward * 10, Color.red, 999 );
            railLeftR.Add( leftLine[ i ] - ( dir * ( railsWidth / 2 ) ) );
            //Debug.DrawRay( railLeftR[ i ], -Vector3.forward * 10, Color.green, 999 );
            railRightL.Add( rightLine[ i ] + ( dir * ( railsWidth / 2 ) ) );
            //Debug.DrawRay( railRightL[ i ], -Vector3.forward * 10, Color.red, 999 );
            railRightR.Add( rightLine[ i ] - ( dir * ( railsWidth / 2 ) ) );
            //Debug.DrawRay( railRightR[ i ], -Vector3.forward * 10, Color.green, 999 );
        }

        /*if( floorParabolic )
        {
            zHeightPrev = curve[ curve.Count - 2 ].z;
            zHeightNext = curve[ curve.Count - 1 ].z;
        }*/

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

        //Debug.DrawLine( rb0, rb1, Color.red, Mathf.Infinity );
        //Debug.DrawLine( rb1, rb2, Color.red, Mathf.Infinity );
        //Debug.DrawLine( rb2, rb3, Color.red, Mathf.Infinity );

        //Debug.DrawLine( lb0, lb1, Color.red, Mathf.Infinity );
        //Debug.DrawLine( lb1, lb2, Color.red, Mathf.Infinity );
        //Debug.DrawLine( lb2, lb3, Color.red, Mathf.Infinity );

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

        //Debug.DrawLine( rb0Inv, rb1Inv, Color.red, Mathf.Infinity );
        //Debug.DrawLine( rb1Inv, rb2Inv, Color.red, Mathf.Infinity );
        //Debug.DrawLine( rb2Inv, rb3Inv, Color.red, Mathf.Infinity );

        //Debug.DrawLine( lb0Inv, lb1Inv, Color.red, Mathf.Infinity );
        //Debug.DrawLine( lb1Inv, lb2Inv, Color.red, Mathf.Infinity );
        //Debug.DrawLine( lb2Inv, lb3Inv, Color.red, Mathf.Infinity );

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

    public static Floor CalculateMonodirectionalFloorMeshVertex( List<Vector3> curve, Vector3? startingDir, float floorWidth, bool floorParabolic, float railsWidth )
    {

        List<Vector3> rightPoints = new List<Vector3>();
        List<Vector3> leftPoints = new List<Vector3>();

        List<Vector3> railCenterR = new List<Vector3>();
        List<Vector3> railCenterL = new List<Vector3>();

        float zHeightPrev = 0.0f;
        float zHeightNext = 0.0f;

        /*if( floorParabolic )
        {
            zHeightPrev = curve[ 0 ].z;
            zHeightNext = curve[ 1 ].z;
        }*/

        //Vector3 dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3(curve[ 0 ].x, curve[ 0 ].y, zHeightPrev) - new Vector3(curve[ 1 ].x, curve[ 1 ].y, zHeightNext ) ).normalized * tunnelWidth;
        Vector3 dir;

        for( int i = 0; i < curve.Count; i++ )
        {
            /*if( floorParabolic )
            {
                zHeightPrev = curve[ i - 1 ].z;
                zHeightNext = curve[ i + 1 ].z;
            }*/

            if( i == 0 ) {
                if( startingDir == null ) {
                    dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ 1 ].x, curve[ 1 ].y, zHeightNext ) - new Vector3( curve[ 0 ].x, curve[ 0 ].y, zHeightPrev ) ).normalized;
                }
                else {
                    dir = ( Vector3 )startingDir;
                    dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * dir.normalized;
                }
            }
            else {
                dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ i ].x, curve[ i ].y, zHeightNext ) - new Vector3( curve[ i - 1 ].x, curve[ i - 1 ].y, zHeightPrev ) ).normalized;
            }

            leftPoints.Add( curve[ i ] + ( dir * ( floorWidth / 2 ) )  );
            //Debug.DrawRay( leftPoints[ i ], -Vector3.forward * 10, Color.yellow, 999 );
            rightPoints.Add(curve[ i ] - ( dir * ( floorWidth / 2 ) )  );
            //Debug.DrawRay( rightPoints[ i ], -Vector3.forward * 10, Color.blue, 999 );

            railCenterL.Add( curve[ i ] + ( dir * ( railsWidth / 2 ) ) );
            //Debug.DrawRay( railCenterL[ i ], -Vector3.forward * 10, Color.red, 999 );
            railCenterR.Add( curve[ i ] - ( dir * ( railsWidth / 2 ) ) );
            //Debug.DrawRay( railCenterR[ i ], -Vector3.forward * 10, Color.green, 999 );
        }

        Floor singleFloor = new Floor();
        singleFloor.centerL = leftPoints;
        singleFloor.centerLine = curve;
        singleFloor.centerR = rightPoints;
        singleFloor.railCenterL = railCenterL;
        singleFloor.railCenterR = railCenterR;

        return singleFloor;
    }

    public static SpecularBaseLine CalculateBaseLinesFromCurve( List<Vector3> curve, Vector3? startingDir, float distance, float angle ) {

        SpecularBaseLine lines = new SpecularBaseLine();
        lines.left = new List<Vector3>();
        lines.right = new List<Vector3>();

        Vector3 curveDir = ( curve[ 1 ] - curve[ 0 ] );
        if( startingDir != null ) {
            curveDir = ( Vector3 )startingDir;
        }
        curveDir = curveDir.normalized;

        Vector3 leftDir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * curveDir * distance;
        Vector3 rightDir = -leftDir;

        leftDir = Quaternion.AngleAxis( -angle, curveDir ) * leftDir;
        rightDir = Quaternion.AngleAxis( angle, curveDir ) * rightDir;

        lines.left.Add( curve[ 0 ] + leftDir );
        lines.right.Add( curve[ 0 ] + rightDir );

        for( int i = 1; i < curve.Count; i++ )
        {

            curveDir = ( curve[ i ] - curve[ i - 1 ] ).normalized;
            leftDir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * curveDir * distance;
            rightDir = -leftDir;

            leftDir = Quaternion.AngleAxis( -angle, curveDir ) * leftDir;
            rightDir = Quaternion.AngleAxis( angle, curveDir ) * rightDir;

            lines.left.Add( curve[ i ] + leftDir );
            lines.right.Add( curve[ i ] + rightDir );

            Debug.DrawRay( lines.left[ i ], lines.left[ i - 1 ] - lines.left[ i ], Color.yellow, 999 );
        }

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

            //Debug.DrawRay( curve[ 0 ], curveDir * 10, Color.cyan, 999 );

            leftDir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ i - 1 ].x, curve[ i - 1 ].y, zHeightPrev ) - new Vector3( curve[ i + 1 ].x, curve[ i + 1 ].y, zHeightNext ) ).normalized * dirLength;
            leftDir = Quaternion.AngleAxis( alpha, curveDir ) * leftDir;
            rightDir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ i - 1 ].x, curve[ i - 1 ].y, zHeightPrev ) - new Vector3( curve[ i + 1 ].x, curve[ i + 1 ].y, zHeightNext ) ).normalized * dirLength;
            rightDir = Quaternion.AngleAxis( -alpha, curveDir ) * rightDir;

            //Debug.DrawRay( curve[ i ], leftDir, Color.green, 999 );
            //Debug.DrawRay( curve[ i ], -rightDir, Color.red, 999 );

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

    public static PlatformSide CalculatePlatformSidesMeshesVertex( List<Vector3> curve, Vector3? startingDir, float floorWidth, bool floorParabolic, float sideHeight, float sideWidth )
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

        Vector3 dir;

        for( int i = 0; i < curve.Count; i++ )
        {

            if( i == 0 ) {
                if( startingDir == null ) {
                    dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ 1 ].x, curve[ 1 ].y, zHeightNext ) - new Vector3( curve[ 0 ].x, curve[ 0 ].y, zHeightPrev ) ).normalized;
                }
                else {
                    dir = ( Vector3 )startingDir;
                    dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * dir.normalized;
                }
            }
            else {
                dir = Quaternion.Euler( 0.0f, 0.0f, 90.0f ) * ( new Vector3( curve[ i ].x, curve[ i ].y, zHeightNext ) - new Vector3( curve[ i - 1 ].x, curve[ i - 1 ].y, zHeightPrev ) ).normalized;
            }

            platformSide.leftDown.Add( curve[ i ] + ( dir * ( floorWidth / 2 ) ) );
            platformSide.rightDown.Add(curve[ i ] - ( dir * ( floorWidth / 2 ) ) );

            platformSide.leftUp.Add( new Vector3( platformSide.leftDown[ i ].x, platformSide.leftDown[ i ].y, platformSide.leftDown[ i ].z - sideHeight ) ); 
            platformSide.leftFloorRight.Add( platformSide.leftUp[ i ] );
            platformSide.leftFloorLeft.Add( platformSide.leftUp[ i ] + ( dir * sideWidth ) );

            platformSide.rightUp.Add( new Vector3( platformSide.rightDown[ i ].x, platformSide.rightDown[ i ].y, platformSide.rightDown[ i ].z - sideHeight ) );
            platformSide.rightFloorLeft.Add( platformSide.rightUp[ i ] );
            platformSide.rightFloorRight.Add( platformSide.rightUp[ i ] - ( dir * sideWidth ) );
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

    public static ExtrudedMesh GenerateExtrudedMesh( List<Vector3> profileVertices, float profileScale, List<Vector3> previousProfileVertices, List<Vector3> baseVertices, float horPosCorrection, bool clockwiseRotation, bool closeMesh, float textureHorLenght, float textureVertLenght, float verticalRotationCorrection, float smoothFactor ) {
        
        if( closeMesh ) {
            baseVertices.Add( baseVertices[ 0 ] );
        }
        
        ExtrudedMesh extrudedMesh = new ExtrudedMesh();

        List<Vector3> lastProfileVertices = new List<Vector3>();

        List<Vector3> profileVerticesScaled = new List<Vector3>();
        foreach( Vector3 profileVertex in profileVertices ) {
            profileVerticesScaled.Add( profileVertex * profileScale );
        }
        
        Mesh mesh = new Mesh();
        mesh.name = "Procedural Mesh";

        int h = profileVerticesScaled.Count;
        int trianglesCounter = ( h - 1 ) * ( baseVertices.Count - 1 ) * 6;
        int verticesCounter = profileVerticesScaled.Count * ( baseVertices.Count );

        int[] triangles = new int[ trianglesCounter ];
        Vector3[] vertices = new Vector3[ verticesCounter ];
        Vector2[] uv = new Vector2[ verticesCounter ];
        Vector3[] normals = new Vector3[ verticesCounter ];

        // Array di supporto con le distanze dei punti del profilo e della base calcolate rispetto allo zero (serve per gestire l'UV mapping ripetuto)
        List<float> distancesHor = new List<float>();
        distancesHor.Add( 0.0f );
        List<float> distancesVert = new List<float>();
        distancesVert.Add( 0.0f );

        List<Vector3> profileDirs = new List<Vector3>();
        for( int i = 1; i < h; i++ ) {

            Vector3 profileDir = profileVerticesScaled[ i ] - profileVerticesScaled[ i - 1 ];
            profileDirs.Add( profileDir );

            distancesVert.Add( distancesVert[ i - 1 ] + profileDir.magnitude );
        }

        List<Vector3> baseDirs = new List<Vector3>();
        List<float>baseAlphas = new List<float>();
        for( int i = 1; i < baseVertices.Count; i++ ) { 
            
            Vector3 baseDir = Vector3.zero;
            if( i == baseVertices.Count - 1 && closeMesh ) {
                baseDir = baseVertices[ 0 ] - baseVertices[ i - 1 ];
            }
            else {
                baseDir = baseVertices[ i ] - baseVertices[ i - 1 ];
            }
            baseDirs.Add( baseDir );

            //Debug.DrawRay( baseVertices[ i - 1 ], baseDirs[ i - 1 ], Color.red, 999 );
            float baseAlpha = DLL_MathExt.Angles.SignedAngleTo360Angle( Vector3.SignedAngle( Vector3.right, baseDir, Vector3.forward ) );
            baseAlphas.Add( baseAlpha );
            //baseAlphas[ i - 1 ] += baseAlphas[ i - 1 ] > 180.0f ? 90.0f : 0.0f;

            //Debug.Log( "baseAlpha[ " + ( i - 1 ) + "]: " + baseAlphas[ i - 1 ] );

            distancesHor.Add( distancesHor[ i - 1 ] + baseDir.magnitude );
        }
        if( closeMesh ) { 
            baseAlphas.Add( baseAlphas[ 0 ] );
        }
        else {
            baseAlphas.Add( baseAlphas[ baseAlphas.Count - 1 ] );
        }
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

                alpha += deltaAlphaAbs * smoothFactor * ( clockwiseRotation ? -1 : 1 );
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
            if( !clockwiseRotation ) {
                normal = -normal;
            }
            normals[ i * h ] = normal;

            //float beta = Vector3.SignedAngle( Vector3.right, baseDirs[ normalHorIndex ], Vector3.up );
            //Debug.Log( ">>> beta: " + beta );

            for( int j = 0; j < profileDirs.Count; j++ ) {

                if( i == 0 && previousProfileVertices != null && previousProfileVertices.Count == profileVertices.Count ) {
                    vertices[ j + 1 ] = previousProfileVertices [ j + 1 ];
                }
                else {
                    //Debug.Log( ">>> i: " + i );
                    //Debug.Log( ">>> j: " + j );
                    vertices[ ( i * h ) + j + 1 ] = ( vertices[ ( i * h ) + j ] + Quaternion.Euler( 0.0f, /*beta*/0.0f, alpha + verticalRotationCorrection ) * profileDirs[ j ] );

                    if( i == baseVertices.Count - 1 ) {
                        lastProfileVertices.Add( vertices[ ( i * h ) + j + 1 ] );
                    }
                }

                //Debug.DrawLine( vertices[ ( i * h ) + j + 1 ], baseVertices[ i ], Color.cyan, 999 ); 

                float v = distancesVert[ j + 1 ] / textureVertLenght;
                uv[ ( i * h ) + j + 1 ] = new Vector2( u, v );

                int normalVertIndex = ( j + 1 ) < profileDirs.Count ? ( j + 1 ) : ( profileDirs.Count - 1 );
                normal = Vector3.Cross( Quaternion.Euler( 0.0f, /*beta*/0.0f, alpha + verticalRotationCorrection ) * profileDirs[ normalVertIndex ], baseDirs[ normalHorIndex ] ).normalized;
                if( !clockwiseRotation ) {
                    normal = -normal;
                }
                normals[ ( i * h ) + j + 1 ] = normal;
                
                //Debug.DrawRay( vertices[ ( i * h ) + j + 1 ], normals[ ( i * h ) + j + 1 ] * 0.5f, Color.red, 9999 );

                if( i > 0 ) {
                    int arrayIndex = ( 6 * ( ( ( i - 1 ) * ( h - 1 ) ) + j ) );
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

        // if( closeMesh ) {
            
        //     int trianglesOffset = ( h - 1 ) * ( baseVertices.Count - 1 ) * 6;

        //     for( int k = 0; k < profileDirs.Count - 1; k++ ) {
                
        //         triangles[ trianglesOffset + ( k * 6 ) + 0 ] = triangles[ trianglesOffset + ( k * 6 ) + 3 ] = k;

        //         if( !clockwiseRotation ) {

        //             triangles[ trianglesOffset + ( k * 6 ) + 1 ] = triangles[ trianglesOffset + ( k * 6 ) + 5 ] = verticesCounter - profileDirs.Count + k/* + ( h + 1 )*/; 
        //             triangles[ trianglesOffset + ( k * 6 ) + 2 ] = verticesCounter - profileDirs.Count + k/* + h*/;
        //             triangles[ trianglesOffset + ( k * 6 ) + 4 ] = k + 1;
        //         }
        //         else {
        //             triangles[ trianglesOffset + ( k * 6 ) + 1 ] = verticesCounter - profileDirs.Count + k/* + h*/;
        //             triangles[ trianglesOffset + ( k * 6 ) + 2 ] = triangles[ trianglesOffset + ( k * 6 ) + 4 ] = verticesCounter - profileDirs.Count + k/* + ( h + 1 )*/; 
        //             triangles[ trianglesOffset + ( k * 6 ) + 5 ] = k + 1;
        //         }
        //     }
        // }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.normals = normals;

        extrudedMesh.mesh = mesh;
        extrudedMesh.lastProfileVertex = lastProfileVertices;

        return extrudedMesh;
    }

    public static Mesh GeneratePlanarMesh( List<Vector3> line, Vector3[ , ] vertMatrix, bool clockwiseRotation,  bool closeMesh, bool centerTexture, float textureHorLenght, float textureVertLenght )
    {
        // In questo modo anche se aggiungo un punto alla lista è stata copiata per valore e non per riferimento, quindi la
        // modifica non ha effetti fuori dal metodo.
        List<Vector3> curve = new List<Vector3>( line );

        if( closeMesh ) {
            List<Vector3> up = new List<Vector3>();
            List<Vector3> down = new List<Vector3>();
            for( int i = 0; i < curve.Count; i++ ) {
                up.Add( vertMatrix[ 0, i ] );
                down.Add( vertMatrix[ 1, i ] );
            }
            up.Add( vertMatrix[ 0, 0 ] );
            down.Add( vertMatrix[ 1, 0 ] );

            vertMatrix = ConvertListsToMatrix_2xM( up, down ); 

            curve.Add( curve[ 0 ] );
        }

        Mesh floorMesh = new Mesh();

        Vector3[] vertices = new Vector3[ curve.Count * 2 ];
        int[] edges = new int[( curve.Count - 1 ) * 6 ];
        Vector2[] uvs = new Vector2[ vertices.Length ];
        float u = 0.0f, vMin = 0.0f, vMax = 0.0f;
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
                u += ( float )( ( curve[ i ] - curve[ i - 1 ] ).magnitude / textureHorLenght );
            }
            vMax = ( float )( ( vertMatrix[ 1, i ] - vertMatrix[ 0, i ] ).magnitude / textureVertLenght );

            if( centerTexture ){
                vMin = -( vMax / 2 );
                vMax = -vMin;
            }

            uvs[ ( i * 2 ) ] = new Vector2( u, vMin );
            uvs[ ( i * 2 ) + 1 ] = new Vector2( u, vMax );

            if( i < curve.Count - 1 )
            {
                if( clockwiseRotation ) {
                    edges[ ( i * 6 ) + 0 ] = ( i * 2 );
                    edges[ ( i * 6 ) + 1 ] = edges[ ( i * 6 ) + 4 ] = ( i * 2 ) + 1;
                    edges[ ( i * 6 ) + 2 ] = edges[ ( i * 6 ) + 3 ] = ( i * 2 ) + 2;
                    edges[ ( i * 6 ) + 5 ] = ( i * 2 ) + 3;
                }
                else {
                    edges[ ( i * 6 ) + 0 ] = ( i * 2 );
                    edges[ ( i * 6 ) + 1 ] = edges[ ( i * 6 ) + 4 ] = ( i * 2 ) + 2;
                    edges[ ( i * 6 ) + 2 ] = edges[ ( i * 6 ) + 3 ] = ( i * 2 ) + 1;
                    edges[ ( i * 6 ) + 5 ] = ( i * 2 ) + 3;
                }
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
