using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class TractorController : MonoBehaviour
{
    private Vector2 lastUartDirection = Vector2.zero;
    private int lastUartGear = 0;
    private SerialFPGA serialFPGA;

    [Header("Motor y Transmisión")]
    public float velocidadMaxima = 6f;
    public float aceleracion = 2f;
    public float desaceleracion = 4f;
    [Range(0, 20)] public int marchaActual = 0;

    [Header("Información (Solo lectura)")]
    public float velocidadActual = 0f;

    [Header("Sprites")]
    public Sprite downSprite;
    public Sprite upSprite;
    public Sprite leftSprite;
    public Sprite rightSprite;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 moveInput;
    private Vector2 direccionInercia;

    void Awake()
    {
        serialFPGA = SerialFPGA.Instance;
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
        // --- UART INPUT (ONLY ONCE) ---
        if (serialFPGA != null)
        {
            char? uartChar;
            while ((uartChar = serialFPGA.ReadChar()).HasValue)
            {
                char c = char.ToUpper(uartChar.Value);
                Debug.Log($"[UART] Received: {c}");

                switch (c)
                {
                    // Direction
                    case 'L': lastUartDirection = new Vector2(-1f, 0f); break;
                    case 'R': lastUartDirection = new Vector2(1f, 0f); break;
                    case 'U': lastUartDirection = new Vector2(0f, 1f); break;
                    case 'N': lastUartDirection = new Vector2(0f, -1f); break;
                    case 'S': lastUartDirection = Vector2.zero; break;

                    // Gear
                    default:
                        if (c >= '0' && c <= '9')
                        {
                            lastUartGear = c - '0';
                        }
                        else if (c >= 'A' && c <= 'K')
                        {
                            lastUartGear = c - 'A' + 10;
                        }
                        break;
                }
            }
        }

        // Apply gear ONLY when changed
        if (marchaActual != lastUartGear)
        {
            marchaActual = lastUartGear;
            Debug.Log($"[UART] Gear updated: {marchaActual}");
        }

        // Direction from UART
        float x = 0f;
        float y = 0f;

        if (lastUartDirection != Vector2.zero)
        {
            x = lastUartDirection.x;
            y = lastUartDirection.y;
        }

        // Prevent diagonal movement
        if (x != 0f) y = 0f;

        moveInput = new Vector2(x, y).normalized;

        // --- SPEED LIMIT BASED ON GEAR ---
        float limiteMarcha = (velocidadMaxima / 20f) * marchaActual;

        // --- ACCELERATION ---
        if (moveInput != Vector2.zero && marchaActual > 0)
        {
            velocidadActual += aceleracion * Time.deltaTime;

            if (velocidadActual > limiteMarcha)
                velocidadActual = limiteMarcha;

            direccionInercia = moveInput;
        }
        else
        {
            velocidadActual -= desaceleracion * Time.deltaTime;

            if (velocidadActual < 0)
                velocidadActual = 0f;
        }

        // --- SPRITES ---
        if (moveInput.x < 0) sr.sprite = rightSprite;
        else if (moveInput.x > 0) sr.sprite = leftSprite;
        else if (moveInput.y > 0) sr.sprite = upSprite;
        else if (moveInput.y < 0) sr.sprite = downSprite;
    }

    void FixedUpdate()
    {
        Vector2 direccionReal = (moveInput != Vector2.zero) ? moveInput : direccionInercia;
        rb.MovePosition(rb.position + direccionReal * velocidadActual * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Recolectable"))
        {
            Destroy(collision.gameObject);

            if (serialFPGA != null)
            {
                Debug.Log("[UART] Sent: Z");
                serialFPGA.SendChar('Z');
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RecolectarCultivo();
            }
        }
    }
}