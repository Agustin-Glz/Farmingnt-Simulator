using System.IO.Ports;
using UnityEngine;

public class SerialFPGA : MonoBehaviour
{
    private System.Collections.Generic.Queue<char> charQueue = new System.Collections.Generic.Queue<char>();

    void Update()
    {
        // Read all available chars and enqueue them
        if (serialPort != null && serialPort.IsOpen)
        {
            while (serialPort.BytesToRead > 0)
            {
                char c = (char)serialPort.ReadChar();
                Debug.Log($"[SerialFPGA] Received: {c} (ASCII {(int)c})");
                charQueue.Enqueue(c);
            }
        }
    }
    public string portName = "COM10";
    public int baudRate = 115200;
    private SerialPort serialPort;

    public static SerialFPGA Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            serialPort = new SerialPort(portName, baudRate);
            try
            {
                serialPort.Open();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"No se pudo abrir el puerto serial: {e.Message}");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SendChar(char c)
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            Debug.Log($"[SerialFPGA] SerialPort.Write sending: {c} (ASCII {(int)c})");
            serialPort.Write(c.ToString());
            serialPort.BaseStream.Flush();
        }
    }

    public char? ReadChar()
    {
        if (charQueue.Count > 0)
        {
            return charQueue.Dequeue();
        }
        return null;
    }

    void OnDestroy()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
        }
    }
}
