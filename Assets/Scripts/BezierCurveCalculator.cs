using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DLL_MathExt;

public static class BezierCurveCalculator
{
    public static Vector3 CalculateNPoint(float t, List<Transform> controlPoints)
    {

        Vector3 bezierPoint = Vector3.zero;
        int n = controlPoints.Count;

        for(int i = 0; i < n; i++)
        {
            long binCoeff = binomialCoeffFactorial2(n, i);

            bezierPoint += binCoeff * controlPoints[i].position * (Mathf.Pow((1 - t), ((n - 1) - i)) * Mathf.Pow(t, i));
        }

        return bezierPoint;
    }

    public static Vector3 CalculateSingleBezierPoint( float t, List<Vector3> controlPoints )
    {
        Vector3 bezierPoint = Vector3.zero;
        int n = controlPoints.Count;

        for( int i = 0; i < n; i++ )
        {
            long binCoeff = binomialCoeffFactorial2( n, i );

            bezierPoint += binCoeff * controlPoints[ i ] * ( Mathf.Pow( ( 1 - t ), ( ( n - 1 ) - i ) ) * Mathf.Pow( t, i ) );
        }

        return bezierPoint;
    }

    public static float CalculateBezierCurveLenght( List<Vector3> baseBezierCurve )
    {
        float distance = 0.0f;

        for( int i = 0; i < baseBezierCurve.Count - 1; i++ )
        {
            distance = Vector3.Distance( baseBezierCurve[ i ], baseBezierCurve[ i + 1 ] );
        }

        return distance;
    }

    public static List<Vector3> RecalcultateCurveWithFixedLenght( List<Vector3> baseBezierCurve, float fixedCurvePointsNumber )
    {
        List<Vector3> fixedLenghtCurve = new List<Vector3>();

        float baseBezierCurveLenght = CalculateBezierCurveLenght( baseBezierCurve );
        float fixedLenght = baseBezierCurveLenght / fixedCurvePointsNumber;

        fixedLenghtCurve.Add( baseBezierCurve[ 0 ] );

        int i = 0;
        int k = 1;

        while( k < baseBezierCurve.Count )
        {
            if( ( baseBezierCurve[ k ] - fixedLenghtCurve[ i ] ).magnitude < fixedLenght )
            {
                k++;
            }
            else
            {
                fixedLenghtCurve.Add( fixedLenghtCurve[ i ] + ( ( baseBezierCurve[ k ] - fixedLenghtCurve[ i ] ).normalized * fixedLenght ) );
                i++;
            }
        }

        return fixedLenghtCurve;

    }

    /*public static List<Vector3> RecalcultateCurveWithFixedLenght( List<Vector3> baseBezierCurve, float fixedLenght )
    {
        List<Vector3> fixedLenghtCurve = new List<Vector3>();

        fixedLenghtCurve.Add( baseBezierCurve[ 0 ] );

        int i = 0;
        int k = 1;

        while( k < baseBezierCurve.Count )
        {
            if( ( baseBezierCurve[ k ] - fixedLenghtCurve[ i ] ).magnitude < fixedLenght )
            {
                k++;
            }
            else
            {
                fixedLenghtCurve.Add( fixedLenghtCurve[ i ] + ( ( baseBezierCurve[ k ] - fixedLenghtCurve[ i ] ).normalized * fixedLenght ) );
                i++;
            }
        }

        return fixedLenghtCurve;

    }*/

    /*public static List<Vector3> RecalcultateCurveWithLimitedAngle( List<Vector3> baseBezierCurve, float angleLimit )
    { 
        List<Vector3> limitedAngleCurve = new List<Vector3>();

        limitedAngleCurve.Add( baseBezierCurve[ 0 ] );
        limitedAngleCurve.Add( baseBezierCurve[ 1 ] );

        float alpha;

        Vector3 prevDir;
        Vector3 nextDir;

        int i = 1;
        while( i < baseBezierCurve.Count - 1 ) { 
            prevDir = limitedAngleCurve[ i ] - limitedAngleCurve[ i - 1 ];
            nextDir = baseBezierCurve[ i + 1 ] - limitedAngleCurve[ i ];

            alpha = Vector3.SignedAngle( prevDir, nextDir, Vector3.forward );

            Debug.Log( "Alpha: " + alpha + " angleLimit:" + angleLimit );

            if( Mathf.Abs( alpha ) <= angleLimit ) {
                limitedAngleCurve.Add( limitedAngleCurve[ i ] + nextDir.normalized * ( baseBezierCurve[ i + 1 ] - baseBezierCurve[ i ] ).magnitude );
            }
            else if( alpha < 0 ) { // Curva a destra
                limitedAngleCurve.Add( limitedAngleCurve[ i ] + ( Quaternion.Euler( 0.0f, 0.0f, -angleLimit ) * nextDir.normalized * ( baseBezierCurve[ i + 1 ] - baseBezierCurve[ i ] ).magnitude ) );
            }
            else if( alpha > 0 ) { // Curva a sinistra
                limitedAngleCurve.Add( limitedAngleCurve[ i ] + ( Quaternion.Euler( 0.0f, 0.0f, angleLimit ) * nextDir.normalized * ( baseBezierCurve[ i + 1 ] - baseBezierCurve[ i ] ).magnitude ) );
            }

            i++;
        }

        return limitedAngleCurve;

    }*/

    public static List<Vector3> RecalcultateCurveWithLimitedAngle( List<Vector3> baseBezierCurve, float angleLimit )
    {
        List<Vector3> limitedAngleCurve = new List<Vector3>();

        limitedAngleCurve.Add( baseBezierCurve[ 0 ] );
        limitedAngleCurve.Add( baseBezierCurve[ 1 ] );

        int i = 1;

        float L1, L2;
        Vector3 vectL1, vectL2;

        float alpha = angleLimit;
        //float beta;
        //float gamma;

        Vector3 prevDir;
        Vector3 nextDir;

        while( i < baseBezierCurve.Count - 1 ) {

            prevDir = limitedAngleCurve[ i ] - limitedAngleCurve[ i - 1 ];
                
            //gamma = Vector3.SignedAngle( Vector3.right, prevDir, Vector3.forward );

            vectL1 = Quaternion.Euler( 0.0f, 0.0f, alpha ) * prevDir.normalized;
            L1 = Vector3.SignedAngle( Vector3.right, vectL1, Vector3.forward );
            vectL2 = Quaternion.Euler( 0.0f, 0.0f, -alpha ) * prevDir.normalized;
            L2 = Vector3.SignedAngle( Vector3.right, vectL2, Vector3.forward );

            Debug.DrawRay( limitedAngleCurve[ i ], vectL1, Color.blue, 99999.0f );
            Debug.DrawRay( limitedAngleCurve[ i ], vectL2, Color.blue, 99999.0f );

            // TODO: rivedere meglio questa zona, l'angolo +-180 fa divergere la curva quando curva verso l'alto a sinistra o verso il basso a sinistra
            nextDir = baseBezierCurve[ i + 1 ] - limitedAngleCurve[ i ];
            float l1NextDir = Vector3.SignedAngle( vectL1, nextDir, Vector3.forward );
            float l2NextDir = Vector3.SignedAngle( vectL2, nextDir, Vector3.forward );
            float prevDirNextDir = Vector3.SignedAngle( prevDir, nextDir, Vector3.forward );

            if( l1NextDir <= 0.0f && l2NextDir >= 0.0f ) {
                limitedAngleCurve.Add( limitedAngleCurve[ i ] + nextDir.normalized * ( baseBezierCurve[ i + 1 ] - baseBezierCurve[ i ] ).magnitude );
            }
            else if( l1NextDir >= 0.0f && l2NextDir >= 0.0f ) {
                limitedAngleCurve.Add( limitedAngleCurve[ i ] + ( vectL1 * ( baseBezierCurve[ i + 1 ] - baseBezierCurve[ i ] ).magnitude ) );
            }
            else if( l1NextDir <= 0.0f && l2NextDir <= 0.0f ) {
                limitedAngleCurve.Add( limitedAngleCurve[ i ] + ( vectL2 * ( baseBezierCurve[ i + 1 ] - baseBezierCurve[ i ] ).magnitude ) );
            }
            else if( l1NextDir >= 0.0f && l2NextDir <= 0.0f ) {
                if( prevDirNextDir >= 0 ) {
                    limitedAngleCurve.Add( limitedAngleCurve[ i ] + ( vectL2 * ( baseBezierCurve[ i + 1 ] - baseBezierCurve[ i ] ).magnitude ) );
                }
                else {
                    limitedAngleCurve.Add( limitedAngleCurve[ i ] + ( vectL1 * ( baseBezierCurve[ i + 1 ] - baseBezierCurve[ i ] ).magnitude ) );
                }
            }

            i++;
        }

        return limitedAngleCurve;  
    }

    public static long binomialCoeffFactorial( int n, int k )
    {
        return ( long )Factorial( n - 1 ) / ( Factorial( k ) * Factorial( ( n - 1 ) - k ) );
    }

    public static long binomialCoeffFactorial2( int n, int k )
    {
        return binomialCoeffFactorial( n - 1, k ) + binomialCoeffFactorial( n - 1, k - 1 );
    }

    private static int Factorial( int number )
    {
        int factNumber = 1;
        for( int i = 1; i <= number; i++ )
        {
            factNumber *= i;
        }

        return factNumber;
    }
}
