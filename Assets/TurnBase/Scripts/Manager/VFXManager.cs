using System.Collections.Generic;
using UnityEngine;

public enum VFX
{
    Striker_Skill1,
    Striker_Skill2,
    Striker_Skill3,
}

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance;

    void Awake()
    {
        Instance = this;
    }

    const string DGMTxt_path = "Graphic3D/DamageText";
    public List<GameObject> VFXList = new List<GameObject>();

    public void ShowDGMText(Vector3 gameObjPos , string DGMAmount)
    {
        GameObject txt = Instantiate( Resources.Load<GameObject>(DGMTxt_path) , gameObjPos + ( 1f * Vector3.up), Quaternion.identity);
        DGMTxt script = txt.GetComponent<DGMTxt>();
        script.Play(DGMAmount , 1f);
        Debug.Log("already show");
    }

    public void AddVFXOnWorld(GameObject GameObj, VFX vFX , float duration)
    {
        GameObject VFX = Instantiate(VFXList[(int)vFX] , GameObj.transform.position , GameObj.transform.rotation);
        Destroy(VFX , duration);
    }

    public void AddVFXOnGameObject(GameObject GameObj , VFX vFX)
    {
        GameObject VFX = Instantiate(VFXList[(int)vFX] , GameObj.transform.position , GameObj.transform.rotation);
        VFX.transform.SetParent(GameObj.transform);
    }

    public void ClearVFX(GameObject GameObj)
    {
        Transform parent = GameObj.transform; 

        foreach (Transform child in parent.GetComponentsInChildren<Transform>())
        {
            if (child.CompareTag("VFX"))
            {
                Destroy(child.gameObject);
            }
        }
    }
}