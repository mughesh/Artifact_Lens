using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Camera mainCamera;
    [SerializeField] private float rotationSpeed = 10f;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (mainCamera == null) return;

        Vector3 directionToCamera = mainCamera.transform.position - transform.position;
        directionToCamera.y = 0; // Lock Y axis

        if (directionToCamera != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-directionToCamera);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.Euler(0, targetRotation.eulerAngles.y, 0),
                rotationSpeed * Time.deltaTime
            );
        }
    }
}