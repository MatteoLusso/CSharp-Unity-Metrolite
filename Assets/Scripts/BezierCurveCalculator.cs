using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DLL_MathExt;

public static class BezierCurveCalculator
{
    public static List<Vector3> CalculateBezierCurve( List<Vector3> controlsPoints, int curvePointsNumber )
    {
        List<Vector3> curve = new List<Vector3>();

        for(int k = 0; k < curvePointsNumber; k++)
        {
            float t = k / ( float )( curvePointsNumber - 1 );
            Vector3 newCurvePoint = CalculateSingleBezierPoint( t, controlsPoints );
            curve.Add( newCurvePoint );
        }

        return curve;
    }

    public static Vector3 CalculateNPoint(float t, List<Transform> controlPoints)
    {

        Vector3 bezierPoint = Vector3.zero;
        int n = controlPoints.Count;

        for(int i = 0; i < n; i++)
        {
            long binCoeff = binomialCoeffFactorial2( n, i );

            bezierPoint += binCoeff * controlPoints[ i ].position * ( Mathf.Pow( ( 1 - t ), ( ( n - 1 ) - i ) ) * Mathf.Pow( t, i ) );
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

    public static float CalculateCurveLenght( List<Vector3> curve )
    {
        float distance = 0.0f;

        for( int i = 0; i < curve.Count - 1; i++ )
        {
            // ERRORE, per ora lascio così per altri test
            distance += Vector3.Distance( curve[ i ], curve[ i + 1 ] );
        }

        return distance;
    }

    public static List<Vector3> RecalcultateCurveWithFixedLenght( List<Vector3> baseCurve, float fixedCurvePointsNumber )
    {
        List<Vector3> fixedLenghtCurve = new List<Vector3>();

        float baseBezierCurveLenght = CalculateCurveLenght( baseCurve );
        float fixedLenght = baseBezierCurveLenght / fixedCurvePointsNumber;

        fixedLenghtCurve.Add( baseCurve[ 0 ] );

        int i = 0;
        int j = 1;

        while( j < baseCurve.Count )
        {
            if( ( baseCurve[ j ] - fixedLenghtCurve[ i ] ).magnitude < fixedLenght )
            {
                j++;
            }
            else
            {
                fixedLenghtCurve.Add( fixedLenghtCurve[ i ] + ( ( baseCurve[ j ] - fixedLenghtCurve[ i ] ).normalized * fixedLenght ) );
                i++;
            }
        }

        return fixedLenghtCurve;

    }

    public static List<Vector3> RecalcultateCurveWithLimitedAngle( List<Vector3> baseCurve, float angleLimit, Vector3 startingDir )
    {
        List<Vector3> limitedAngleCurve = new List<Vector3>();
        int i;
        limitedAngleCurve.Add( baseCurve[ 0 ] );
        if( startingDir == Vector3.zero ) {
            limitedAngleCurve.Add( baseCurve[ 1 ] );
            i = 1;
        }
        else {
            i = 0;
        }

        Vector3 prevDir, nextDir, leftLimitDir, rightLimitDir;

        float alpha = angleLimit;
        float beta;

        while( i < baseCurve.Count - 1 ) {
            if( i == 0 ) {
                prevDir = startingDir;
            }
            else {
                prevDir = limitedAngleCurve[ i ] - limitedAngleCurve[ i - 1 ];
            }

            bool inRange = false;

            for( int j = i; j < baseCurve.Count - 1; j++ ) {
                nextDir = baseCurve[ j + 1 ] - limitedAngleCurve[ i ];
                beta = Vector3.SignedAngle( prevDir, nextDir, Vector3.forward );
                
                if( Mathf.Abs( beta ) <= alpha ) {
                    limitedAngleCurve.Add( limitedAngleCurve[ i ] + nextDir.normalized * ( baseCurve[ j + 1 ] - baseCurve[ j ] ).magnitude );
                    inRange = true;
                    break;
                }
            }

            if( !inRange ) {
                nextDir = baseCurve[ i + 1 ] - limitedAngleCurve[ i ];
                beta = Vector3.SignedAngle( prevDir, nextDir, Vector3.forward );

                if( beta > 0 ) {
                    leftLimitDir = Quaternion.Euler( 0.0f, 0.0f, alpha ) * prevDir.normalized;
                    limitedAngleCurve.Add( limitedAngleCurve[ i ] + ( leftLimitDir * ( baseCurve[ i + 1 ] - baseCurve[ i ] ).magnitude ) );
                }
                else if( beta < 0 ) {
                    rightLimitDir = Quaternion.Euler( 0.0f, 0.0f, -alpha ) * prevDir.normalized;
                    limitedAngleCurve.Add( limitedAngleCurve[ i ] + ( rightLimitDir * ( baseCurve[ i + 1 ] - baseCurve[ i ] ).magnitude ) );
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
