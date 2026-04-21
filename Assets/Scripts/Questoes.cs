using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Questoes : MonoBehaviour
{
    public enum NivelQuiz
    {
        Facil = 0,
        Medio = 1,
        Dificil = 2
    }

    [System.Serializable]
    public class PerguntaData
    {
        [TextArea] public string enunciado;
        public string alternativaA;
        public string alternativaB;
        public string alternativaC;
        public string alternativaD;
        [Range(0, 3)] public int indiceCorreto;
    }

    [Header("Textos")]
    public TMP_Text pergunta;
    public TMP_Text alternativa_a;
    public TMP_Text alternativa_b;
    public TMP_Text alternativa_c;
    public TMP_Text alternativa_d;

    [Header("Modais")]
    public GameObject modalPergunta;
    public GameObject modalNivelConcluido;
    public GameObject modalCodigo;
    public GameObject modalDeactivate;
    public GameObject modalResultados;

    [Header("Botoes de Nivel")]
    public Button buttonQuizz;
    public Button buttonQuizzMedio;
    public Button buttonQuizzDificil;
    public Button buttonSeguir;

    [Header("Botoes de Alternativa")]
    public Button alternativaAButton;
    public Button alternativaBButton;
    public Button alternativaCButton;
    public Button alternativaDButton;

    [Header("Botoes do Deactivate")]
    public Button buttonDeactivate;
    public Button buttonIrParaDeactivate;
    public Button buttonDesativar;
    public Button buttonAbortar;

    [Header("UI do Deactivate")]
    public TMP_InputField inputCodigoDeactivate;
    public TMP_Text feedbackCodigoDeactivate;

    [Header("UI do Modal Resultados")]
    public TMP_Text textoSequenciaAcertos;
    public TMP_Text textoQuantidadeErros;
    public TMP_Text textoMelhorSequenciaAcertos;
    public TMP_Text textoMelhorTempoQuiz;
    public TMP_Text textoTempoTotalGasto;
    public TMP_Text textoMenorQuantidadeErrosNivel;

    [Header("Codigo Final")]
    public string codigoDesativacao = "9982-X";
    public TMP_Text textoCodigoNoBotaoDificil;

    [Header("Temporizador")]
    public float tempoLimite = 60f;
    public AudioSource audioSourceTempo;
    public AudioClip somTempoEsgotado;
    public TMP_Text textoTemporizador;

    [Header("Banco de Perguntas")]
    public List<PerguntaData> perguntasFacil = new List<PerguntaData>();
    public List<PerguntaData> perguntasMedio = new List<PerguntaData>();
    public List<PerguntaData> perguntasDificil = new List<PerguntaData>();

    private NivelQuiz nivelAtual;
    private int indicePerguntaAtual;
    private int maiorNivelDesbloqueado = (int)NivelQuiz.Facil;
    private bool quizEmAndamento;

    private int sequenciaAcertosAtual;
    private int quantidadeErrosTotal;
    private int melhorSequenciaAcertos;
    private float melhorTempoQuiz = float.PositiveInfinity;
    private int menorQuantidadeErrosNivel = int.MaxValue;
    private bool sessaoIniciada;
    private float inicioSessaoTempo;
    private float inicioNivelTempo;
    private int errosNivelAtual;
    private float tempoTotalGasto;
    private float bloqueioInteracaoAte;

    private float tempoRestante;
    private bool temporizadorAtivo;

    private void Awake()
    {
        PreencherBancosPadraoSeVazio();
        VincularObjetosDaCena();
        GarantirUIDeactivate();
        GarantirUIResultados();
        AtualizarCodigoNoBotaoDificil();
        ConfigurarListeners();
    }

    private void Start()
    {
        ResetarMetricasSessao();
        quizEmAndamento = false;
        FecharTodosOsModais();
        AtualizarVisibilidadeBotoesDeNivel();
    }

    private void Update()
    {
        if (!temporizadorAtivo) return;

        tempoRestante -= Time.deltaTime;

        if (textoTemporizador != null)
        {
            textoTemporizador.text = Mathf.CeilToInt(tempoRestante).ToString();
        }

        if (tempoRestante <= 0f)
        {
            TempoEsgotado();
        }
    }

    private void TempoEsgotado()
    {
        temporizadorAtivo = false;
        tempoRestante = 0f;

        if (audioSourceTempo != null && somTempoEsgotado != null)
        {
            audioSourceTempo.PlayOneShot(somTempoEsgotado);
        }

        errosNivelAtual++;
        quantidadeErrosTotal++;
        sequenciaAcertosAtual = 0;

        ReiniciarNivelAtual();

        tempoRestante = tempoLimite;
        temporizadorAtivo = true;
    }

    public void IniciarNivelFacil()
    {
        if (InteracaoBloqueada())
        {
            return;
        }

        IniciarNivel(NivelQuiz.Facil);
    }

    public void IniciarNivelMedio()
    {
        if (InteracaoBloqueada())
        {
            return;
        }

        IniciarNivel(NivelQuiz.Medio);
    }

    public void IniciarNivelDificil()
    {
        if (InteracaoBloqueada())
        {
            return;
        }

        IniciarNivel(NivelQuiz.Dificil);
    }

    public void ResponderAlternativa(int indiceAlternativa)
    {
        if (InteracaoBloqueada())
        {
            return;
        }

        if (!quizEmAndamento)
        {
            return;
        }

        List<PerguntaData> perguntasNivelAtual = ObterPerguntasDoNivel(nivelAtual);
        if (perguntasNivelAtual.Count == 0 || indicePerguntaAtual >= perguntasNivelAtual.Count)
        {
            return;
        }

        PerguntaData perguntaAtual = perguntasNivelAtual[indicePerguntaAtual];

        if (indiceAlternativa == perguntaAtual.indiceCorreto)
        {
            sequenciaAcertosAtual++;
            if (sequenciaAcertosAtual > melhorSequenciaAcertos)
            {
                melhorSequenciaAcertos = sequenciaAcertosAtual;
            }

            indicePerguntaAtual++;

            if (indicePerguntaAtual >= perguntasNivelAtual.Count)
            {
                ConcluirNivel();
                return;
            }

            AtualizarPerguntaNaTela();
            return;
        }

        quantidadeErrosTotal++;
        errosNivelAtual++;
        sequenciaAcertosAtual = 0;
        ReiniciarNivelAtual();
    }

    public void AoClicarSeguirNivelConcluido()
    {
        if (InteracaoBloqueada())
        {
            return;
        }

        if (modalNivelConcluido != null)
        {
            modalNivelConcluido.SetActive(false);
        }
        else if (ModalManager.Instance != null)
        {
            ModalManager.Instance.CloseModal(ModalType.NivelConcluido);
        }

        AtualizarVisibilidadeBotoesDeNivel();
    }

    public void ValidarCodigoDeactivate()
    {
        if (InteracaoBloqueada())
        {
            return;
        }

        if (modalDeactivate == null || !modalDeactivate.activeSelf)
        {
            return;
        }

        string codigoDigitado = inputCodigoDeactivate == null ? string.Empty : inputCodigoDeactivate.text.Trim();

        if (codigoDigitado == codigoDesativacao)
        {
            LimparUIDeactivate();

            if (modalDeactivate != null)
            {
                modalDeactivate.SetActive(false);
            }

            FecharTodosOsModais();
            FinalizarSessaoEAtualizarResultados();

            if (modalResultados != null)
            {
                modalResultados.SetActive(true);
            }
            else if (ModalManager.Instance != null)
            {
                ModalManager.Instance.ShowModal(ModalType.Resultados);
            }

            return;
        }

        ExibirErroCodigo("Codigo invalido. Tente novamente.");
    }

    public void AbortarDeactivate()
    {
        if (InteracaoBloqueada())
        {
            return;
        }

        LimparUIDeactivate();

        if (modalDeactivate != null)
        {
            modalDeactivate.SetActive(false);
        }
    }

    private void IniciarNivel(NivelQuiz nivel)
    {
        if (InteracaoBloqueada())
        {
            return;
        }

        if ((int)nivel > maiorNivelDesbloqueado)
        {
            return;
        }

        List<PerguntaData> perguntasNivel = ObterPerguntasDoNivel(nivel);
        if (perguntasNivel.Count == 0)
        {
            Debug.LogError($"Sem perguntas configuradas para o nivel {nivel}.");
            return;
        }

        FecharTodosOsModais();

        if (!sessaoIniciada)
        {
            sessaoIniciada = true;
            inicioSessaoTempo = Time.time;
        }

        inicioNivelTempo = Time.time;
        errosNivelAtual = 0;
        nivelAtual = nivel;
        indicePerguntaAtual = 0;
        quizEmAndamento = true;

        tempoRestante = tempoLimite;
        temporizadorAtivo = true;

        if (modalPergunta != null)
        {
            modalPergunta.SetActive(true);
        }

        AtualizarPerguntaNaTela();
    }

    private void ReiniciarNivelAtual()
    {
        indicePerguntaAtual = 0;
        AtualizarPerguntaNaTela();
    }

    private void ConcluirNivel()
    {
        temporizadorAtivo = false;

        quizEmAndamento = false;
        BloquearInteracaoPor(0.25f);

        if (modalPergunta != null)
        {
            modalPergunta.SetActive(false);
        }

        RegistrarConclusaoNivel();

        if (nivelAtual == NivelQuiz.Dificil)
        {
            AbrirModalCodigo();
            return;
        }

        int proximoNivel = Mathf.Min((int)NivelQuiz.Dificil, (int)nivelAtual + 1);
        maiorNivelDesbloqueado = Mathf.Max(maiorNivelDesbloqueado, proximoNivel);

        if (modalNivelConcluido != null)
        {
            modalNivelConcluido.SetActive(true);
        }
        else if (ModalManager.Instance != null)
        {
            ModalManager.Instance.ShowModal(ModalType.NivelConcluido);
        }
        else
        {
            Debug.LogError("ModalNivelConcluido nao encontrado para exibir ao concluir o nivel.");
        }

        AtualizarVisibilidadeBotoesDeNivel();
    }

    private void AtualizarPerguntaNaTela()
    {
        List<PerguntaData> perguntasNivelAtual = ObterPerguntasDoNivel(nivelAtual);
        if (perguntasNivelAtual.Count == 0 || indicePerguntaAtual >= perguntasNivelAtual.Count)
        {
            return;
        }

        PerguntaData atual = perguntasNivelAtual[indicePerguntaAtual];

        if (pergunta != null) pergunta.text = atual.enunciado;
        if (alternativa_a != null) alternativa_a.text = atual.alternativaA;
        if (alternativa_b != null) alternativa_b.text = atual.alternativaB;
        if (alternativa_c != null) alternativa_c.text = atual.alternativaC;
        if (alternativa_d != null) alternativa_d.text = atual.alternativaD;
    }

    private List<PerguntaData> ObterPerguntasDoNivel(NivelQuiz nivel)
    {
        switch (nivel)
        {
            case NivelQuiz.Facil:
                return perguntasFacil;
            case NivelQuiz.Medio:
                return perguntasMedio;
            case NivelQuiz.Dificil:
                return perguntasDificil;
            default:
                return perguntasFacil;
        }
    }

    private void AtualizarVisibilidadeBotoesDeNivel()
    {
        if (buttonQuizz != null)
        {
            buttonQuizz.gameObject.SetActive(true);
        }

        if (buttonQuizzMedio != null)
        {
            buttonQuizzMedio.gameObject.SetActive(maiorNivelDesbloqueado >= (int)NivelQuiz.Medio);
        }

        if (buttonQuizzDificil != null)
        {
            buttonQuizzDificil.gameObject.SetActive(maiorNivelDesbloqueado >= (int)NivelQuiz.Dificil);
        }
    }

    private void FecharTodosOsModais()
    {
        temporizadorAtivo = false;

        if (ModalManager.Instance != null)
        {
            ModalManager.Instance.CloseAll();
        }

        if (modalPergunta != null) modalPergunta.SetActive(false);
        if (modalNivelConcluido != null) modalNivelConcluido.SetActive(false);
        if (modalCodigo != null) modalCodigo.SetActive(false);
        if (modalDeactivate != null) modalDeactivate.SetActive(false);
        if (modalResultados != null) modalResultados.SetActive(false);
    }

    private void ConfigurarListeners()
    {
        ConfigurarListener(buttonQuizz, IniciarNivelFacil);
        ConfigurarListener(buttonQuizzMedio, IniciarNivelMedio);
        ConfigurarListener(buttonQuizzDificil, IniciarNivelDificil);
        ConfigurarListener(buttonSeguir, AoClicarSeguirNivelConcluido);

        ConfigurarListener(alternativaAButton, () => ResponderAlternativa(0));
        ConfigurarListener(alternativaBButton, () => ResponderAlternativa(1));
        ConfigurarListener(alternativaCButton, () => ResponderAlternativa(2));
        ConfigurarListener(alternativaDButton, () => ResponderAlternativa(3));

        ConfigurarListener(buttonDeactivate, AbrirModalDeactivate);
        ConfigurarListener(buttonIrParaDeactivate, AbrirModalDeactivate);
        ConfigurarListener(buttonDesativar, ValidarCodigoDeactivate);
        ConfigurarListener(buttonAbortar, AbortarDeactivate);
    }

    private static void ConfigurarListener(Button botao, UnityEngine.Events.UnityAction acao)
    {
        if (botao == null)
        {
            return;
        }

        botao.onClick.RemoveAllListeners();
        botao.onClick.AddListener(acao);
    }

    private void VincularObjetosDaCena()
    {
        VincularBotaoSeNulo(ref buttonQuizz, "ButtonQuizz");
        VincularBotaoSeNulo(ref buttonQuizzMedio, "ButtonQuizzMedio");
        VincularBotaoSeNulo(ref buttonQuizzDificil, "ButtonQuizzDificil");
        VincularBotaoSeNulo(ref buttonSeguir, "ButtonSeguir");

        VincularBotaoSeNulo(ref alternativaAButton, "AlternativaA");
        VincularBotaoSeNulo(ref alternativaBButton, "AlternativaB");
        VincularBotaoSeNulo(ref alternativaCButton, "AlternativaC");
        VincularBotaoSeNulo(ref alternativaDButton, "AlternativaD");
        VincularBotaoSeNulo(ref buttonDeactivate, "ButtonDeactivate");
        VincularBotaoSeNulo(ref buttonIrParaDeactivate, "ButtonIrParaDeactivate");
        VincularBotaoSeNulo(ref buttonDesativar, "ButtonDesativar");
        VincularBotaoSeNulo(ref buttonAbortar, "ButtonAbortar");

        VincularModalSeNulo(ref modalPergunta, "ModalPergunta");
        VincularModalSeNulo(ref modalNivelConcluido, "ModalNivelConcluido");
        VincularModalSeNulo(ref modalCodigo, "ModalCodigo");
        VincularModalSeNulo(ref modalDeactivate, "ModalDeactivate");
        VincularModalSeNulo(ref modalResultados, "ModalResultados");

        VincularInputCodigoDeactivateSeNulo();
        VincularFeedbackDeactivateSeNulo();
        VincularTextosResultados();
    }

    private static void VincularBotaoSeNulo(ref Button campo, string nomeDoObjeto)
    {
        if (campo != null)
        {
            return;
        }

        GameObject encontrado = EncontrarObjetoNaCena(nomeDoObjeto);
        if (encontrado == null)
        {
            return;
        }

        campo = encontrado.GetComponent<Button>();
    }

    private static void VincularModalSeNulo(ref GameObject campo, string nomeDoObjeto)
    {
        if (campo == null)
        {
            campo = EncontrarObjetoNaCena(nomeDoObjeto);
        }
    }

    private static GameObject EncontrarObjetoNaCena(string nomeDoObjeto)
    {
        if (string.IsNullOrEmpty(nomeDoObjeto))
        {
            return null;
        }

        Scene cenaAtiva = SceneManager.GetActiveScene();
        if (!cenaAtiva.IsValid())
        {
            return GameObject.Find(nomeDoObjeto);
        }

        GameObject[] roots = cenaAtiva.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Transform encontrado = EncontrarFilhoRecursivo(roots[i].transform, nomeDoObjeto);
            if (encontrado != null)
            {
                return encontrado.gameObject;
            }
        }

        return null;
    }

    private static Transform EncontrarFilhoRecursivo(Transform raiz, string nome)
    {
        if (raiz == null)
        {
            return null;
        }

        if (raiz.name == nome)
        {
            return raiz;
        }

        int filhos = raiz.childCount;
        for (int i = 0; i < filhos; i++)
        {
            Transform encontrado = EncontrarFilhoRecursivo(raiz.GetChild(i), nome);
            if (encontrado != null)
            {
                return encontrado;
            }
        }

        return null;
    }

    private void VincularInputCodigoDeactivateSeNulo()
    {
        if (modalDeactivate == null || inputCodigoDeactivate != null)
        {
            return;
        }

        Transform inputTransform = EncontrarFilhoRecursivo(modalDeactivate.transform, "InputCodigo");
        if (inputTransform != null)
        {
            inputCodigoDeactivate = inputTransform.GetComponent<TMP_InputField>();
        }

        if (inputCodigoDeactivate == null)
        {
            inputCodigoDeactivate = modalDeactivate.GetComponentInChildren<TMP_InputField>(true);
        }
    }

    private void VincularFeedbackDeactivateSeNulo()
    {
        if (modalDeactivate == null || feedbackCodigoDeactivate != null)
        {
            return;
        }

        Transform feedbackTransform = EncontrarFilhoRecursivo(modalDeactivate.transform, "TextoFeedbackDeactivate");
        if (feedbackTransform != null)
        {
            feedbackCodigoDeactivate = feedbackTransform.GetComponent<TMP_Text>();
        }
    }

    private void VincularTextosResultados()
    {
        if (modalResultados == null)
        {
            return;
        }

        VincularTextoResultadoSeNulo(ref textoSequenciaAcertos, "TextoSequenciaAcertos");
        VincularTextoResultadoSeNulo(ref textoQuantidadeErros, "TextoQuantidadeErros");
        VincularTextoResultadoSeNulo(ref textoMelhorSequenciaAcertos, "TextoMelhorSequenciaAcertos");
        VincularTextoResultadoSeNulo(ref textoMelhorTempoQuiz, "TextoMelhorTempoQuiz");
        VincularTextoResultadoSeNulo(ref textoTempoTotalGasto, "TextoTempoTotalGasto");
        VincularTextoResultadoSeNulo(ref textoMenorQuantidadeErrosNivel, "TextoMenorQuantidadeErrosNivel");
    }

    private void VincularTextoResultadoSeNulo(ref TMP_Text campo, string nomeDoObjeto)
    {
        if (campo != null || modalResultados == null)
        {
            return;
        }

        Transform encontrado = EncontrarFilhoRecursivo(modalResultados.transform, nomeDoObjeto);
        if (encontrado != null)
        {
            campo = encontrado.GetComponent<TMP_Text>();
        }
    }

    private void AbrirModalCodigo()
    {
        LimparUIDeactivate();
        BloquearInteracaoPor(0.25f);

        if (modalDeactivate != null)
        {
            modalDeactivate.SetActive(false);
        }

        if (modalCodigo != null)
        {
            modalCodigo.SetActive(true);
            return;
        }

        if (ModalManager.Instance != null)
        {
            ModalManager.Instance.ShowModal(ModalType.Codigo);
            return;
        }

        Debug.LogError("ModalCodigo nao encontrado para abrir ao concluir nivel dificil.");
    }

    private void AbrirModalDeactivate()
    {
        if (InteracaoBloqueada())
        {
            return;
        }

        LimparUIDeactivate();

        if (modalCodigo != null)
        {
            modalCodigo.SetActive(false);
        }

        if (modalDeactivate != null)
        {
            modalDeactivate.SetActive(true);
            return;
        }

        Debug.LogError("ModalDeactivate nao encontrado.");
    }

    private void ExibirErroCodigo(string mensagem)
    {
        if (feedbackCodigoDeactivate != null)
        {
            feedbackCodigoDeactivate.text = mensagem;
            feedbackCodigoDeactivate.color = new Color(1f, 0.3f, 0.3f, 1f);
        }
    }

    private void LimparUIDeactivate()
    {
        if (inputCodigoDeactivate != null)
        {
            inputCodigoDeactivate.text = string.Empty;
        }

        if (feedbackCodigoDeactivate != null)
        {
            feedbackCodigoDeactivate.text = string.Empty;
        }
    }

    private void ResetarMetricasSessao()
    {
        sequenciaAcertosAtual = 0;
        quantidadeErrosTotal = 0;
        melhorSequenciaAcertos = 0;
        melhorTempoQuiz = float.PositiveInfinity;
        menorQuantidadeErrosNivel = int.MaxValue;
        sessaoIniciada = false;
        inicioSessaoTempo = 0f;
        inicioNivelTempo = -1f;
        errosNivelAtual = 0;
        tempoTotalGasto = 0f;
    }

    private void RegistrarConclusaoNivel()
    {
        if (inicioNivelTempo < 0f)
        {
            return;
        }

        float tempoNivel = Time.time - inicioNivelTempo;
        if (tempoNivel < melhorTempoQuiz)
        {
            melhorTempoQuiz = tempoNivel;
        }

        if (errosNivelAtual < menorQuantidadeErrosNivel)
        {
            menorQuantidadeErrosNivel = errosNivelAtual;
        }
    }

    private void FinalizarSessaoEAtualizarResultados()
    {
        GarantirUIResultados();

        if (sessaoIniciada)
        {
            tempoTotalGasto = Time.time - inicioSessaoTempo;
        }
        else
        {
            tempoTotalGasto = 0f;
        }

        AtualizarTextosResultados();
    }

    private void AtualizarTextosResultados()
    {
        if (textoSequenciaAcertos != null)
        {
            textoSequenciaAcertos.text = $"Sequencia de acertos: {sequenciaAcertosAtual}";
        }

        if (textoQuantidadeErros != null)
        {
            textoQuantidadeErros.text = $"Quantidade de erros: {quantidadeErrosTotal}";
        }

        if (textoMelhorSequenciaAcertos != null)
        {
            textoMelhorSequenciaAcertos.text = $"Melhor sequencia de acertos: {melhorSequenciaAcertos}";
        }

        if (textoMelhorTempoQuiz != null)
        {
            string melhorTempo = melhorTempoQuiz == float.PositiveInfinity ? "--:--" : FormatarTempo(melhorTempoQuiz);
            textoMelhorTempoQuiz.text = $"Melhor tempo para responder um quizz: {melhorTempo}";
        }

        if (textoTempoTotalGasto != null)
        {
            textoTempoTotalGasto.text = $"Tempo total gasto: {FormatarTempo(tempoTotalGasto)}";
        }

        if (textoMenorQuantidadeErrosNivel != null)
        {
            string menorErro = menorQuantidadeErrosNivel == int.MaxValue ? "0" : menorQuantidadeErrosNivel.ToString();
            textoMenorQuantidadeErrosNivel.text = $"Menor quantidade de erros em um nivel: {menorErro}";
        }
    }

    private static string FormatarTempo(float tempoEmSegundos)
    {
        int totalSegundos = Mathf.Max(0, Mathf.FloorToInt(tempoEmSegundos));
        int horas = totalSegundos / 3600;
        int minutos = (totalSegundos % 3600) / 60;
        int segundos = totalSegundos % 60;

        if (horas > 0)
        {
            return $"{horas:00}:{minutos:00}:{segundos:00}";
        }

        return $"{minutos:00}:{segundos:00}";
    }

    private void AtualizarCodigoNoBotaoDificil()
    {
        if (buttonQuizzDificil == null)
        {
            return;
        }

        if (textoCodigoNoBotaoDificil == null)
        {
            textoCodigoNoBotaoDificil = buttonQuizzDificil.GetComponentInChildren<TMP_Text>(true);
        }

        if (textoCodigoNoBotaoDificil == null)
        {
            GameObject textoGo = new GameObject("TextoCodigoDificil", typeof(RectTransform));
            textoGo.transform.SetParent(buttonQuizzDificil.transform, false);

            RectTransform rect = textoGo.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            TextMeshProUGUI texto = textoGo.AddComponent<TextMeshProUGUI>();
            texto.alignment = TextAlignmentOptions.Center;
            texto.fontSize = 14f;
            texto.color = Color.white;
            texto.raycastTarget = false;

            textoCodigoNoBotaoDificil = texto;
        }

        textoCodigoNoBotaoDificil.text = codigoDesativacao;
    }

    private void GarantirUIResultados()
    {
        if (modalResultados == null)
        {
            return;
        }

        VincularTextosResultados();
        TMP_FontAsset fonteResultado = ObterFonteResultados();

        float posicaoX = 0f;
        float posicaoYInicial = 85f;
        float espacamentoY = 28f;

        if (textoSequenciaAcertos == null)
        {
            textoSequenciaAcertos = CriarTextoResultado(modalResultados.transform, "TextoSequenciaAcertos", new Vector2(posicaoX, posicaoYInicial), fonteResultado);
        }

        if (textoQuantidadeErros == null)
        {
            textoQuantidadeErros = CriarTextoResultado(modalResultados.transform, "TextoQuantidadeErros", new Vector2(posicaoX, posicaoYInicial - espacamentoY), fonteResultado);
        }

        if (textoMelhorSequenciaAcertos == null)
        {
            textoMelhorSequenciaAcertos = CriarTextoResultado(modalResultados.transform, "TextoMelhorSequenciaAcertos", new Vector2(posicaoX, posicaoYInicial - (espacamentoY * 2f)), fonteResultado);
        }

        if (textoMelhorTempoQuiz == null)
        {
            textoMelhorTempoQuiz = CriarTextoResultado(modalResultados.transform, "TextoMelhorTempoQuiz", new Vector2(posicaoX, posicaoYInicial - (espacamentoY * 3f)), fonteResultado);
        }

        if (textoTempoTotalGasto == null)
        {
            textoTempoTotalGasto = CriarTextoResultado(modalResultados.transform, "TextoTempoTotalGasto", new Vector2(posicaoX, posicaoYInicial - (espacamentoY * 4f)), fonteResultado);
        }

        if (textoMenorQuantidadeErrosNivel == null)
        {
            textoMenorQuantidadeErrosNivel = CriarTextoResultado(modalResultados.transform, "TextoMenorQuantidadeErrosNivel", new Vector2(posicaoX, posicaoYInicial - (espacamentoY * 5f)), fonteResultado);
        }

        DefinirCorTextosResultado(Color.white);
        AtualizarTextosResultados();
    }

    private void DefinirCorTextosResultado(Color cor)
    {
        if (textoSequenciaAcertos != null) textoSequenciaAcertos.color = cor;
        if (textoQuantidadeErros != null) textoQuantidadeErros.color = cor;
        if (textoMelhorSequenciaAcertos != null) textoMelhorSequenciaAcertos.color = cor;
        if (textoMelhorTempoQuiz != null) textoMelhorTempoQuiz.color = cor;
        if (textoTempoTotalGasto != null) textoTempoTotalGasto.color = cor;
        if (textoMenorQuantidadeErrosNivel != null) textoMenorQuantidadeErrosNivel.color = cor;
    }

    private TMP_FontAsset ObterFonteResultados()
    {
        if (pergunta != null && pergunta.font != null)
        {
            return pergunta.font;
        }

        if (alternativa_a != null && alternativa_a.font != null)
        {
            return alternativa_a.font;
        }

        if (alternativa_b != null && alternativa_b.font != null)
        {
            return alternativa_b.font;
        }

        return null;
    }

    private static TMP_Text CriarTextoResultado(Transform parent, string nome, Vector2 posicao, TMP_FontAsset fonte)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicao;
        rect.sizeDelta = new Vector2(245f, 24f);

        TextMeshProUGUI texto = go.AddComponent<TextMeshProUGUI>();
        texto.text = string.Empty;
        texto.fontSize = 11f;
        if (fonte != null)
        {
            texto.font = fonte;
        }
        texto.color = Color.white;
        texto.alignment = TextAlignmentOptions.Center;
        texto.raycastTarget = false;

        return texto;
    }

    private void GarantirUIDeactivate()
    {
        if (modalDeactivate == null)
        {
            return;
        }

        VincularInputCodigoDeactivateSeNulo();
        if (inputCodigoDeactivate == null)
        {
            Debug.LogError("Nao foi encontrado TMP_InputField 'InputCodigo' em ModalDeactivate.");
        }

        if (feedbackCodigoDeactivate == null)
        {
            feedbackCodigoDeactivate = CriarTextoFeedbackDeactivate(modalDeactivate.transform);
        }
    }

    private static TMP_Text CriarTextoFeedbackDeactivate(Transform parent)
    {
        GameObject go = new GameObject("TextoFeedbackDeactivate", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 5f);
        rect.sizeDelta = new Vector2(340f, 28f);

        TextMeshProUGUI texto = go.GetComponent<TextMeshProUGUI>();
        texto.text = string.Empty;
        texto.fontSize = 18f;
        texto.color = new Color(1f, 0.3f, 0.3f, 1f);
        texto.alignment = TextAlignmentOptions.Center;
        texto.raycastTarget = false;

        return texto;
    }

    private void PreencherBancosPadraoSeVazio()
    {
        if (perguntasFacil.Count == 0)
        {
            perguntasFacil.Add(new PerguntaData
            {
                enunciado = "Se você está participando de uma corrida e ultrapassa o segundo colocado, em que posição você fica?",
                alternativaA = "Primeiro",
                alternativaB = "Segundo",
                alternativaC = "Terceiro",
                alternativaD = "Último",
                indiceCorreto = 1
            });

            perguntasFacil.Add(new PerguntaData
            {
                enunciado = "Quantos meses têm 28 dias?",
                alternativaA = "1",
                alternativaB = "6",
                alternativaC = "12",
                alternativaD = "2",
                indiceCorreto = 2
            });

            perguntasFacil.Add(new PerguntaData
            {
                enunciado = "Um avião cai na fronteira entre dois países. Onde os sobreviventes são enterrados?",
                alternativaA = "No país A",
                alternativaB = "No país B",
                alternativaC = "Na fronteira",
                alternativaD = "Em lugar nenhum",
                indiceCorreto = 3
            });

            perguntasFacil.Add(new PerguntaData
            {
                enunciado = "O que pesa mais: 1kg de ferro ou 1kg de algodão?",
                alternativaA = "Ferro",
                alternativaB = "Algodão",
                alternativaC = "Os dois pesam igual",
                alternativaD = "Depende",
                indiceCorreto = 2
            });

            perguntasFacil.Add(new PerguntaData
            {
                enunciado = "Você tem 10 maçãs e pega 3. Com quantas fica?",
                alternativaA = "7",
                alternativaB = "3",
                alternativaC = "10",
                alternativaD = "13",
                indiceCorreto = 1
            });
        }

        if (perguntasMedio.Count == 0)
        {
            perguntasMedio.Add(new PerguntaData
            {
                enunciado = "Se um médico te dá 3 comprimidos e manda tomar um a cada 30 minutos, em quanto tempo você toma todos?",
                alternativaA = "30 minutos",
                alternativaB = "1 hora",
                alternativaC = "1h30",
                alternativaD = "2 horas",
                indiceCorreto = 1
            });

            perguntasMedio.Add(new PerguntaData
            {
                enunciado = "Quantos animais de cada espécie Moisés colocou na arca?",
                alternativaA = "2",
                alternativaB = "4",
                alternativaC = "1",
                alternativaD = "Nenhum",
                indiceCorreto = 3
            });

            perguntasMedio.Add(new PerguntaData
            {
                enunciado = "Qual palavra está escrita errado no dicionário?",
                alternativaA = "Errado",
                alternativaB = "Nenhuma",
                alternativaC = "Todas",
                alternativaD = "A palavra \"errado\"",
                indiceCorreto = 3
            });

            perguntasMedio.Add(new PerguntaData
            {
                enunciado = "Se você tem fósforos e entra num quarto escuro com uma vela, um lampião e uma lareira, o que você acende primeiro?",
                alternativaA = "Vela",
                alternativaB = "Lampião",
                alternativaC = "Lareira",
                alternativaD = "Fósforo",
                indiceCorreto = 3
            });

            perguntasMedio.Add(new PerguntaData
            {
                enunciado = "Se você joga uma pedra azul no mar vermelho, o que acontece?",
                alternativaA = "Afunda",
                alternativaB = "Fica azul",
                alternativaC = "Vira vermelha",
                alternativaD = "Flutua",
                indiceCorreto = 0
            });
        }

        if (perguntasDificil.Count == 0)
        {
            perguntasDificil.Add(new PerguntaData
            {
                enunciado = "O pai de Maria tem 5 filhas: Nana, Nene, Nini, Nono e...?",
                alternativaA = "Nunu",
                alternativaB = "Nane",
                alternativaC = "Maria",
                alternativaD = "Nenhuma",
                indiceCorreto = 2
            });

            perguntasDificil.Add(new PerguntaData
            {
                enunciado = "Se você tem uma vela e apaga ela, para onde vai a luz?",
                alternativaA = "Some",
                alternativaB = "Fica no ar",
                alternativaC = "Vai para outro lugar",
                alternativaD = "Não existe",
                indiceCorreto = 0
            });

            perguntasDificil.Add(new PerguntaData
            {
                enunciado = "Um homem estava andando na chuva sem guarda-chuva nem chapéu, mas nenhum fio de cabelo ficou molhado. Como?",
                alternativaA = "Corria muito rápido",
                alternativaB = "Estava dentro de casa",
                alternativaC = "Era careca",
                alternativaD = "Chuva fraca",
                indiceCorreto = 2
            });

            perguntasDificil.Add(new PerguntaData
            {
                enunciado = "Se um galo bota um ovo em cima do telhado inclinado, para que lado o ovo cai?",
                alternativaA = "Esquerda",
                alternativaB = "Direita",
                alternativaC = "Para baixo",
                alternativaD = "Nenhum",
                indiceCorreto = 3
            });

            perguntasDificil.Add(new PerguntaData
            {
                enunciado = "Um trem elétrico está indo para o norte a 100 km/h e o vento sopra para o sul. Para onde vai a fumaça?",
                alternativaA = "Norte",
                alternativaB = "Sul",
                alternativaC = "Para os lados",
                alternativaD = "Não há fumaça",
                indiceCorreto = 3
            });
        }
    }

    private bool InteracaoBloqueada()
    {
        return Time.unscaledTime < bloqueioInteracaoAte;
    }

    private void BloquearInteracaoPor(float duracao)
    {
        bloqueioInteracaoAte = Time.unscaledTime + Mathf.Max(0f, duracao);
    }
}
