<div align="center">
<!-- <img src="https://github.com/user-attachments/assets/dcfd6983-0568-4d2a-9e58-141c66eae52e"> -->
<h1>The Socrates Plugin</h1>  
</div>

![Unity Version](https://img.shields.io/badge/dynamic/yaml?url=https://raw.githubusercontent.com/alexzorzella/Socrates_Plugin/main/ProjectSettings/ProjectVersion.txt&query=m_EditorVersion&label=Unity&color=222c37&logo=unity)
![Latest Release](https://img.shields.io/github/v/release/alexzorzella/Socrates_Plugin)
![Issues](https://img.shields.io/github/issues/alexzorzella/Socrates_Plugin)
![GitHub License](https://img.shields.io/github/license/alexzorzella/Socrates_Plugin)
![Static Badge](https://img.shields.io/badge/Alex%20Loves-Unit%20Testing-green?logo=githubactions&logoColor=white&labelColor=%23f0ab0c)
<br>

This repository is a collection of scripts that my dad and I have worked on for years. My of them started out as tutorial scripts, but have been expanded upon. The documented scripts are the best ones, and the standard here is well documented, tested code. The status of each system's documentation and test thoroughness is listed in the 'Included' section below. The goal is to have everything tested (if applicable) and documented. I prioritize refactoring, testing, and documenting more complicated systems before simpler ones. Systems like the basic camera movement and shake, inventory, and state machine are much shorter and are intended to use as baselines. Systems that are more complicated, like the Socrates Dialogue system (which is essentially three systems in one), come first. Feel free to take and expand on these scripts as you wish! Again, a lot of these scripts are meant to be used as a base to write something more specific.

### No Attribution Required
This repository and its contents are protected by the MIT License. If you don't know what that means and don't want to read the license, it means you're free to copy, distribute, and use this material in published commercial works without attribution.

### Contributions (Use Unity 6000.0.58f2)

Contributions to the Socrates Plugin are more than welcome! If you intend on working with the Socrates Plugin's source code please do so in Unity verison 6000.0.58f2, and make sure you're not commiting any editor caches! (These are folders like .idea/ or .vscode/. If you use another editor that isn't already in the .gitignore, make sure to add it to the .gitignore before making a pull request). The editor may be upgraded in the future.

## Socrates: Dialogue Markdown Language

![socrates_markdown_gif](https://github.com/user-attachments/assets/dcfd6983-0568-4d2a-9e58-141c66eae52e)

Socrates is an open source dialogue markdown language. The entire suite of Socrates systems include

1. Socrates Text: TextMeshPro based fancy text
2. Socrates Dialogue: A component based dialogue system
3. SocraTSV: A parser that converts .tsv files written in Socrates Markdown to Socrates Dialogue

Socrates Text and Socrates Dialogue both work as standalone systems, but they come bundled. SocraTSV is written specifically to convert Socrates Markdown to Socrates Dialogue, but can still be reworked to be compatible with another dialogue system.

### Write Everything in TSV
With lots of dialogue, it's important to be able to easily manage it. Instead of writing everything as constructor variables in a script, a .tsv can just be loaded once. Not only are .tsv files easier to manage and keep track of, but localization also becomes easier.
<img width="1268" height="347" alt="image" src="https://github.com/user-attachments/assets/715ab698-0fec-44da-b32b-d7f40fa76648" />

### Tag Per Cell, Per Column, Or Do Both
To make the contents of a column default to a certain tag, specify it above! For example, all of the rows in the first column will be treated as if it was following the 'name:' tag, unless otherwise specified. In the third column, all of the rows that aren't empty will be treated as if they were following 'ref:', but any rows that have a different tag will ignore the default, i.e. 'sound:dialogue_2' will not be interpreted as 'ref:sound:dialogue_2', but as 'sound:dialogue_2'.
<img width="1398" height="373" alt="image" src="https://github.com/user-attachments/assets/aa065d97-6f75-4e9c-b1c3-5beeaea478f3" />

### Use Variables
<img width="231" height="106" alt="image" src="https://github.com/user-attachments/assets/730dcb2d-96ba-4d85-84ac-a579a21fe078" />

#### Quick Reference
##### SocraTSV
`ref:REFERENCE` indicates that the line can be referenced via REFERENCE. This is used along with the 'next' and 'option' tags.<br>
`name:NAME` specifies that line's speaker's name is NAME. It will appear in the name text box when that line is displayed.<br>
`content:CONTENT` specifies that the line's content is CONTENT. It's the dialogue.<br>
`sound:SOUND_NAME` will play a sound named SOUND_NAME.<br>
`next:REFERENCE` specifies the next line in the conversation. By default, the next line will be the next line in the file, unless that line is empty, in which case the dialogue will end. Specifying nothing after 'next:' will make the dialogue end after that line.<br>
`option:TEXT,REFERENCE` will add an option with the label 'TEXT' that leads to the dialogue 'REFERENCE'. By using multiple of these, the conversation can branch.<br>
`event:FUNC_NAME(PARAMS)` will send out a signal identified with the event tag FUNC_NAME and parameters PARAMS. You must parse and handle these in a script.<br>
Anything in curly braces will be looked up from a token/value table specified in the file. For example, if there's a table like the one above, writing {man} anywhere will be referencing Rahul. If you reference a token that doesn't exist, it just won't replace it, i.e. 'name:{nonexistenttoken}' will just set the speaker's name to '{nonexistenttoken}'.<br>

##### Socrates Markdown
While writing the names and contents of dialogue, the following tags can be used to add flavor:<br>
`[wave,INT]This text will wave![!wave]` will make the surrounded text wave with the passed INT overriding the default amplitude.<br>
`[shake,INT]This text will shake![!shake]` will make the surrounded text shake with the passed INT overriding the default intensity.<br>
`This text is displayed before the delay...[delay,FLOAT][!delay] and this text is displayed FLOAT seconds after!` will pause the text as it appears for FLOAT seconds.<br>
`[gradient]This text will have a scrolling gradient specified in DialogueGradients.cs![!gradient]` will make the surrounded text have a scrolling gradient (which by default is a rainbow).<br>
Socrates Markdown works with [TextMeshPro Rich Text](https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/manual/RichText.html).

### Customize Everything
<img width="893" height="369" alt="socrates_annotation" src="https://github.com/user-attachments/assets/6222e6a7-b82b-4829-b71c-9a3e0ef079ff" />

## Included

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
