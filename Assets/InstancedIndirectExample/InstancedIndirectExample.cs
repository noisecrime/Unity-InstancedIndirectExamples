// https://docs.unity3d.com/560/Documentation/ScriptReference/Graphics.DrawMeshInstancedIndirect.html

using UnityEngine;
 
public class InstancedIndirectExample : MonoBehaviour
{ 
    public int instanceCount = 100000;
    public Mesh instanceMesh;
    public Material instanceMaterial;
 
    private int cachedInstanceCount = -1;
    private ComputeBuffer positionBuffer;
    private ComputeBuffer argsBuffer;
	private ComputeBuffer colorBuffer;

    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
 
    void Start()
	{
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        UpdateBuffers();
    }

    void Update()
	{ 
        // Update starting position buffer
        if (cachedInstanceCount != instanceCount) UpdateBuffers();

        // Pad input
        if (Input.GetAxisRaw("Horizontal") != 0.0f) instanceCount = (int)Mathf.Clamp(instanceCount + Input.GetAxis("Horizontal") * 40000, 1.0f, 5000000.0f);
 
        // Render
      //  instanceMaterial.SetBuffer("positionBuffer", positionBuffer);
        Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, instanceMaterial, new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), argsBuffer);
    }
 
    void OnGUI()
	{ 
        GUI.Label(new Rect(265, 12, 200, 30), "Instance Count: " + instanceCount.ToString("N0"));
        instanceCount = (int)GUI.HorizontalSlider(new Rect(25, 20, 200, 30), (float)instanceCount, 1.0f, 5000000.0f);
    }
 
    void UpdateBuffers()
	{ 
		if ( instanceCount < 1 ) instanceCount = 1;

        // Positions & Colors
        if (positionBuffer != null) positionBuffer.Release();
		if (colorBuffer != null) colorBuffer.Release();

        positionBuffer	= new ComputeBuffer(instanceCount, 16);
		colorBuffer		= new ComputeBuffer(instanceCount, 4*4);

        Vector4[] positions = new Vector4[instanceCount];
		Vector4[] colors	= new Vector4[instanceCount];

        for (int i=0; i < instanceCount; i++)
		{
            float angle = Random.Range(0.0f, Mathf.PI * 2.0f);
            float distance = Random.Range(20.0f, 100.0f);
            float height = Random.Range(-2.0f, 2.0f);
            float size = Random.Range(0.05f, 0.25f);
            positions[i]	= new Vector4(Mathf.Sin(angle) * distance, height, Mathf.Cos(angle) * distance, size);
			colors[i]		= new Vector4( Random.value, Random.value, Random.value, 1f );
        }

        positionBuffer.SetData(positions);
		colorBuffer.SetData(colors);

		instanceMaterial.SetBuffer("positionBuffer", positionBuffer);
		instanceMaterial.SetBuffer("colorBuffer", colorBuffer);

        // indirect args
        uint numIndices = (instanceMesh != null) ? (uint)instanceMesh.GetIndexCount(0) : 0;
        args[0] = numIndices;
        args[1] = (uint)instanceCount;
        argsBuffer.SetData(args);
 
        cachedInstanceCount = instanceCount;
    }

    void OnDisable()
	{
        if (positionBuffer != null) positionBuffer.Release();
        positionBuffer = null;

		if (colorBuffer != null) colorBuffer.Release();
        colorBuffer = null;

        if (argsBuffer != null) argsBuffer.Release();
        argsBuffer = null;
    }
}
