# MDF-Manager
A C# WPF GUI application to edit RE Engine material files (*.mdf2)

# Usage
Open a compatible mdf file by clicking File -> Open, or by simply dropping the file onto the program. You can save the currently selected mdf by pressing File -> Save,
or File -> Save As to save it under a different name. 

To add an mdf to the library, click the "Add" button above the Library view and then select the mdf. You can add materials from mdfs in the library to currently opened 
mdf by clicking on the arrow to the side of the mdf path in the library, then dragging the material over to the opened material. You can remove mdfs from the library by 
selecting them in the Library view and then clicking the "Remove" button. 

Note that this program cannot add new texture bindings or properties to materials, as these are determined by the shader (*.mmtr) and are effectively immutable without 
knowledge of shader coding.

# Installation
Just download the most recent release from the sidebar to the right. If you would like to build the project, open the solution in Visual Studio, right click on the 
MDF-Manager project and select "Restore NuGet Packages", then build the project.

# Credits
* **Che, Darkness, and alphaZomega** - for MDF structure documentation
* **AsteriskAmpersand** - his mrl3 editor served as a general inspiration for the layout and library functionality
