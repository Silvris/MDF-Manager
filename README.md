# MDF-Manager
A C# WPF GUI application to edit RE Engine material files (*.mdf2)

# Usage
Open a compatible mdf file by clicking File -> Open, or by simply dropping the file onto the program. You can save the currently selected mdf by pressing File -> Save,
or File -> Save As to save it under a different name. 

The Library is a list of mdfs from which materials can be pulled from and added to opened mdfs. To add an mdf to the library, click the "Add" button above the Library view and then select the mdf. You can add materials from mdfs in the library to currently opened mdf by clicking on the arrow to the side of the mdf path in the library, then dragging the material over to the opened material. You can remove mdfs from the library by selecting them in the Library view and then clicking the "Remove" button. 

The Compendium is a complete list of mdfs within specified directories, filtered by their version and then the MMTR used. These mdf entries can then be dragged to be added to the library or directly opened within the editor. "Rebase Compendium" will overwrite the current compendium entries with the entries generated from the given directory, while "Expand Compendium" will add all entries from the given directory to the current compendium.

The Batch Converter found under File -> Batch Convert can be used to convert MDFs from one format (such as RE2) to another (RERT). 
**NOTE: Conversion will only work if the underlying MMTR remain relatively unchanged. If new parameters were added to the MMTR, you *will* need to recreate the MDF.**

Note that this program cannot add new texture bindings or properties to materials, as these are determined by the shader (*.mmtr) and are effectively immutable without 
knowledge of shader coding.

The color of certain UI elements can be changed by clicking Theme at the top of the program.

# Installation
Just download the most recent release from the sidebar to the right. If you would like to build the project, open the solution in Visual Studio, right click on the 
MDF-Manager project and select "Restore NuGet Packages", then build the project.

# Credits
* **Che, Darkness, and alphaZomega** - for MDF structure documentation
* **AsteriskAmpersand** - his mrl3 editor served as a general inspiration for the layout and library/compendium functionality
