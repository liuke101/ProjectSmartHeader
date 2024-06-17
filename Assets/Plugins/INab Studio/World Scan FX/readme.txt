Support: Discord (https://discord.gg/K88zmyuZFD) or izzynab.publisher@gmail.com

Online Documentation: https://inabstudios.gitbook.io/world-scan-fx/
You can find more specific information on the asset functionality and setup process.
 
## Quick Start

### IMPORTING
After downloading the asset, import the appropriate .unitypackage file based on your Unity version and SRP (either Built-in.unitypackage, URP.unitypackage, or URP2022+.unitypackage).
Include the post-processing shader that is used by the asset in the Always Included Shaders tab in Project Settings/Graphics:
Built-in: INab Studio/Post Processing Scan FX
URP: INab Studio/ScanFX PostPrcocess

#### BUILT-IN
1. Make sure your project has Shader Graph installed.
2. Go to the main camera object in your scene.
3. Add the ScanFX.cs script to your camera.
4. Link a game object to the Scan Origin field in the script.
5. Create a new material using the Post Processing Scan FX shader.
6. Assign this new material to the Scan Material field in the ScanFX script. Enable the "Update Scan Material Properties" option.
7. You're now ready to experiment with the scan material properties in the "Testing" settings to see how they affect your scene.

#### URP
1. Attach the ScanFX.cs script to any game object in your scene.
2. Connect a game object to the Scan Origin field within the script.
3. Create a new material with the ScanFX PostProcess shader.
4. Set this new material to the Scan Material field in the ScanFX script and turn on the "Update Scan Material Properties" option.
5. Ensure the URP (Universal Render Pipeline) asset in your project has the depth texture option enabled.
6. Add the ScanFXFeature component to your URP data asset.
7. Assign the scan material you created earlier to the scan material field in the ScanFX feature settings.
8. Now, you're all set to play with the scan material properties under the "Testing" settings and observe the changes in your scene.

If you're unfamiliar with URP Assets or their setup:
- You can use the provided ScanFX URP Asset.
- Change your project's current URP asset through the Project Settings/Graphics tab.
- Lastly, check the Quality settings to ensure the render asset isn't overridden, allowing the ScanFX effects to render as intended.

### ISSUES
If you encounter any difficulties using, implementing, or understanding the asset, please feel free to contact me.
