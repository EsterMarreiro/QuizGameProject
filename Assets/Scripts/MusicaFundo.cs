using UnityEngine;

public class MusicaFundo : MonoBehaviour
{
    private static MusicaFundo instancia;

    private void Awake()
    {
        if (instancia != null)
        {
            Destroy(gameObject);
            return;
        }

        instancia = this;
        DontDestroyOnLoad(gameObject);
    }
}