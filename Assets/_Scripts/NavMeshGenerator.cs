using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class NavMeshGenerator : MonoBehaviour
{
    public void GenerateNavMesh() {

        this.gameObject.GetComponent<NavMeshSurface>().BuildNavMesh();
        
    }
}
