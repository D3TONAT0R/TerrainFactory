# ASCReader
A small utility program for convertig ESRI ASCII-GRID files to various formats

This program is still in it's alpha and may not work with all files.

Features for export (so far):
  - Subsampling of the original data
  - Splitting data into multiple files
  - Export to the following formats:
    - ASC
    - XYZ point data
    - 3DS model
    - FBX model
    - PNG (Heightmaps, Normalmaps and Hillshade)
  
Upcoming:
  - Exporting a selected range of the data grid
  - Modifications to the original data, including:
    - Simple global operations for all points on the grid (add/substract/etc)
    - Modifying the values in the header of an ASC file
