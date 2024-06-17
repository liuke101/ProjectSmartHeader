using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace INab.WorldScanFX.URP2022
{
	
	public class ScanFXFeature : ScriptableRendererFeature
	{
		public Material scanMaterial;
        public RenderPassEvent Event = RenderPassEvent.BeforeRenderingTransparents;

        private ScanFXPass scanFXPass;

        public override void Create()
		{

            scanFXPass = new ScanFXPass(Event, scanMaterial);
		}
        public override void SetupRenderPasses(ScriptableRenderer renderer,
                                      in RenderingData renderingData)
        {
            // The target is used after allocation
            scanFXPass.SetCameraTarget(renderer.cameraColorTargetHandle);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			if (renderingData.cameraData.cameraType != CameraType.Game && renderingData.cameraData.cameraType != CameraType.SceneView) return;
            
			scanFXPass.Setup();
			renderer.EnqueuePass(scanFXPass);
		}

		public class ScanFXPass : ScriptableRenderPass
		{
			public Material scanMaterial;

            private RTHandle cameraColorTarget;
            private RTHandle m_TemporaryColorTexture;

            public ScanFXPass(RenderPassEvent renderPassEvent,Material scanMaterial)
			{
				this.renderPassEvent = renderPassEvent;
                this.scanMaterial = scanMaterial;

                m_TemporaryColorTexture = RTHandles.Alloc("_TemporaryColorTexture", name: "_TemporaryColorTexture");
            }

            public void Setup()
			{
				ConfigureInput(ScriptableRenderPassInput.Normal);
			}

            public void SetCameraTarget(RTHandle cameraColorTarget)
            {
                this.cameraColorTarget = cameraColorTarget;
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                RenderTextureDescriptor textureDescriptor = renderingData.cameraData.cameraTargetDescriptor;
                textureDescriptor.msaaSamples = 1;
                textureDescriptor.depthBufferBits = 0; // 32 bits breaks everytinhg

                RenderingUtils.ReAllocateIfNeeded(ref m_TemporaryColorTexture, textureDescriptor, FilterMode.Point, name: "_TemporaryRT");
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
			{
				CommandBuffer cmd = CommandBufferPool.Get("World Scan FX");

                Shader.SetGlobalMatrix("_InverseView", renderingData.cameraData.camera.cameraToWorldMatrix);

                Blitter.BlitCameraTexture(cmd, cameraColorTarget, m_TemporaryColorTexture, scanMaterial, 0);
                Blitter.BlitCameraTexture(cmd, m_TemporaryColorTexture, cameraColorTarget);

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
			}

			public override void FrameCleanup(CommandBuffer cmd)
			{
                cmd.ReleaseTemporaryRT(Shader.PropertyToID(m_TemporaryColorTexture.name));
            }
        }
	}
}