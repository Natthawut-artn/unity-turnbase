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
        public Delegate.CallBack UseSkill1;
        public Delegate.CallBack UseSkill2;
        public Delegate.CallBack UseSkill3;
        public List<Skill> Skillset;
        public Data(Delegate.CallBack skip, Delegate.CallBack attack,
        Delegate.CallBack useSkill1 , Delegate.CallBack useSkill2 , Delegate.CallBack useSkill3, List<Skill> skillset)
        {
            Skip = skip;
            Attack = attack;
            UseSkill1 = useSkill1;
            UseSkill2 = useSkill2;
            UseSkill3 = useSkill3;
            Skillset = skillset;
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
            if(i >= 2)
            {
                Image image = button.GetComponent<Image>();
                image.sprite = Resources.Load<Sprite>(data.Skillset[i - 2]?.SkillSprite_Path);
                button.interactable = false;

                if (data.Skillset[i - 2].Unlock)
                {
                    button.transform.Find("Image").gameObject.SetActive(false);
                    button.interactable = true;
                }
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
            data.UseSkill1?.Invoke();
            buttons[2].interactable = false;
        });
        buttons[3].onClick.AddListener(() =>
        {
            data.UseSkill2?.Invoke();
            buttons[3].interactable = false;
        });
        buttons[4].onClick.AddListener(() =>
        {
            data.UseSkill3?.Invoke();
            buttons[4].interactable = false;
        });
    }
}

