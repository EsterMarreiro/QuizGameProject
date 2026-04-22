using UnityEngine;
using UnityEngine.UI;

public class ControladorVolume : MonoBehaviour
{
    public Slider slider;

    private AudioSource musicaFundo;

    void Start()
{
    GameObject obj = GameObject.Find("BackgroundSound");
    if (obj != null)
    {
        Debug.Log("BackgroundSound encontrado!");
        musicaFundo = obj.GetComponent<AudioSource>();
        slider.value = musicaFundo.volume;
    }
    else
    {
        Debug.Log("BackgroundSound NAO encontrado!");
    }
}

    public void MudarVolume(float valor)
    {
        if (musicaFundo != null)
        {
            musicaFundo.volume = valor;
        }
    }
}