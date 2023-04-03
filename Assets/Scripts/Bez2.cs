using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bez2: MonoBehaviour
{
    public LineRenderer curve_Quadratic;
    public int curve_BezierPointsNumber = 50;
    public List<Transform> curve_ControlPoints;


    public Vector3[] curve_Points;
    void Start()
    {
        if(curve_Quadratic == null)
        {
            curve_Quadratic = new LineRenderer();
        }
        curve_Quadratic.positionCount = curve_BezierPointsNumber;

        curve_Points = new Vector3[curve_BezierPointsNumber];

        //DrawBezierCurve();
    }

    void Update()
    {
        DrawBezierCurve();
    }

    private void DrawBezierCurve()
    {
        for(int k = 0; k < curve_BezierPointsNumber; k++)
        {
            float t = k / (float)(curve_BezierPointsNumber - 1);
            //curve_Points[i] = CalculateQuadraticBezierPoint(t, curve_ControlPoints);
            curve_Points[k] = CalculateNBezierPoint(t, curve_ControlPoints);
        }

        curve_Quadratic.SetPositions(curve_Points);
    }

    private Vector3 CalculateQuadraticBezierPoint(float t, List<Transform> points)
    {
        //B(t) = (1 - t)² * P0 + [2 * t * (1 - t)] * P1 + t² * P2

        return (Mathf.Pow((1 - t), 2) * points[0].position) + (2 * t * (1 - t) * points[1].position) + (Mathf.Pow(t, 2) * points[2].position);

    }

    private Vector3 CalculateNBezierPoint(float t, List<Transform> controlPoints)
    {

        Vector3 bezierPoint = Vector3.zero;
        int n = controlPoints.Count;

        for(int i = 1; i <= n; i++)
        {

            float coeff = Factorial(n) / (float)(Factorial(i) * Factorial(n - i));
            bezierPoint += coeff * controlPoints[i - 1].position * (Mathf.Pow((1 - t), (n - i)) * Mathf.Pow(t, i));
        }

        //Debug.Log(bezierPoint);

        return bezierPoint /*+ (controlPoints[n - 1].position * t)*/;
    }

    private int Factorial(int number)
    {
        int factNumber = 1;
        for(int i = 1; i <= number; i++)
        {
            factNumber *= i;
        }

        return factNumber;
    }

    
}