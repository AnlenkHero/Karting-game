# 🏎️ Online Karting Game

**Multiplayer race game made with unity 6 and photon fusion 2.**

---

## About the Project

Karting Game is a multiplayer 3D racing experience built with Unity, showcasing smooth gameplay, scalable systems, and robust networking.
Using Photon Fusion 2, the game supports synchronized online play with a matchmaking system that connects players seamlessly.

Beyond just being fun to play, this project demonstrates my ability to design and implement:

Physics-based vehicle controllers

Modular UI systems adaptable to different modes

Extensible game mode architecture

Multiplayer networking and synchronization

The result is a project that highlights both gameplay design and software engineering practices, making it easy to extend with new tracks, rules, or features.

Sounds still in progress(i am not sound producer) there are system like crossfading and etc. but sounds not set up well yet.

---

## Screenshots

<img width="1581" height="880" alt="image" src="https://github.com/user-attachments/assets/42f8220a-aa84-47e1-898d-665c77b657fe" />
<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/2e02a7fa-ac2d-4280-a341-a2596d214099" />
<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/193c7231-be30-44fe-a2b9-ddc3c151184f" />
<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/df8303ac-53ee-4a9b-b940-8d311b2bfb75" />
<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/ac03fcb4-7730-4cea-8b2a-33e9ccebd294" />
<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/373e0c8b-9cb9-49f9-a818-09b963d2fbab" />

---

<a name="list-of-mechanics"></a>
## List of mechanics

|**Name**|**Description**|**Status**|
|:------:|:-------------:|:--------:|
|Semi-arcade physics based car controller|Car has a lot of physics settings including usage of wheel collider and own custom solutions.Its combined with arcade behaviour so player will have easier and funnier control.|Done|
|Steering helper(counter steering)|This systems helps player to get the car back from drifting easier. It does counter steering in the right direction so the car becomes stable.|Done|
|Matchmaking system|This systems first finds available lobby, if lobby doesnt exist then it creates new one. It waits for the max amount of players and starts. If someone leaves during starting game - search continues. Lobbies are filled by age. So old one gets priority.|Done|
|Surface system|This system allows to create custom behaviour on different surfaces. Surfaces are defined by collider zones so you shouldnt care about layers. Surface supports custom audio, particles and impacts car controls.|Done|
|Game mode strategy|It allows you to create new modes easily and separate so its easier to test. Also mode can contain different UI. Now i use simple factory instead of default. I plan to migrate to normal one so code will be even more maintainable(basically it will be better for modular ui) and i also thinking about extracting score system in separete place|Done/In progress|
|Unique main menu|The unique in it - control. Main menu stylized to car interior and uses steering wheel to select options. Information about options is transmitted to display. Also there is a trick to show settings on screen and zooming it like you watching in car|Done|
|Points system|This system takes scoring rules for every different mode. So you can score different amount of points for different mode. So it mode easier you will get less points. UI shows standings for race and then global standings. After all races done there is cutscene inspired by old mario kart game. Cutscene needs new models, effects and scene setup in general. There is also performance ranking system. Basically it takes points scored/all available points.|Done/In progress|
|Karting sound system|System not only plays car sounds, its also networked and has support of surfaces. For surfaces used crossfade behaviour, so sound transition is smooth(its controlled in surface options)|Done|
|Minimap|Minimap uses simple approach of screenshoting map from above and then you need to input map size into minimap settings. Also if your map starts not from (0x,0z) its handled and you need to input your min world points. Minimap shows strings and has possibility to show icons. Everything is networked|Done|
|Post-effects|Outline, celshading effect(not currently in use cause of low poly models), volumetric fog|Done|
|Networked UI|Game mode`s UI is networked(maybe not in the best and efficient way), Player UI networked in best and easiest way. Also there is country locator so you can hide or show your flag in settings.|Done|
