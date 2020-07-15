# ASCReader
A small utility program for convertig ESRI ASCII-GRID files to various formats

This program is still in it's alpha and may not work with all files.

Notable Features (so far):

  - Subsampling of the original data (reduce resolution)
  - Splitting data into multiple files
  - Isolating a selected area of the data for export
  - Rescaling the elevation data and cell width
  - Generate a preview to quickly examine the output and determine grid coordinates for selection

Supported file types:

  - Import
    - ESRI ASCII-GRID (.asc)
    - Any image as a heightmap (.png/.jpg/.tif/.bmp/...)
    - Minecraft region files (.mca)

  - Export to
    - ASC
    - XYZ point data
    - 3DS model
    - FBX model
    - PNG (Heightmaps, Normalmaps and Hillshade)
    - Minecraft region files, including customized biome and surface generation

Upcoming:
  - Simple global operations for all points on the grid (add/substract/etc)
  - Modifying the values in the header of an ASC file
