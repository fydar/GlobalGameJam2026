using System.Collections;
using UnityEngine;

public class Overworld : MonoBehaviour
{
    public IEnumerator Run()
    {
        yield return null;
        Debug.Log("Overworld is running");
    }
}
