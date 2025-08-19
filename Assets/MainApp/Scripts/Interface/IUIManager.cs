public interface IUIManager
{
    void Register(IUIElement element);
    void ShowElement(IUIElement element);
    void HideElement(IUIElement element);
}
