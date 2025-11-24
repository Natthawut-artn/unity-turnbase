using UnityEngine;
using System.Threading.Tasks;

public class LoadingPage : IUIElement
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

    public async void Show(object rawData)
    {
        _selfGameObject = GameObject.Instantiate
          (
              Resources.Load<GameObject>("Prefabs/UI/Pages/LoadingPage")
        );
        _selfGameObject.transform.SetParent(CanvasManager.Canvas0.transform, false);
        await Task.Delay(3000);
        Hide();
    }
}
