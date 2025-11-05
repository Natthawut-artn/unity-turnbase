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
        if (material.HasProperty("_BaseColor"))
        {
            Color c = material.GetColor("_BaseColor");
            c.a = alpha;
            material.SetColor("_BaseColor", c);
        }

        if (material.HasProperty("_Color"))
        {
            Color c = material.GetColor("_Color");
            c.a = alpha;
            material.SetColor("_Color", c);
        }
    }


    private void SetupMaterialForFade(Material mat)
    {
        if (mat.HasProperty("_Surface"))
        {
            mat.SetFloat("_Surface", 1f); // 1 = Transparent
        }

        if (mat.HasProperty("_Blend"))
        {
            mat.SetFloat("_Blend", 0f); // Alpha (0) = Normal blending
        }

        if (mat.HasProperty("_ZWrite"))
        {
            mat.SetFloat("_ZWrite", 0f); // Disable depth write
        }

        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        if (mat.HasProperty("_BaseColor"))
        {
            Color baseColor = mat.GetColor("_BaseColor");
            baseColor.a = mat.color.a; // sync กับ material.color
            mat.SetColor("_BaseColor", baseColor);
        }
    }

}
