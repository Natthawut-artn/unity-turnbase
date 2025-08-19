using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour, IUIManager
{
    private readonly List<IUIElement> _elements = new();

    public void Register(IUIElement element)
    {
        if (!_elements.Contains(element))
            _elements.Add(element);
    }

    public void ShowElement(IUIElement element)
    {
        element.Show();
    }

    public void HideElement(IUIElement element)
    {
        element.Hide();
    }
}
