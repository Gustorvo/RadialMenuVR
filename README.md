# RadialMenuVR
![Animation Menu S2](https://user-images.githubusercontent.com/31797378/151679206-c009dd50-0a85-435e-92c6-bcc9b33dffac.gif)
### Physically based radial menu for VR.
Physically based (driven by [numeric springs](http://allenchou.net/2015/04/game-math-precise-control-over-numeric-springing/)) interactive radial menu! This is one of my favorite hobby projects that I've been working on with so much passion. It is still in early development, but yet powerfull enought!
The main purpose is **simplicity of use** and **extensibility**.
Enjoy and have fun!
### **Disclaimer:** ***this project is in active development. Things might (and will) change!***
## Roadmap
- [X] Add animation curve support
- [ ] Add hand tracking support
- [ ] Add velocity base sound system
- [ ] Add support for nested menu (items)
- [ ] Add state machine/state controller

## Preface:
When dealing with dynamic interactions (more often seen in VR), things must look natural and intuitive, like they do in real life.
One big advantage of numeric springing over animation curves is that it can be dynamic and interactive. For instance, when the springing simulation has not completely come to a stop, and you poke the system (modify the target value or velocity) based on user input, the system can handle it gracefully with numeric springing and everything looks natural. On the other hand, it’s usually hard to interrupt an animation using animation curves and have it animate to a new target value without making it look visually jarring.
