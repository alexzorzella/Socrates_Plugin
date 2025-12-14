# The Socrates Plugin

### No Attribution Required
This repository and its contents are protected by the GPL 3.0 License. If you don't know what that means and don't want to read the license, it means you're free to copy, distribute, and use this material in published commerial works without attribution.

### Contributions (Use 6000.0.58f2)

Contributions to the Socrates Plugin are more than welcome! If you intend on working with the Socrates Plugin's source code please do so in Unity verison 6000.0.58f2, and make sure you're not commiting any editor caches! (These are folders like .idea/ or .vscode/. If you use another editor that isn't already in the .gitignore, make sure to add it to the .gitignore before making a pull request). The editor may be upgraded in the future.

## Scripts

This repository is a colleciton of scripts that my dad and I have worked on for years. The standard here is documented, tested code. Feel free to take and expand on these scripts as you wish! A lot of these scripts are meant to be used as a base to write something more specific.

<i>Audio Management</i>: MultiAudioSource is streamlined to make it easy to load and play audio files directly from your files.

<i>Camera Movement and Shake</i>: Basic camera movement and shake.

<i>JConsole</i>: A command console designed to make testing, debugging, and demoing eaiser.

<i>Input System</i>: Use the new input system to manage multiple controllers and local multiplayer.

<i>Inventory</i>: A basic inventory system.

<i>AlexLang</i>: Quick .tsv based localization. This is not as robust as many other localization packages out there, but it gets the job done.

<i>Physics</i>: A couple of physics bases to work off of.

<i>Practicality</i>: Various scripts to make your life easier, including (but not limited to) a script to automatically update the build version on build (found in Assets/Editor) and increment with overflow.

<i>Save System</i>: A non-encrypted saving binary for saving and loading game states.

<i>Socrates Dialogue</i>: A robust dialogue system that supports loading from .tsv. Includes text annotations including wavy text, shaky text, and delays during text scroll. Compatible with TextMeshPro rich text tags.

<i>State Machine</i>: A custom state machine framework.

<i>GameManager</i>: A static singleton instance to manage the game state.

<i>ResourceLoader</i>: Streamlines loading objects, sprites, animators, and more directly from your files.

<i>GnaTransition</i>: Streamlines transitioning scenes while running bootstrap and teardown code if necessary.
