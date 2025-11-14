using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance;

    void Awake()
    {
        Instance = this;
    }

    const string DGMTxt_path = "Graphic3D/DamageText";

    public void ShowDGMText(Vector3 gameObjPos , string DGMAmount)
    {
        GameObject txt = Instantiate( Resources.Load<GameObject>(DGMTxt_path) , gameObjPos + ( 1f * Vector3.up), Quaternion.identity);
        DGMTxt script = txt.GetComponent<DGMTxt>();
        script.Play(DGMAmount , 1f);
        Debug.Log("already show");
    }
}