using UnityEngine;
using System.IO.Ports;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class TractorController : MonoBehaviour
{
    public static TractorController Instance;
    private SerialPort _serialPort;

    public float moveSpeed = 3f;

    public Sprite downSprite;
    public Sprite upSprite;
    public Sprite leftSprite;
    public Sprite rightSprite;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 moveInput;

    void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        try
        {
            // IMPORTANTE: Cambia "COM3" al puerto COM correcto de tu adaptador FTDI
            _serialPort = new SerialPort("COM10", 9600);
            _serialPort.ReadTimeout = 1;
            _serialPort.Open();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error al abrir el puerto serial: " + ex.Message);
        }

        if (downSprite != null)
            sr.sprite = downSprite;
    }

    void Update()
    {
        float x = 0f;
        float y = 0f;

        if (_serialPort != null && _serialPort.IsOpen)
        {
            try
            {
                char command = (char)_serialPort.ReadChar();
                switch (command)
                {
                    case 'W': y = 1f; break;
                    case 'S': y = -1f; break;
                    case 'A': x = -1f; break;
                    case 'D': x = 1f; break;
                }
            }
            catch (System.TimeoutException)
            {
                // Es normal que no lleguen datos en cada frame. No hacer nada.
            }
        }

        // estilo Pokémon: sin diagonal
        if (x != 0f) y = 0f;

        moveInput = new Vector2(x, y).normalized;

        if (x < 0)
            sr.sprite = rightSprite;
        else if (x > 0)
            sr.sprite = leftSprite;
        else if (y > 0)
            sr.sprite = upSprite;
        else if (y < 0)
            sr.sprite = downSprite;
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    public void SendFruitPickupSignal()
    {
        if (_serialPort != null && _serialPort.IsOpen)
        {
            _serialPort.Write("F");
        }
    }

    void OnDestroy()
    {
        if (_serialPort != null && _serialPort.IsOpen)
        {
            _serialPort.Close();
        }
    }
}