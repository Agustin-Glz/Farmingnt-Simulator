using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class TractorController : MonoBehaviour
{
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
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (downSprite != null)
            sr.sprite = downSprite;
    }

    void Update()
    {
        float x = 0f;
        float y = 0f;

        if (Input.GetKey(KeyCode.A)) x = -1f;
        if (Input.GetKey(KeyCode.D)) x = 1f;
        if (Input.GetKey(KeyCode.W)) y = 1f;
        if (Input.GetKey(KeyCode.S)) y = -1f;

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
}