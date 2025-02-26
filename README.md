# botanical-forager
I am currently working on this game. I plan to make it about discovering a procedurally generated mountain as a herbalist/alchemist and discovering new minerals and herbs. Using these materials, the player is going to experiment and brew potions to help people with illnesses and certain situations.

Currently I am developing the procedural terrain generation system. It is far from what I plan to turn it into, but these are what I have done so far:
* `Procedural land mesh` with custom noise parameters
* `Proximity chunk system` that loads chunks on different threads for optimization
* `Adjustable Level Of Detail (LOD)` system for the chunks with different distances to the player
* `Custom texture scriptable object system` that lets the user define different textures and height levels to display on the terrain
* `Triplanar shading` for the textures so same texture on different surface normals and different textures can blend seamlessly

[Link to the test build of the current terrain generation system](https://github.com/emretahtali/botanical-forager/tree/main/land%20generation%20test%20build)

---
![chunks](https://github.com/user-attachments/assets/fa89a72e-d6fb-4235-8719-787949cd6099)
![view](https://github.com/user-attachments/assets/cdfe220a-e726-486d-bb7d-89065333b833)
![texture](https://github.com/user-attachments/assets/ce7214ec-9675-4415-81ad-b4b7f6044191)
![custom](https://github.com/user-attachments/assets/83501e9f-92d0-4825-98bf-76261678a4fd)
