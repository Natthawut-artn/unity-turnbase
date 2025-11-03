using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillPointsHUD : IUIElement
{
    public class Data
    {
        
    }
    public Data data;

    public GameObject SelfGameObject
    {
        get { return _selfGameObject; }
    }

    private GameObject _selfGameObject;
    private IUIManager _uiManager;

    public void Init()
    {
        _uiManager = ServiceLocator.Get<IUIManager>();
        ServiceLocator.Register(this);
        _uiManager.Register(this);
    }

    public void Dispose()
    {

    }

    public void Hide()
    {
        if (_selfGameObject == null)
            return;

        _selfGameObject.SetActive(false);
        Object.Destroy(_selfGameObject);
        _selfGameObject = null;
    }

    public void Show()
    {
        Show(new object());
    }

    public void Show(object rawData)
    {
        _selfGameObject = GameObject.Instantiate
          (
              Resources.Load<GameObject>("Prefabs/UI/HUDs/SkillPointsHUD")
        );
        _selfGameObject.transform.SetParent(CanvasManager.Canvas1.transform, false);

        GameObject SkillPoints = _selfGameObject.transform.Find("SkillPoints").gameObject;
        int currentSkillPoint = SkillPointsManager.currentSkillPoint;
        for (int i = 0; i < SkillPoints.transform.childCount; i++)
        {
            GameObject SP = SkillPoints.transform.GetChild(i).gameObject;
            GameObject SkillPoint = SP.transform.Find("Image").gameObject;
            if (i < currentSkillPoint)
            {
                SkillPoint.SetActive(true);
            }
            else
            {
                SkillPoint.SetActive(false);
            }
        }
    }

    public void Update()
    {
        if (_selfGameObject == null) return;
        GameObject SkillPoints = _selfGameObject.transform.Find("SkillPoints").gameObject;
        int currentSkillPoint = SkillPointsManager.currentSkillPoint;
        for (int i = 0; i < SkillPoints.transform.childCount; i++)
        {
            GameObject SP = SkillPoints.transform.GetChild(i).gameObject;
            GameObject SkillPoint = SP.transform.Find("Image").gameObject;
            if (i < currentSkillPoint)
            {
                SkillPoint.SetActive(true);
            }
            else
            {
                SkillPoint.SetActive(false);
            }
        }
    }
}
