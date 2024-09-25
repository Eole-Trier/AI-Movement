using UnityEngine;
using static UnityEditor.ShaderData;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class Movement : MonoBehaviour {
    [SerializeField]
    private float MaxSpeed = 10f;
    [SerializeField]
    private float MaxForce = 0.05f;
    [SerializeField]
    private float Mass = 1f;
    [SerializeField]
    private float SlowingRadius = 1f;

    private Vector3 velocity = Vector3.zero;
    private float rotation = 0f;

    private Vector3 desiredVelocity = Vector3.zero;
    private Vector3 steering = Vector3.zero;

    private float distance = 0f;

    private float posOffsetY = 0.5f;
    private Vector3 targetPos;
    public Vector3 TargetPos
    {
        get { return targetPos; }
        set { targetPos = value; targetPos.y += posOffsetY; }
    }

    private void Start()
    {
        targetPos = transform.position;
    }

	public void Stop()
	{
		velocity = Vector3.zero;
	}

    protected float GetOrientationFromDirection(Vector3 direction)
    {
        if (direction.magnitude > 0)
        {
            return Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        }
        return rotation;
    }

    // Update is called once per frame
    private void Update ()
	{
        desiredVelocity = (targetPos - transform.position);

        distance = desiredVelocity.magnitude;
        // Check the distance to detect whether the character 
        // is inside the slowing area 
        if (distance < SlowingRadius)
        {
            // Inside the slowing area 
            desiredVelocity = (desiredVelocity.normalized * MaxSpeed * (distance / SlowingRadius));
        }
        else
        {
            // Outside the slowing area. 
            desiredVelocity = desiredVelocity.normalized * MaxSpeed;
        }
        // Set the steering based on this 
        steering = desiredVelocity - velocity;

        steering = steering.normalized * MaxForce;
        steering = steering / Mass;
        velocity += steering;

        rotation = GetOrientationFromDirection(velocity);

		// truncate to max speed
        if (velocity.magnitude > MaxSpeed)
		{
			velocity.Normalize();
            velocity *= MaxSpeed;
            // steering 
            
        }
        // If (velocity + steering) equals zero, then there is no movement 
       
        // Update position and rotation
        transform.position += velocity * Time.deltaTime;

		transform.eulerAngles = Vector3.up * rotation;

  //      // keep position above the floor
		//RaycastHit hitInfo = new RaycastHit();
  //      if (Physics.Raycast(transform.position + Vector3.up * 10, Vector3.down, out hitInfo, 100, 1 << LayerMask.NameToLayer("Floor")))
  //      {
  //          transform.position = hitInfo.point + Vector3.up * posOffsetY;
  //      }
	}

    private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawRay(transform.position, velocity);
	}
}
