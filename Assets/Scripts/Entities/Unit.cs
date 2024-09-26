using UnityEngine;
using System.Collections;

public class Unit : SelectableEntity
{
    public Movement Movement;
    public bool IsLeader = false;

    // Use this for initialization
    override protected void Awake()
    {
        base.Awake();
        Movement = GetComponent<Movement>();
	}

    public void SetTargetPos(Vector3 pos)
    {
        Movement.TargetPos = pos;
    }
}
