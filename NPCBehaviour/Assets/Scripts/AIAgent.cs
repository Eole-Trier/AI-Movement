using UnityEngine;
using UnityEngine.AI;

namespace FSMMono
{
    public class AIAgent : MonoBehaviour
    {
        private NavMeshAgent NavMeshAgentInst;

        private Material MaterialInst;

        private GameObject Target = null;
        public GameObject GetTarget() { return Target; }

        private SphereCollider SightTrigger = null;

        public float StartWaitTime { get; set; }

        private void SetMaterial(Color col)
        {
            MaterialInst.color = col;
        }
        public void SetWhiteMaterial() { SetMaterial(Color.white); }
        public void SetRedMaterial() { SetMaterial(Color.red); }
        public void SetBlueMaterial() { SetMaterial(Color.blue); }
        public void SetYellowMaterial() { SetMaterial(Color.yellow); }

        #region MonoBehaviour

        private void Awake()
        {
            SightTrigger = GetComponent<SphereCollider>();
            NavMeshAgentInst = GetComponent<NavMeshAgent>();

            Renderer rend = transform.Find("Body").GetComponent<Renderer>();
            MaterialInst = rend.material;

            StartWaitTime = Time.time;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                if (IsTargetInView(other.gameObject))
                    Target = other.gameObject;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject == Target)
            {
                Target = null;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            //Gizmos.DrawRay(sightRay);
            if (SightTrigger)
                Gizmos.DrawLine(sightRay.origin, sightRay.origin + sightRay.direction * SightTrigger.radius);
        }

        #endregion

        #region Perception methods

        public bool IsTargetInView()
        {
            return IsTargetInView(Target);
        }

        private Ray sightRay = new Ray();

        private bool IsTargetInView(GameObject _target)
        {
            if (_target == null)
                return false;

            int ignoreSelfLayer = ~(1 << LayerMask.NameToLayer("NPC"));
            Vector3 eyePos = transform.position;
            eyePos.y = _target.transform.position.y;
            sightRay.origin = eyePos;
            sightRay.direction = _target.transform.position - eyePos;
            RaycastHit hit;
            if (Physics.Raycast(sightRay, out hit, 100f, ignoreSelfLayer, QueryTriggerInteraction.Ignore))
            {
                //Debug.Log("Raycast with " + hit.collider.gameObject.name);
                if (hit.collider.gameObject == _target)
                    return true;
            }

            return false;
        }

        #endregion

        #region MoveMethods

        public void StopMove()
        {
            NavMeshAgentInst.isStopped = true;
        }

        public void MoveTo(Vector3 dest)
        {
            NavMeshAgentInst.isStopped = false;
            NavMeshAgentInst.SetDestination(dest);
        }

        public bool HasReachedPos()
        {
            return NavMeshAgentInst.remainingDistance - NavMeshAgentInst.stoppingDistance <= 0f;
        }

        public void MoveToTarget()
        {
            if (Target == null)
                return;
            Vector3 targetPos = Target.transform.position;
            targetPos.y = 0f;
            MoveTo(targetPos);
        }

        #endregion
    }
}