using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Configuración de Juego")]
    public float tiempoInicial = 30f;
    public float multiplicadorPuntaje = 10f;

    [Header("Referencias UI Juego")]
    public TextMeshProUGUI timerText;
    public GameObject losePanel;
    public GameObject victoryPanel;
    public TextMeshProUGUI scoreText;

    [Header("Referencias UI Menú")]
    // --- ESTA ES LA LÍNEA QUE TE FALTA ---
    public GameObject creditosPanel; 

    private float tiempoRestante;
    private bool juegoTerminado = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        tiempoRestante = tiempoInicial;
        juegoTerminado = false;

        // Desactivar paneles al iniciar
        if (losePanel != null) losePanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (creditosPanel != null) creditosPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    void Update()
    {
        // Solo corre el timer si hay un texto asignado (para evitar errores en el Menú)
        if (juegoTerminado || timerText == null) return; 

        tiempoRestante -= Time.deltaTime;

        if (tiempoRestante <= 0f)
        {
            tiempoRestante = 0f;
            Perder();
            return;
        }

        timerText.text = "Tiempo: " + Mathf.CeilToInt(tiempoRestante);
    }

    // --- FUNCIONES DE NAVEGACIÓN ---
    public void Jugar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene");
    }

    public void IrAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    public void SalirJuego()
    {
        Application.Quit();
        Debug.Log("Saliendo del juego...");
    }

    // --- FUNCIONES DE CRÉDITOS ---
    public void MostrarCreditos()
    {
        if (creditosPanel != null) creditosPanel.SetActive(true);
    }

    public void OcultarCreditos()
    {
        if (creditosPanel != null) creditosPanel.SetActive(false);
    }

    // --- LÓGICA DE JUEGO ---
    public void Ganar()
    {
        if (juegoTerminado) return;
        juegoTerminado = true;
        Time.timeScale = 0f;
        int puntajeFinal = Mathf.CeilToInt(tiempoRestante * multiplicadorPuntaje);
        if (scoreText != null) scoreText.text = "Puntaje: " + puntajeFinal;
        if (victoryPanel != null) victoryPanel.SetActive(true);
    }

    public void Perder()
    {
        if (juegoTerminado) return;
        juegoTerminado = true;
        Time.timeScale = 0f;
        if (losePanel != null) losePanel.SetActive(true);
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}