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

    public static List<Vector3> RecalcultateCurveWithLimitedAngle( List<Vector3> baseBezierCurve, float angleLimit )
    {
        // I primi due punti della curva combaciano sempre, perché mi servono per calcolare la prima prevDir
        List<Vector3> limitedAngleCurve = new List<Vector3>();
        limitedAngleCurve.Add( baseBezierCurve[ 0 ] );
        limitedAngleCurve.Add( baseBezierCurve[ 1 ] );

        Vector3 prevDir, nextDir, leftLimitDir, rightLimitDir;

        float alpha = angleLimit;
        float beta;

        int i = 1;
        while( i < baseBezierCurve.Count - 1 ) {

            prevDir = limitedAngleCurve[ i ] - limitedAngleCurve[ i - 1 ];
            nextDir = baseBezierCurve[ i + 1 ] - limitedAngleCurve[ i ];

            // Se l'angolo fra la prevDir e la newDir è minore o uguale al limite imposto, allora la
            // nuova curva deve "andare" verso la curva originale
            beta = Vector3.SignedAngle( prevDir, nextDir, Vector3.forward );
            if( Mathf.Abs( beta ) <= alpha ) {
                limitedAngleCurve.Add( limitedAngleCurve[ i ] + nextDir.normalized * ( baseBezierCurve[ i + 1 ] - baseBezierCurve[ i ] ).magnitude );
            }
            // Altrimenti continuo a costeggiarla a seconda che la curva originale vada a destra o a sinistra
            else if( beta > 0 ) {
                leftLimitDir = Quaternion.Euler( 0.0f, 0.0f, alpha ) * prevDir.normalized;
                limitedAngleCurve.Add( limitedAngleCurve[ i ] + ( leftLimitDir * ( baseBezierCurve[ i + 1 ] - baseBezierCurve[ i ] ).magnitude ) );
            }
            else if( beta < 0 ) {
                rightLimitDir = Quaternion.Euler( 0.0f, 0.0f, -alpha ) * prevDir.normalized;
                limitedAngleCurve.Add( limitedAngleCurve[ i ] + ( rightLimitDir * ( baseBezierCurve[ i + 1 ] - baseBezierCurve[ i ] ).magnitude ) );
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
