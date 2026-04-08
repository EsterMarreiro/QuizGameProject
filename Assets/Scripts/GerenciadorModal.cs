using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ModalType
{
    Aviso,
    NivelConcluido,
    Codigo,
    Resultados
}

public class ModalManager : MonoBehaviour
{
    [System.Serializable]
    public class ModalItem
    {
        public ModalType type;
        public GameObject modalObject;
    }

    public List<ModalItem> modals;

    private Dictionary<ModalType, GameObject> modalDict;

    public static ModalManager Instance;

    void Awake()
    {
        Instance = this;

        modalDict = new Dictionary<ModalType, GameObject>();

        foreach (var item in modals)
        {
            modalDict[item.type] = item.modalObject;
            item.modalObject.SetActive(false);
        }
    }

    void Start()
    {
        CloseAll();
        StartCoroutine(ShowAfterDelay(ModalType.Aviso, 2f));
    }

    // 🔹 abre modal (uso interno)
    public void ShowModal(ModalType type)
    {
        CloseAll();

        if (modalDict.ContainsKey(type))
        {
            modalDict[type].SetActive(true);
        }
    }

    // 🔹 fecha modal (uso interno)
    public void CloseModal(ModalType type)
    {
        if (modalDict.ContainsKey(type))
        {
            modalDict[type].SetActive(false);
        }
    }

    // 🔹 abre por índice (para UI Button)
    public void ShowModalByIndex(int index)
    {
        ShowModal((ModalType)index);
    }

    // 🔹 fecha por índice (para UI Button)
    public void CloseModalByIndex(int index)
    {
        CloseModal((ModalType)index);
    }

    public void CloseAll()
    {
        foreach (var modal in modalDict.Values)
        {
            modal.SetActive(false);
        }
    }

    IEnumerator ShowAfterDelay(ModalType type, float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowModal(type);
    }
}