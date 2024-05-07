# ProjectLocker v4.2
Battlefield 4 Simple External Cheat. It works Offline (Campaign) and Online.
Total conversion to C# of [TheKawaiiNeko](https://github.com/TheKawaiiNeko) project written in [C++](https://github.com/TheKawaiiNeko/BF4-External)  

## Features
### AimBot
* No aim prediction、VehicleAimBot. among other things.
* Optimized, although code cleaning and restructuring is needed.
* This uses the mouse_event WinAPI.
#### ESP
* Soldier ESP : 2DBox, BoxFilled, Line, HealthBar, Distance, Name, VisCheck
* Vehicle ESP : 2D Box
#### Misc
* StreamProof
* Spectator NameList
* NoRecoil/Spread
* DamageHack
* UnlockAll
* ~~Config Load/Save~~  ->  Not implemented at the moment.
* "偉大なるナチズム国家"への"忠誠" -> No idea, written by the original creator of the project.

## Q&A
Q. PBSS Bypassはしてあるのか？  
A. It's an external hack, so it's basically unnecessary. In fact, I've been unbanned for almost 100 hours without being detected。

Q. Detect?  
A. BF4 anti-cheat performs Sig-based program detection, so rewrite the code or use some Dotnet protector/packer, such as ConfuserEx / VMProtect or Enigma Protector to bypass Sig-based detection。  

## Preview
![Captura de pantalla 2024-04-14 194349](https://github.com/DestroyerDarkNess/BF4-External/assets/32405118/4e945efa-b060-439b-8ab4-925170de89f6)
![Captura de pantalla 2024-04-16 210240](https://github.com/DestroyerDarkNess/BF4-External/assets/32405118/c736599d-c9eb-4342-b6c1-5a841391d356)

<center>
<h2> 🛠️ Resources </h2>
</center>

- [EasyImGui](https://github.com/DestroyerDarkNess/EasyImGui) - Use ImGui with Winforms / .NET Framework.
- [RenderSpy.Imgui](https://www.nuget.org/packages/RenderSpy.ImGui) - NuGet package that includes [DearImguiSharp](https://github.com/Sewer56/DearImguiSharp) for the .NET Framework.
- [RenderSpy](https://github.com/DestroyerDarkNess/RenderSpy) - Universal graphical and input hook for a D3D9-D3D12, OpenGL and Vulkan based games.
- [Themer](https://github.com/arshx86/Themer) - Various blur effects for WinForms.
