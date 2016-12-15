using UnityEngine;
using System.Collections;


public class InstancedIndirectSelection : MonoBehaviour
{	
	public  Camera								m_TargetCamera;
	public	bool								m_ShowRenderTexture;
	public	int									m_DisplayDownScale = 4;
	public	Texture2D							m_OutputTextureTarget;

	private Color								m_LastSelection = new Color(1f,1f,1f,0f); // = 16,777,215 which is deemed as no selection
	private InstancedIndirectSelectionExample	m_InstancedIndirectSelectionExample;
	private RenderTexture						m_SelectorRenderTexture;

	void Awake()
	{
		m_InstancedIndirectSelectionExample = FindObjectOfType<InstancedIndirectSelectionExample>();
		m_OutputTextureTarget				= new Texture2D ( 1, 1, TextureFormat.ARGB32, false);
	}

	void Start ()
	{
		// Debug.Log(m_TargetCamera.cullingMask + "  " + (1 << 8));
	}

	void OnDestroy()
	{
		if ( null != m_SelectorRenderTexture )
		{
			m_SelectorRenderTexture.Release();
			Destroy( m_SelectorRenderTexture );
		}

		if ( null != m_OutputTextureTarget )
		{
			Destroy( m_OutputTextureTarget );
		}
	}


	// Note - outputtexture need only be 1 pixel in size!!
	void LateUpdate()
	{
		if ( Input.GetMouseButtonDown( 0 ) )	StartCoroutine( GetSelection() );	// Single left click to select once
		if ( Input.GetMouseButton( 1 ) )		StartCoroutine( GetSelection() );	// Hold down right mouse to constantly check for selection - illustrates worse case performance
	}

	void OnGUI()
	{
		int decodedID = DecodeFloatRGBA(m_LastSelection);
		string validSelection = decodedID == 16777215 ? "Nothing" : "A valid ID";
		GUI.Label(new Rect(25, Screen.height-48, 512, 30), "Selected " + validSelection + " [ " + decodedID.ToString("N0") + " ]   Color: " + m_LastSelection  );
		
		if( m_ShowRenderTexture && null != m_SelectorRenderTexture) GUI.DrawTexture( new Rect( 0, 0, Screen.width/m_DisplayDownScale, Screen.height/m_DisplayDownScale ), m_SelectorRenderTexture, ScaleMode.ScaleToFit, true );
	}

	IEnumerator GetSelection()
	{
		yield return new WaitForEndOfFrame();

		// Build RenderTexture
		BuildRenderTexture();

		// Cache Camera Values
		int cachedCullingMask			= m_TargetCamera.cullingMask;
		Color cachedBackground			= m_TargetCamera.backgroundColor;

		// Override Camera Values			
		m_TargetCamera.cullingMask		= 256;							// Set the camera cullingMask to only render layer 8 ('Selection')  // 1 << 8; // Layer 8 = 256
		m_TargetCamera.backgroundColor	= new Color(1f, 1f, 1f, 0f);	// Set background color to 16,777,215 which is deemed to be 'empty'

		// Set Camera to RenderTexture
		m_TargetCamera.targetTexture	= m_SelectorRenderTexture;	

		// Issue DrawCommand
		m_InstancedIndirectSelectionExample.RenderSelectionInstances();

		// Render Camera
		m_TargetCamera.Render();
				
		// Restore Camera Values
		m_TargetCamera.targetTexture	= null;
		m_TargetCamera.cullingMask		= cachedCullingMask;
		m_TargetCamera.backgroundColor	= cachedBackground;


		// ReadPixels from RenderTexture
		RenderTexture.active = m_SelectorRenderTexture;

		// Read RenderTexture into 1x1 Pixel Texture - assumed faster than reading back entire RenerTexture?
		m_OutputTextureTarget.ReadPixels( new Rect( (int) Input.mousePosition.x, Screen.height - (int) Input.mousePosition.y, 1, 1 ), 0, 0, false );
		m_OutputTextureTarget.Apply(); // needed?
			
		RenderTexture.active = null;

		// GetPixel value
		m_LastSelection = m_OutputTextureTarget.GetPixel(0,0);
	}


	void BuildRenderTexture()
	{
		if ( null != m_SelectorRenderTexture && m_SelectorRenderTexture.width == Screen.width &&  m_SelectorRenderTexture.height == Screen.height) return;

		if ( null != m_SelectorRenderTexture ) m_SelectorRenderTexture.Release();
		m_SelectorRenderTexture = null;

		m_SelectorRenderTexture						= new RenderTexture( Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32 );
		m_SelectorRenderTexture.filterMode			= FilterMode.Point;
		m_SelectorRenderTexture.antiAliasing		= 1;
		m_SelectorRenderTexture.wrapMode			= TextureWrapMode.Clamp;
		m_SelectorRenderTexture.autoGenerateMips	= false;
		m_SelectorRenderTexture.useMipMap			= false;
		m_SelectorRenderTexture.Create();
	}

	// Note: Not using alpha of RGBA for ID value - this is so the renderTexture can be overlayed on the screen for visual checking.
	// This means max ID is limited to 16,777,214 as 16,777,215 is deemed to be 'empty' or no selection.
	// ID starts at zero.
	int DecodeFloatRGBA( Color encoded )
	{
		return (int)(encoded.r * 255.0f) + ( ( int )( encoded.g * 255.0f ) << 8) + ( ( int )( encoded.b * 255.0f) << 16) ;//+ ((int)( encoded.a * 255.0f ) << 24);
	}

}
