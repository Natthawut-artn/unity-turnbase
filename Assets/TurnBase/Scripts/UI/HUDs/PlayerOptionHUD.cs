using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DelegateManager;
using UnityEngine.UI;

public class PlayerOptionHUD : IUIElement
{
    public class Data
    {
        public Delegate.CallBack Skip;
        public Delegate.CallBack Attack;
        public Delegate.CallBack UseSkill;
        public Skill Skill;
        public Data(Delegate.CallBack skip, Delegate.CallBack attack , Delegate.CallBack useSkill , Skill skill)
        {
            Skip = skip;
            Attack = attack;
            UseSkill = useSkill;
            Skill = skill;
        }
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
        data = (Data)rawData;
        _selfGameObject = GameObject.Instantiate
          (
              Resources.Load<GameObject>("Prefabs/UI/HUDs/PlayerOptionHUD")
        );
        _selfGameObject.transform.SetParent(CanvasManager.Canvas1.transform, false);

        List<Button> buttons = new List<Button>();
        for (int i = 0; i < _selfGameObject.transform.childCount; i++)
        {
            Button button = _selfGameObject.transform.GetChild(i).GetComponent<Button>();
            buttons.Add(button);
            if(i == 2)
            {
                Image image = button.transform.Find("Image").GetComponent<Image>();
                image.sprite = Resources.Load<Sprite>(data.Skill?.SkillSprite_Path);
            }
        }

        buttons[0].onClick.AddListener(() =>
        {
            data.Attack?.Invoke();
        });
        buttons[1].onClick.AddListener(() =>
        {
            data.Skip?.Invoke();
        });
        buttons[2].onClick.AddListener(() =>
        {
            data.UseSkill?.Invoke();
            buttons[2].interactable = false;
        });
    }
}

