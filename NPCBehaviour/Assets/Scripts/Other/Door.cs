using UnityEngine;
//using System.Reflection;

public class Door : MonoBehaviour
{
    [SerializeField]
    private AnimationClip openAnim = null;

    private Animation doorAnim;

    void Awake()
    {
        doorAnim = GetComponentInChildren<Animation>();
        doorAnim.AddClip(openAnim, "open");

        //Component c = null;
        //System.Type t = c.GetType();
        //PropertyInfo info = t.GetRuntimeProperty("enabled");
        //info.GetGetMethod().Invoke(c, new object[] { false });
    }

    private void OnTriggerEnter(Collider other)
    {
        doorAnim["open"].speed = 1;
        doorAnim.Play("open");
    }

    private void OnTriggerExit(Collider other)
    {
        doorAnim["open"].speed = -1;
        if (doorAnim["open"].time == 0f)
            doorAnim["open"].time = doorAnim["open"].length;
        doorAnim.Play("open");
    }
}
