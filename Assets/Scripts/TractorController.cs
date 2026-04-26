using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class TractorController : MonoBehaviour
{
    [Header("Motor y Transmisión")]
    public float velocidadMaxima = 6f; // Velocidad tope en marcha 20
    public float aceleracion = 2f;     // Qué tan rápido gana velocidad al avanzar
    public float desaceleracion = 4f;  // Qué tan rápido se detiene al soltar las teclas
    [Range(0, 20)] public int marchaActual = 0;
    
    [Header("Información (Solo lectura)")]
    public float velocidadActual = 0f; // Para que veas en el Inspector cómo sube tu velocidad

    [Header("Sprites")]
    public Sprite downSprite;
    public Sprite upSprite;
    public Sprite leftSprite;
    public Sprite rightSprite;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 moveInput;
    private Vector2 direccionInercia; // Para que el tractor resbale un poquito al frenar

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (downSprite != null) sr.sprite = downSprite;
        direccionInercia = Vector2.down;
    }

    void Update()
    {
        // --- 1. Límite de la marcha actual ---
        // Calcula cuál es la velocidad máxima permitida en la marcha en la que estás
        float limiteMarcha = (velocidadMaxima / 20f) * marchaActual;

        // --- 2. Lógica de Cambio de Marchas Realista ---
        if (Input.GetKeyDown(KeyCode.E)) 
        {
            // REGLA: Solo te deja subir de marcha si estás en Neutral (0) 
            // O si tu velocidad ya alcanzó al menos el 80% del límite de tu marcha actual.
            if (marchaActual < 20 && (velocidadActual >= limiteMarcha * 0.8f || marchaActual == 0))
            {
                marchaActual++;
                Debug.Log("Subiste a marcha: " + marchaActual);
            }
            else if (marchaActual < 20)
            {
                // Si intentas cambiar antes de tiempo, te avisa en la consola
                Debug.Log("¡Revoluciona más el motor! Vas muy lento para la siguiente marcha.");
            }
        }
        else if (Input.GetKeyDown(KeyCode.Q)) 
        {
            if (marchaActual > 0) 
            {
                marchaActual--;
                Debug.Log("Bajaste a marcha: " + marchaActual);
            }
        }

        // --- 3. Controles de Dirección ---
        float x = 0f;
        float y = 0f;

        if (Input.GetKey(KeyCode.A)) x = -1f;
        if (Input.GetKey(KeyCode.D)) x = 1f;
        if (Input.GetKey(KeyCode.W)) y = 1f;
        if (Input.GetKey(KeyCode.S)) y = -1f;
        
        if (x != 0f) y = 0f;

        moveInput = new Vector2(x, y).normalized;

        // --- 4. Sistema de Aceleración (El "Acelerador" del tractor) ---
        if (moveInput != Vector2.zero && marchaActual > 0)
        {
            // Si estás presionando una tecla, agarra vuelo poco a poco
            velocidadActual += aceleracion * Time.deltaTime;
            
            // Topa la velocidad para que no pase del límite de la marcha actual
            if (velocidadActual > limiteMarcha) velocidadActual = limiteMarcha;
            
            direccionInercia = moveInput; // Guarda hacia dónde iba para frenar con inercia
        }
        else
        {
            // Si sueltas las teclas o estás en neutral, frena poco a poco (freno de motor)
            velocidadActual -= desaceleracion * Time.deltaTime;
            if (velocidadActual < 0) velocidadActual = 0f;
        }

        // --- 5. Sprites visuales ---
        if (moveInput.x < 0) sr.sprite = rightSprite; 
        else if (moveInput.x > 0) sr.sprite = leftSprite;
        else if (moveInput.y > 0) sr.sprite = upSprite;
        else if (moveInput.y < 0) sr.sprite = downSprite;
    }

    void FixedUpdate()
    {
        // Aplica el movimiento. Usa direccionInercia para que se detenga de forma suave y no en seco.
        Vector2 direccionReal = (moveInput != Vector2.zero) ? moveInput : direccionInercia;
        rb.MovePosition(rb.position + direccionReal * velocidadActual * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Recolectable"))
        {
            Destroy(collision.gameObject); 
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RecolectarCultivo();
            }
        }
    }
}