using UnityEngine;
using UnityEngine.UI;
public class healthbar : MonoBehaviour
{
    public Image healthBarImage;

    public void UpdateHealthBar(float hp, float maxhp)
    {
        healthBarImage.fillAmount = Mathf.Clamp(hp / maxhp, 0, 1f);
    }
}