using UnityEngine;
using UnityEngine.UI;

public class ControladorVolume : MonoBehaviour
{
    public Slider slider;
    public AudioSource musicaFundo;

    void Start()
    {
        slider.value = musicaFundo.volume;
    }

    public void MudarVolume(float valor)
    {
        musicaFundo.volume = valor;
    }
}