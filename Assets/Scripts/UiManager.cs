using UnityEngine;
using TMPro;

public class UiManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void UpdateLife(int newLife)
    {
        scoreText.text = "Vidas: " + newLife.ToString();
    }
}
