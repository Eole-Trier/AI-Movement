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
    [SerializeField]
    private float SeparationRadius = 1f;
    [SerializeField]
    private float MaxSeparation = 2f;
    [SerializeField]
    private float LeaderSightRadius = 5f;

    public Vector3 velocity = Vector3.zero;
    public float rotation = 0f;

    public Vector3 desiredVelocity = Vector3.zero;
    public Vector3 steering = Vector3.zero;

    private float distance = 0f;

    private Vector3 behind = Vector3.zero;
    private Vector3 ahead = Vector3.zero;
    [SerializeField]
    private float LeaderBehindDistance;

    private float posOffsetY = 0.5f;
    private Vector3 targetPos;
    public Vector3 TargetPos
    {
        get { return targetPos; }
        set { targetPos = value; targetPos.y += posOffsetY; }
    }

    private Unit unit;
    public Unit Leader;
    public PlayerController PlayerController;

    private void Start()
    {
        unit = GetComponent<Unit>();
        PlayerController = FindObjectOfType<PlayerController>();
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
        if (unit.IsLeader) 
            steering = desiredVelocity - velocity;
        else
        {
            Vector3 force = Vector3.zero;
            force = FollowLeaderForce() + CrowdSeparationForce();
            //if (IsOnLeaderSight())
                //force += evade
            steering += force;
        }
        steering = steering.normalized * MaxForce;
        steering = steering / Mass;
        velocity += steering;

        rotation = GetOrientationFromDirection(velocity);

		// truncate to max speed
        if (velocity.magnitude > MaxSpeed)
		{
			velocity.Normalize();
            velocity *= MaxSpeed;
        }
        if (Leader.Movement.desiredVelocity.magnitude < 0.1f)
        {
            velocity = Vector3.zero;
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

    private Vector3 FollowLeaderForce()
    {
        Vector3 tv = Leader.Movement.velocity * -1;
        tv = tv.normalized * LeaderBehindDistance;
        behind = Leader.transform.position + tv;
        return behind - transform.position;
    }

    private bool IsOnLeaderSight()
    {
        return (ahead - transform.position).magnitude <= LeaderSightRadius
            || (Leader.transform.position - transform.position).magnitude <= LeaderSightRadius;
    }

    private Vector3 CrowdSeparationForce()
    {
        Vector3 force = Vector3.zero;
        int neighborCount = 0;
        foreach (Unit unit in PlayerController.Units)
        {
            if (unit != this && (unit.transform.position - transform.position).magnitude < SeparationRadius)
            {
                force.x += unit.transform.position.x - transform.position.x;
                force.z += unit.transform.position.z - transform.position.z;
                neighborCount++;    
            }
        }
        if (neighborCount != 0)
        {
            force.x /= neighborCount;
            force.z /= neighborCount;
            force *= (-1);
        }
        force = force.normalized;
        force *= MaxSeparation;
        return force;
    }

    private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawRay(transform.position, velocity);
	}
}
