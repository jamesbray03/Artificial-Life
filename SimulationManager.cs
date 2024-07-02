using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.InteropServices;
using System.Diagnostics;

public class SimulationManager : MonoBehaviour
{
    #region parameters
    public ComputeShader cellularAutomataShader;

    [Header("Resolution")]
    public int resolution;
    public int kernelResolution;

    [Header("Outputs")]
    public RawImage display;
    public RawImage kernel;
    public Material gradientMat;

    [Header("Toggles")]
    public Toggle autoRun;
    public TMP_Text interestReport;
    public Toggle drawMode;
    public TMP_Text drawReport;

    [Header("Sliders")]
    public SliderManager sliders;
    public Slider brushSize;
    public Slider fps;
    public Slider growthRate;
    public Slider growthMu;
    public Slider growthSigma;
    public Slider kernelSize;
    public Slider kernelMu;
    public Slider kernelSigma;

    [Header("Textures")]
    private RenderTexture currentTexture;
    private RenderTexture nextTexture;
    private RenderTexture kernelTexture;
    private RenderTexture gradientTexture;

    [Header("Kernel Handles")]
    private int initializeHandle;
    private int drawHandle;
    private int updateHandle;
    private int stabilityHandle;

    [Header("Miscellaneous")]
    private int seed;
    private int framesSinceStatic = 0;
    private float kernelWeight;
    private float timeSinceLastUpdate = 0.0f;
    private ComputeBuffer stabilityResultBuffer;
    #endregion


    // called on start
    void Start()
    {
        // set random seed
        seed = (int)DateTime.Now.Ticks;
        cellularAutomataShader.SetInt("seed", seed);

        // create new buffer
        stabilityResultBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Default);

        // find kernel handles
        initializeHandle = cellularAutomataShader.FindKernel("Initialize");
        drawHandle = cellularAutomataShader.FindKernel("Draw");
        updateHandle = cellularAutomataShader.FindKernel("Update");
        stabilityHandle = cellularAutomataShader.FindKernel("CheckStability");

        // set resolution
        cellularAutomataShader.SetInt("resolution", resolution);
        cellularAutomataShader.SetInt("kernelResolution", kernelResolution);

        // create gradient texture
        gradientTexture = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32);
        gradientTexture.Create();

        // initialize
        InitializeSimulation();
    }


    // sets up the simulation
    public void InitializeSimulation()
    {
        // create textures
        currentTexture = CreateTexture(resolution);
        nextTexture = CreateTexture(resolution);

        // set up initial texture
        if (!drawMode.isOn)
        {
            cellularAutomataShader.SetTexture(initializeHandle, "currentTexture", currentTexture);
            cellularAutomataShader.Dispatch(initializeHandle, resolution / 8, resolution / 8, 1);
        }
        display.texture = currentTexture;

        // update kernel texture
        UpdateKernel();
    }


    // updates the kernel texture
    public void UpdateKernel()
    {
        kernelTexture = CreateTexture(kernelResolution);
        Texture2D texture = new Texture2D(kernelResolution, kernelResolution, TextureFormat.RGBA32, false);
        int centre = (kernelResolution - 1) / 2;
        Vector2 centreCoords = new Vector2(centre, centre);
        kernelWeight = 0.0f;

        // fill kernel texture
        for (int i = 0; i < kernelResolution; i++)
        {
            for (int j = 0; j < kernelResolution; j++)
            {
                // calculate distance from center
                float distance = Vector2.Distance(new Vector2(i, j), centreCoords) / kernelSize.value;

                if (distance == 0) { texture.SetPixel(i, j, Color.black); continue; }

                if (distance <= 1)
                {
                    float weight = Mathf.Exp(-Mathf.Pow(distance - kernelMu.value, 2) / (2 * Mathf.Pow(kernelSigma.value, 2)));
                    texture.SetPixel(i, j, new Color(weight, weight, weight, 1));
                    kernelWeight += weight;
                }
                else
                {
                    texture.SetPixel(i, j, Color.black);
                }
            }
        }

        texture.Apply();
        Graphics.Blit(texture, kernelTexture);
        RenderTexture.active = null;
        UnityEngine.Object.Destroy(texture);
        texture = null;
        kernel.texture = kernelTexture;
    }


    void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate >= 1.0f / fps.value)
        {
            // set shader textures and parameters
            cellularAutomataShader.SetTexture(updateHandle, "currentTexture", currentTexture);
            cellularAutomataShader.SetTexture(updateHandle, "nextTexture", nextTexture);
            cellularAutomataShader.SetTexture(updateHandle, "kernelTexture", kernelTexture);

            cellularAutomataShader.SetFloat("rate", growthRate.value);
            cellularAutomataShader.SetFloat("mu", growthMu.value);
            cellularAutomataShader.SetFloat("sigma", growthSigma.value);

            cellularAutomataShader.SetFloat("kernelWeight", kernelWeight);

            // handle drawing mode
            if (drawMode.isOn)
            {
                if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(display.rectTransform, Input.mousePosition, Camera.main))
                    {
                        Vector2 localMousePosition;
                        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(display.rectTransform, Input.mousePosition, Camera.main, out localMousePosition))
                        {
                            // convert local position to pixel coordinates
                            int pixelX = Mathf.FloorToInt((localMousePosition.x + display.rectTransform.rect.width / 2) / display.rectTransform.rect.width * resolution) - 200;
                            int pixelY = Mathf.FloorToInt((localMousePosition.y + display.rectTransform.rect.height / 2) / display.rectTransform.rect.height * resolution);

                            seed = (int)DateTime.Now.Ticks;
                            cellularAutomataShader.SetInt("seed", seed);
                            cellularAutomataShader.SetFloat("drawRadius", brushSize.value);
                            cellularAutomataShader.SetInts("drawCoords", new int[] { pixelX, pixelY });

                            if (Input.GetMouseButton(0))
                            {
                                cellularAutomataShader.SetInt("drawMode", 1);
                                drawReport.text = "(drawing)";
                            }
                            else if (Input.GetMouseButton(1))
                            {
                                cellularAutomataShader.SetInt("drawMode", 0);
                                drawReport.text = "(erasing)";
                            }

                            cellularAutomataShader.SetTexture(drawHandle, "currentTexture", display.texture);
                            cellularAutomataShader.Dispatch(drawHandle, resolution / 8, resolution / 8, 1);
                        }
                    }
                }
                else
                {
                    drawReport.text = "(idle)";
                }
            }

            // dispatch update
            cellularAutomataShader.Dispatch(updateHandle, resolution / 8, resolution / 8, 1);

            // handle auto run mode
            if (autoRun.isOn)
            {
                if (framesSinceStatic > fps.value)
                {
                    interestReport.text = "(idle)";
                }
                else
                {
                    interestReport.text = "(adjusting)";
                }

                int result = CheckIfStable();
                if (result == 2) // dead, restart
                {
                    sliders.Randomise();
                    InitializeSimulation();
                    framesSinceStatic = 0;
                }
                else // continue
                {
                    Graphics.Blit(nextTexture, currentTexture);
                    if (result == 1)
                    {
                        sliders.Nudge();
                        framesSinceStatic = 0;
                    }
                    else
                    {
                        framesSinceStatic++;
                    }
                }
            }
            else
            {
                Graphics.Blit(nextTexture, currentTexture);
            }

            // update gradient texture
            gradientMat.SetTexture("Texture2D_9c9bcc366aac4955aa7e8b770ae4fdd5", currentTexture);
            Graphics.Blit(currentTexture, gradientTexture, gradientMat);
            display.texture = currentTexture;
            timeSinceLastUpdate = 0.0f;
        }
    }


    int CheckIfStable()
    {
        int[] result = new int[1] { 3 };
        stabilityResultBuffer.SetData(result);

        // set up stability check
        cellularAutomataShader.SetBuffer(stabilityHandle, "stabilityResult", stabilityResultBuffer);
        cellularAutomataShader.SetTexture(stabilityHandle, "currentTexture", currentTexture);
        cellularAutomataShader.SetTexture(stabilityHandle, "nextTexture", nextTexture);

        // dispatch stability check
        cellularAutomataShader.Dispatch(stabilityHandle, resolution / 8, resolution / 8, 1);

        // get result
        stabilityResultBuffer.GetData(result);

        return result[0];
    }


    RenderTexture CreateTexture(int size)
    {
        // create render texture
        RenderTexture texture = new RenderTexture(size, size, 0);
        texture.enableRandomWrite = true;
        texture.filterMode = FilterMode.Point;
        texture.Create();
        return texture;
    }


    void OnDestroy()
    {
        // release buffer
        if (stabilityResultBuffer != null)
        {
            stabilityResultBuffer.Release();
            stabilityResultBuffer = null;
        }
    }
}
