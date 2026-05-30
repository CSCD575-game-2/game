using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Transform target;

    [Header("Offset")]
    [SerializeField] private float distance = 30f;
    [SerializeField] private float height = 10f;

    [Header("Smoothing")]
    [SerializeField] private float followSpeed = 6f;
    [SerializeField] private float rotationSpeed = 8f;

    private Vector3 lastGoodUp = Vector3.up;

    private void LateUpdate()
    {
        //if (target == null) return;

        //Vector3 travelAxis = target.forward.normalized;

        //// Find an "up" direction perpendicular to travel.
        //Vector3 cameraUp = Vector3.ProjectOnPlane(Vector3.up, travelAxis);

        //// If traveling straight up/down, world-up cannot define camera-up.
        //// Fall back to the ship's own up direction.
        //if (cameraUp.sqrMagnitude < 0.001f)
        //{
            //cameraUp = Vector3.ProjectOnPlane(target.up, travelAxis);
        //}

        //if (cameraUp.sqrMagnitude > 0.001f)
        //{
            //lastGoodUp = cameraUp.normalized;
        //}

        //Vector3 desiredPosition =
            //target.position
            //- travelAxis * distance
            //+ lastGoodUp * height;

        //transform.position = Vector3.Lerp(
            //transform.position,
            //desiredPosition,
            //followSpeed * Time.deltaTime
        //);

        //Quaternion desiredRotation = Quaternion.LookRotation(
            //target.position - transform.position,
            //lastGoodUp
        //);

        //transform.rotation = Quaternion.Slerp(
            //transform.rotation,
            //desiredRotation,
            //rotationSpeed * Time.deltaTime
        //);
    }
}
