using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    [ System.Serializable ] 
    public class Shape {
        public string name;
        public List<Vector3> partialProfile;
        public Vector2 positionCorrection;
        public Material material;
        public Vector2 tiling;
        public float smoothFactor = 0.5f;
        public float scale = 1.0f;
    }
}
