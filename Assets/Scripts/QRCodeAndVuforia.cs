using UnityEngine;
using System;
using Vuforia;
using System.Threading;
using ZXing;
using UnityEngine.UI;

public class QRCodeAndVuforia : MonoBehaviour
{
    public GameObject webObject;
    public bool reading;
    public string QRMessage;
    public Text statusText;
    public Text targetNameText;
    public Text scanText;

    private readonly PIXEL_FORMAT m_PixelFormat = PIXEL_FORMAT.GRAYSCALE;
    private bool m_RegisteredFormat = false;
    WebAPIManager webAPIManager;
    Thread qrThread = null;
    private Color32[] c;
    private int W, H;
    Vuforia.Image QCARoutput;
    bool updC;
    string currentQRMessage = "";

    void Start() {
        webAPIManager = webObject.GetComponent<WebAPIManager>();
    }


    void OnEnable() {
        VuforiaARController.Instance.RegisterTrackablesUpdatedCallback(OnTrackablesUpdated);


        var isAutoFocus = CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
        if (!isAutoFocus) {
            CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_NORMAL);
        }

        StartQRCodeScanning();
    }


    void OnDisable() {
        if (qrThread != null) {
            StopQRCodeScanning();
        }
    }
    public void OnTrackablesUpdated() {
        Vuforia.CameraDevice cam = Vuforia.CameraDevice.Instance;

        if (!m_RegisteredFormat) {
            Vuforia.CameraDevice.Instance.SetFrameFormat(m_PixelFormat, true);
            m_RegisteredFormat = true;
        }
        QCARoutput = cam.GetCameraImage(m_PixelFormat);
        if (QCARoutput != null) {
            reading = true;
            updC = true;
        }
        else {
            reading = false;
            Debug.Log(m_PixelFormat + " image is not available yet");
        }
    }

    void Update() {
        CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
        if (reading) {
            if (QCARoutput != null) {
                if (updC) {
                    updC = false;
                    Invoke("ForceUpdateC", 1f);
                    if (QCARoutput == null) {
                        return;
                    }
                    c = null;
                    c = ImageToColor32(QCARoutput);
                    if (W == 0 | H == 0) {
                        W = QCARoutput.BufferWidth;
                        H = QCARoutput.BufferHeight;
                    }
                    QCARoutput = null;
                }
            }

            // If new ar qrcode detected, start ar cloud reco
            if (string.IsNullOrEmpty(currentQRMessage) && !string.IsNullOrEmpty(QRMessage)) {
                currentQRMessage = QRMessage;
                webAPIManager.GetNewImage(QRMessage);
            }
        }

        statusText.text = qrThread != null ? "QR Code scanning" : "QR code not scanning";
        targetNameText.text = string.IsNullOrEmpty(QRMessage) ? "No message" : QRMessage;
    }
    void DecodeQR() {
        var barcodeReader = new BarcodeReader { AutoRotate = false, TryHarder = false };
        while (true) {
            if (reading && c != null) {
                try {
                    ZXing.Result result = barcodeReader.Decode(c, W, H);
                    c = null;
                    if (result != null) {
                        QRMessage = result.Text;
                    }
                }
                catch (Exception e) {
                    //Debug.LogError(e.Message);
                }
            }

        }
    }
    void ForceUpdateC() {
        updC = true;
    }

    Color32[] ImageToColor32(Vuforia.Image a) {
        if (!Vuforia.Image.IsNullOrEmpty(a)) return null;
        Color32[] r = new Color32[a.BufferWidth * a.BufferHeight];
        for (int i = 0; i < r.Length; i++) {
            r[i].r = r[i].g = r[i].b = a.Pixels[i];
        }
        return r;
    }

    public void StartQRCodeScanning() {
        if (qrThread != null) {
            Debug.Log("Already started");
        } else {
            Debug.Log("Start qr code scanning");
            qrThread = new Thread(DecodeQR);
            qrThread.Start();
            QRMessage = "";
            currentQRMessage = "";
            scanText.text = "Scanning...";
        }
    }

    public void StopQRCodeScanning() {
        if (qrThread == null) {
            Debug.Log("No qr code scanning");
        } else {
            Debug.Log("Stop qr code scanning");
            qrThread.Abort();
            qrThread = null;
            scanText.text = "Scan";
        }
    }

    public void ClearMessage() {
        QRMessage = "";
        currentQRMessage = "";
    }
}