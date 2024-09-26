using UnityEngine;
using System.Collections.Generic;
using Navigation;

public class PlayerController : MonoBehaviour {

    [SerializeField]
    private GameObject TargetCursorPrefab = null;
    private GameObject targetCursor = null;
    [SerializeField]
    private GameObject UnitPrefab = null;
    [SerializeField]
    private Transform PlayerStart = null;
    public List<Unit> Units;

    [SerializeField]
    private int numberOfUnits;

    delegate void InputEventHandler();
    event InputEventHandler OnMouseClicked;


    private GameObject GetTargetCursor()
    {
        if (targetCursor == null)
            targetCursor = Instantiate(TargetCursorPrefab);
        return targetCursor;
    }

    // Use this for initialization
    private void Start ()
    {
        Units = new List<Unit>();
        if (UnitPrefab)
        {
            for (int i = 0; i < numberOfUnits; i++)
            {
                GameObject unitInst = Instantiate(UnitPrefab, PlayerStart, false);
                unitInst.transform.parent = null;
                unitInst.transform.localPosition += new Vector3(2 * i, 0, 0);
                Units.Add(unitInst.GetComponent<Unit>());
                RaycastHit raycastInfo;
                Ray ray = new Ray(unitInst.transform.position, Vector3.down);
                if (Physics.Raycast(ray, out raycastInfo, 10f, 1 << LayerMask.NameToLayer("Floor")))
                {
                    unitInst.transform.position = raycastInfo.point;
                }

                Units[i].SetSelected(true);
            }
            Units[0].IsLeader = true;
            for (int i = 0; i < numberOfUnits; i++)
            {
                Units[i].Movement.Leader = Units[0];
            }
        }

        OnMouseClicked += () =>
        {
            int floorLayer = 1 << LayerMask.NameToLayer("Floor");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit raycastInfo;
            // unit move target
            if (Physics.Raycast(ray, out raycastInfo, Mathf.Infinity, floorLayer))
            {
                Vector3 newPos = raycastInfo.point;
                Vector3 targetPos = newPos;
                targetPos.y += 0.1f;
                GetTargetCursor().transform.position = targetPos;

                if (TileNavGraph.Instance.IsPosValid(newPos))
                {
                    foreach (Unit unit in Units)
                    {
                        unit.SetTargetPos(newPos);
                    }
                }
            }
        };
    }

    // Update is called once per frame
    private void Update ()
    {
        if (Input.GetMouseButtonDown(0))
            OnMouseClicked();
	}
}
