# HeightMap Converter (HMCon) Console Application

A modular elevation data conversion tool.

Provides a console application that can convert elevation data from various file types into various formats. Import files, modify their elevation data as needed, then export them as one or more formats listed below. Install optional modules that add support for other formats or custom commands that modify the imported data.

### Supported File Formats / Types

| Format                             | Import             | Export             |
| ---------------------------------- | ------------------ | ------------------ |
| ESRI ASCII Grid                    | :heavy_check_mark: | :heavy_check_mark: |
| ASCII XYZ                          | :x:                | :heavy_check_mark: |
| Raw Data (16/32 Bit)               | :x:                | :heavy_check_mark: |
| AutoCAD DXF (2D & 3D)              | :x:                | :heavy_check_mark: |
| 8 Bit PNG Heightmaps <sup>1</sup>  | :heavy_check_mark: | :heavy_check_mark: |
| 16 Bit PNG Heightmaps <sup>1</sup> | :heavy_check_mark: | :heavy_check_mark: |
| 8 Bit JPG Heightmaps <sup>1</sup>  | :heavy_check_mark: | :heavy_check_mark: |
| 8 Bit TIFHeightmaps <sup>1</sup>   | :heavy_check_mark: | :heavy_check_mark: |
| PNG Normal Maps <sup>1</sup>       | :x:                | :heavy_check_mark: |
| PNG Hillshade Maps <sup>1</sup>    | :x:                | :heavy_check_mark: |
| 3DS 3D Models <sup>2</sup>         | :x:                | (WIP)              |
| FBX 3D Models <sup>2</sup>         | :x:                | (WIP)              |
| Minecraft Region File <sup>3</sup> | :heavy_check_mark: | :heavy_check_mark: |
| Minecraft World File <sup>3</sup>  | :x:                | :heavy_check_mark: |

<sub>1 - Requires HMConImage Module</sub>
<sub>2 - Requires HMCon3D Module</sub>
<sub>3 - Requires HMConMC Module</sub>

### Notable Features (so far)

- Convert any importable format into any other supported format.
- Basic commands for modifying the imported elevation data, such as cropping, resizing, or subsampling operations.
- Sequentially export to multiple formats at once.
- Generate hillshade maps for a visual representation of the terrain data.
- Specifically designed with CAD / 3D Modelling / Game Development in mind.
- Create labeled point grids as 2-Dimensional DXF files for CAD applications.
- Pregenerate entire playable Minecraft worlds with customizable world gen instructions and weightmaps for biome / feature distribution control.
- Modular Plug-In system for additional file format support or adding custom commands.

### How to use:

- Download the latest release (TO DO)

- (Optional) Add modules by placing the DLL files next to the executable.

- Run the .exe and type (or drag and drop into the console window if your OS supports it) the path of the file you want to import.

- (Optional) Modify the data using the various `modify ...` commands.

- Define one or more target formats with the `format ...` command.

- Type `export` then type the file path where you want to save the file(s).

- That's all there is to it!
