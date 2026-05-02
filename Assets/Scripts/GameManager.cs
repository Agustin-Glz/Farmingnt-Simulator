using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Configuración de Juego")]
    public float tiempoInicial = 45f; // 45 seconds per level
    public float multiplicadorPuntaje = 10f;
    public int puntosPorCultivo = 10; 

    [Header("Referencias UI Juego")]
    public TextMeshProUGUI timerText;
    public GameObject losePanel;
    public GameObject victoryPanel;
    public TextMeshProUGUI scoreText;

    [Header("Referencias UI Menú")]
    public GameObject creditosPanel; 
    public GameObject menuInicioPanel; // <-- Para apagar todo el menú a la vez

    private float tiempoRestante;
    private bool juegoTerminado = false;
    private int cultivosAgarrados = 0; 

    void Awake()
    {
        // Singleton para acceder desde otros scripts
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        tiempoRestante = tiempoInicial;
        juegoTerminado = false;
        cultivosAgarrados = 0;

        // Estado inicial de los paneles
        if (losePanel != null) losePanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (creditosPanel != null) creditosPanel.SetActive(false);
        
        // Aseguramos que el menú principal esté visible al inicio
        if (menuInicioPanel != null) menuInicioPanel.SetActive(true);

        Time.timeScale = 1f;
    }

    void Update()
    {
        // Solo corre el tiempo si hay un texto de timer (estamos en el nivel)
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

    public void RecolectarCultivo()
    {
        cultivosAgarrados++;
    }

    // --- FUNCIONES DE NAVEGACIÓN ---
    public void Jugar()
    {
        Time.timeScale = 1f;
        // Send reset counter to FPGA
        var serialFPGA = SerialFPGA.Instance;
        if (serialFPGA != null) serialFPGA.SendChar('Y');
        SceneManager.LoadScene("SampleScene"); // Asegúrate de que el nombre coincida
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

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        // Send reset counter to FPGA
        var serialFPGA = SerialFPGA.Instance;
        if (serialFPGA != null) serialFPGA.SendChar('Y');
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ---> NUEVA FUNCIÓN AGREGADA AQUÍ <---
    public void CargarSiguienteNivel()
    {
        // Esto asegura que el tiempo vuelva a correr normal si pausaste el juego al ganar
        Time.timeScale = 1f; 
        // Send reset counter to FPGA
        var serialFPGA = SerialFPGA.Instance;
        if (serialFPGA != null) serialFPGA.SendChar('Y');
        // Obtiene el número del nivel actual y carga el que sigue en la lista
        int indiceActual = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(indiceActual + 1);
    }

    // --- FUNCIONES DE CRÉDITOS ---
    public void MostrarCreditos()
    {
        if (creditosPanel != null) creditosPanel.SetActive(true);
        if (menuInicioPanel != null) menuInicioPanel.SetActive(false); // Oculta el menú y el logo
    }

    public void OcultarCreditos()
    {
        if (creditosPanel != null) creditosPanel.SetActive(false);
        if (menuInicioPanel != null) menuInicioPanel.SetActive(true); // Muestra el menú y el logo
    }

    // --- LÓGICA DE JUEGO ---
    public void Ganar()
    {
        if (juegoTerminado) return;
        juegoTerminado = true;
        Time.timeScale = 0f;

        int puntosDeCultivos = cultivosAgarrados * puntosPorCultivo;
        int puntosPorTiempo = Mathf.CeilToInt(tiempoRestante * multiplicadorPuntaje);
        int puntajeFinal = puntosDeCultivos + puntosPorTiempo;

        if (scoreText != null) 
        {
            scoreText.text = "Puntaje: " + puntajeFinal;
        }

        if (victoryPanel != null) victoryPanel.SetActive(true);
    }

    public void Perder()
    {
        if (juegoTerminado) return;
        juegoTerminado = true;
        Time.timeScale = 0f;
        if (losePanel != null) losePanel.SetActive(true);
    }
}