using UnityEngine;

public class FadeObject
{
    private Renderer rend;
    private Material material;
    private float fadeDuration;
    private float targetAlpha;
    private float currentAlpha;
    private bool isHitThisFrame = false;

    public FadeObject(Renderer rend, float duration, float alpha)
    {
        this.rend = rend;
        this.fadeDuration = duration;
        this.targetAlpha = alpha;

        // IMPORTANT: ทำให้วัสดุสามารถแยก Instance ได้
        material = rend.material;
        SetupMaterialForFade(material);

        currentAlpha = material.color.a;
    }

    public void MarkAsHitThisFrame()
    {
        isHitThisFrame = true;
        // เริ่ม Fade to transparent
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.deltaTime / fadeDuration);
        SetAlpha(currentAlpha);
    }

    public void MarkAsNotHitThisFrame()
    {
        isHitThisFrame = false;
    }

    public void ApplyFade()
    {
        if (!isHitThisFrame)
        {
            // ค่อยๆ fade กลับ
            currentAlpha = Mathf.MoveTowards(currentAlpha, 1f, Time.deltaTime / fadeDuration);
            SetAlpha(currentAlpha);
        }
        isHitThisFrame = false;
    }

    private void SetAlpha(float alpha)
    {
        Color c = material.color;
        c.a = alpha;
        material.color = c;
    }

    private void SetupMaterialForFade(Material mat)
    {
        mat.SetFloat("_Mode", 2); // 2 = Fade
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }
}
