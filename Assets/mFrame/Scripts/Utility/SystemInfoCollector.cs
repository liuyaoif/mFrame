using UnityEngine;

namespace mFrame.Utility
{
    public class SystemInfoCollector : MonoBehaviour
    {

        void Start()
        {
            string content = null;
            content += " graphicsMultiThreaded : " + SystemInfo.graphicsMultiThreaded + "\n\n";
            content += " supports3DTextures : " + SystemInfo.supports3DTextures + "\n\n";
            content += " supportsAccelerometer : " + SystemInfo.supportsAccelerometer + "\n\n";
            content += " supportsComputeShaders : " + SystemInfo.supportsComputeShaders + "\n\n";
            content += " supportsGyroscope : " + SystemInfo.supportsGyroscope + "\n\n";
            content += " supportsImageEffects : " + SystemInfo.supportsImageEffects + "\n\n";
            content += " supportsInstancing : " + SystemInfo.supportsInstancing + "\n\n";
            content += " supportsLocationService : " + SystemInfo.supportsLocationService + "\n\n";
            content += " supportsRawShadowDepthSampling : " + SystemInfo.supportsRawShadowDepthSampling + "\n\n";
            content += " supportsRenderTextures : " + SystemInfo.supportsRenderTextures + "\n\n";
            content += " supportsRenderToCubemap : " + SystemInfo.supportsRenderToCubemap + "\n\n";
            content += " supportsShadows : " + SystemInfo.supportsShadows + "\n\n";
            content += " supportsSparseTextures : " + SystemInfo.supportsSparseTextures + "\n\n";
            //content +=" supportsVertexPrograms : " + SystemInfo.supportsVertexPrograms + "\n\n";
            content += " supportsVibration : " + SystemInfo.supportsVibration + "\n\n";
            content += " deviceType : " + SystemInfo.deviceType + "\n\n";
            content += " graphicsDeviceType : " + SystemInfo.graphicsDeviceType + "\n\n";
            content += " graphicsDeviceID : " + SystemInfo.graphicsDeviceID + "\n\n";
            content += " graphicsDeviceVendorID : " + SystemInfo.graphicsDeviceVendorID + "\n\n";
            content += " graphicsMemorySize : " + SystemInfo.graphicsMemorySize + "\n\n";
            //content +=" graphicsPixelFillrate : " + SystemInfo.graphicsPixelFillrate + "\n\n";
            content += " graphicsShaderLevel : " + SystemInfo.graphicsShaderLevel + "\n\n";
            content += " maxTextureSize : " + SystemInfo.maxTextureSize + "\n\n";
            content += " processorCount : " + SystemInfo.processorCount + "\n\n";
            content += " processorFrequency : " + SystemInfo.processorFrequency + "\n\n";
            content += " supportedRenderTargetCount : " + SystemInfo.supportedRenderTargetCount + "\n\n";
            content += " supportsStencil : " + SystemInfo.supportsStencil + "\n\n";
            content += " systemMemorySize : " + SystemInfo.systemMemorySize + "\n\n";
            content += " npotSupport : " + SystemInfo.npotSupport + "\n\n";
            content += " deviceModel : " + SystemInfo.deviceModel + "\n\n";
            content += " deviceName : " + SystemInfo.deviceName + "\n\n";
            content += " deviceUniqueIdentifier : " + SystemInfo.deviceUniqueIdentifier + "\n\n";
            content += " graphicsDeviceName : " + SystemInfo.graphicsDeviceName + "\n\n";
            content += " graphicsDeviceVendor : " + SystemInfo.graphicsDeviceVendor + "\n\n";
            content += " graphicsDeviceVersion : " + SystemInfo.graphicsDeviceVersion + "\n\n";
            content += " operatingSystem : " + SystemInfo.operatingSystem + "\n\n";
            content += " processorType : " + SystemInfo.processorType + "\n\n";
        }
    }
}