using UnityEngine;

public class VertexPoint : MonoBehaviour
{
    public int vertexIndex { get; private set; }
    
    public void Initialize(int index, Vector3 position)
    {
        vertexIndex = index;
        transform.position = position;
    }
}