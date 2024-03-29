<img align="left" width="128" alt="Logo" src="images/logo_base.png">
<br/>

# TerrainFactory

A modular elevation data conversion tool.

Provides a console application that can convert elevation data from various file types into multiple different formats. Import files, modify their elevation data as needed, then export them as one or more formats listed below. Install optional modules that add support for other formats or custom commands that modify the imported data.

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
| 8 Bit TIF Heightmaps <sup>1</sup>  | :heavy_check_mark: | :heavy_check_mark: |
| PNG Normal Maps <sup>1</sup>       | :x:                | :heavy_check_mark: |
| PNG Hillshade Maps <sup>1</sup>    | :x:                | :heavy_check_mark: |
| 3DS 3D Models <sup>2</sup>         | :x:                | (WIP)              |
| FBX 3D Models <sup>2</sup>         | :x:                | (WIP)              |
| Minecraft Region File <sup>3</sup> | :heavy_check_mark: | :heavy_check_mark: |
| Minecraft World File <sup>3</sup>  | :x:                | :heavy_check_mark: |

<sub>1 - [Requires TerrainFactory Image Module](#terrainfactory---image-module)</sub><br/>
<sub>2 - [Requires TerrainFactory 3D Module](#terrainfactory---3d-module)</sub><br/>
<sub>3 - [Requires TerrainFactory MC Module](#terrainfactory---minecraft-module)</sub><br/>

### Features

- Convert any importable format into any other supported format.
- Basic commands for modifying the imported elevation data, such as cropping, resizing, or subsampling operations.
- Sequentially export to multiple formats at once.
- Generate hillshade maps for a visual representation of the terrain data.
- Create labeled point grids as 2-Dimensional DXF files for CAD applications.
- Pregenerate entire playable Minecraft worlds with customizable world gen instructions and weightmaps for biome / feature distribution control.
- Modular Plug-In system for additional file format support or adding custom commands.
- Specifically designed with CAD / 3D Modelling / Game Dev in mind.

### How to use

- Download the latest release (TO DO)
- (Optional) Add modules by placing the corresponding module files next to the executable.
- Run the .exe and type (or drag and drop into the console window if your OS supports it) the path of the file you want to import.
- (Optional) Modify the data by running the various modification commands.
- Define one or more output formats.
- Export the resulting files.
- That's all there is to it!

---

<img align="left" width="128" alt="Logo" src="images/logo_img.png">
<br/><br/>

# TerrainFactory - Image Module

to do

---

<img align="left" width="128" alt="Logo" src="images/logo_3d.png">
<br/><br/>

# TerrainFactory - 3D Module

to do

---

<img align="left" width="128" alt="Logo" src="images/logo_mc.png">
<br/><br/>

# TerrainFactory - Minecraft Module

to do
