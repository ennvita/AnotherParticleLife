# AnotherParticleLife

Version 0 of another particle life simulator. This was our first foray into the world of simulations on Unity in a game context. 

This simulation uses GPU shaders to render the particles. We would welcome any and all help and optimizations! 

Thanks for checking it out!

Controls:

- Right-click: move camera
- Scroll: zoom

- Save: saves the current settings as a JSON file (under AppData\LocalLow\Vita Co\Singularity\data.json)
- Load: loads the saved settings

- Esc: exit side menu and toggle pause menu
- H/Help: toggle this help menu
- M: toggle side menu

- Reset: reset the simulation and set variables to default values
- P: reset particle positions without resetting other variables

Variables:

- R-MAX: the maximum radius at which particles have non-zero interactions
- BETA: the distance (% of R-MAX), below which forces are always repulsive
- FORCE FACTOR: the scaling term for the forces
- PARTICLE COUNT: the number of particles
- PARTICLE RADIUS: the radius of the polygons being rendered for particles
- PARTICLE VERTICES: the number of vertices in the particle polygons
- MAP WIDTH: the width of the map
- MAP HEIGHT: the height of the map
