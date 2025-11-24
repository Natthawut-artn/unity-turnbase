using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPage : IUIElement
{
    public GameObject SelfGameObject
    {
        get { return _selfGameObject; }
    }

    private GameObject _selfGameObject;
    private IUIManager _uiManager;
    private RectTransform _contentTranform;
    private int _maxPage = 4;
    private int _currentPage = 1;
    private Vector3 _targetPosition;
    private Vector3 _pageStep = new Vector3(-1500, 0, 0);
    private float tweentime = 0.3f;
    private LeanTweenType TweenType = LeanTweenType.easeOutCubic;

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
        _currentPage = 1;
        _targetPosition = Vector3.zero;
    }

    public void Show()
    {
        Show(new object());
    }

    public void Show(object rawData)
    {
        if (_selfGameObject == null)
        {
            _selfGameObject = GameObject.Instantiate
            (
                Resources.Load<GameObject>("Prefabs/UI/Pages/TutorialPage")
            );
            _selfGameObject.transform.SetParent(CanvasManager.Canvas4.transform, false);
        }

        _contentTranform = _selfGameObject.transform.Find("Scroll View/Viewport/Content").GetComponent<RectTransform>();

        Button LeftBtn = _selfGameObject.transform.Find("Next").GetComponent<Button>();
        LeftBtn.onClick.AddListener(() =>
        {
            SFXManager.Instance.PlayOneShot(SFX.Press1);
            Next();
        });

        Button RightBtn = _selfGameObject.transform.Find("Pers").GetComponent<Button>();
        RightBtn.onClick.AddListener(() =>
        {
            SFXManager.Instance.PlayOneShot(SFX.Press2);
            Pervious();
        });

        Button Confirm = _selfGameObject.transform.Find("Close").GetComponent<Button>();
        Confirm.onClick.AddListener(() =>
        {
            SFXManager.Instance.PlayOneShot(SFX.Cancel);
            Hide();
            PocketHUDController.Show();
            SettingHUDController.Show();
            HPBarsHUDController.Show();
            if (AppStateManager.appState == AppState.State)
            {
                PlayersInfoHUDController.Show();
            }
        });
    }

    
    public void Next()
    {
        if (_currentPage < _maxPage)
        {
            _currentPage++;
            _targetPosition += _pageStep;
            Move();
        }
    }

    public void Pervious()
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            _targetPosition -= _pageStep;
            Move();
        }
    }

    public void Move()
    {
        _contentTranform.LeanMoveLocal(_targetPosition, tweentime).setEase(TweenType);
    }
}
