Shader "Custom/DisableZ"
{ 
	SubShader{

		 Tags{
		"RenderType" = "Opaque"
		}


		Pass{
		      Zwrite Off
		}

	}

}

