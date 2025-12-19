# The Socrates Plugin

### No Attribution Required
This repository and its contents are protected by the GPL 3.0 License. If you don't know what that means and don't want to read the license, it means you're free to copy, distribute, and use this material in published commerial works without attribution.

### Contributions (Use 6000.0.58f2)

Contributions to the Socrates Plugin are more than welcome! If you intend on working with the Socrates Plugin's source code please do so in Unity verison 6000.0.58f2, and make sure you're not commiting any editor caches! (These are folders like .idea/ or .vscode/. If you use another editor that isn't already in the .gitignore, make sure to add it to the .gitignore before making a pull request). The editor may be upgraded in the future.

## Socrates: Dialogue Markdown Language

Socrates is an open source dialogue markdown language. The entire suite of Socrates systems include

1. Socrates Text: TextMeshPro based fancy text
2. Socrates Dialogue: A component based dialogue system
3. SocraTSV: A parser that converts .tsv files written in Socrates Markdown to Socrates Dialogue

Socrates Text and Socrates Dialogue both work as standalone systems, but they come bundled. SocraTSV is written specifically to convert Socrates Markdown to Socrates Dialogue, but can still be reworked to be compatible with another dialogue system.

## Included

This repository is a colleciton of scripts that my dad and I have worked on for years. The standard here is documented, tested code. The status of each system's documentation and test thoroughness is listed below. The goal is to have everything tested (if applicable) and documented. I prioritize refactoring, testing, and documenting more complicated systems before simpler ones. Systems like the basic camera movement and shake, inventory, and state machine are much shorter and are intented to use as baselines or for very basic functionality. Systems that are more complicated, like the Socrates Dialogue system (which is essentially three systems in one) comes first. Feel free to take and expand on these scripts as you wish! Again, a lot of these scripts are meant to be used as a base to write something more specific.

<i>Socrates Dialogue</i> (Fully documented and somewhat tested): A robust dialogue system that supports loading from .tsv. Includes text annotations including wavy text, shaky text, and delays during text scroll. Compatible with TextMeshPro rich text tags.

<i>Audio Management</i> (Fully documented): MultiAudioSource is streamlined to make it easy to load and play audio files directly from your files.

<i>ResourceLoader</i> (Fully documented): Streamlines loading objects, sprites, animators, and more directly from your files.

<i>GnaTransition</i> (Fully documented): Streamlines transitioning scenes while running bootstrap and teardown code if necessary.

<i>Save System</i> (Fully documented): A non-encrypted saving binary for saving and loading game states.

<i>State Machine</i> (Somewhat documented): A custom state machine framework.

<i>GameManager</i> (Somewhat documented): A static singleton instance to manage the game state.

<i>Camera Movement and Shake</i> (No documentation): Basic camera movement and shake.

<i>JConsole</i> (No documentation): A command console designed to make testing, debugging, and demoing eaiser.

<i>Input System</i> (No documentation): Use the new input system to manage multiple controllers and local multiplayer.

<i>Inventory</i> (No documentation): A basic inventory system.

<i>AlexLang</i> (No documentation): Quick .tsv based localization. This is not as robust as many other localization packages out there, but it gets the job done.

<i>Physics</i> (No documentation): A couple of physics bases to work off of.

<i>Practicality</i> (No documentation): Various scripts to make your life easier, including (but not limited to) a script to automatically update the build version on build (found in Assets/Editor) and increment with overflow.
