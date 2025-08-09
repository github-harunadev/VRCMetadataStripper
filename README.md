# VRCMetadataStripper
This simple program find images (with any metadata) through ALL the photos in the folder (where EXE is located), and clones/overwrites the images.

Supports JPG, JPEG, and PNG. This program also allows scanning/processing images outside VRChat Photo folder, but unexpected issue may happen.

Supports English, Korean, and Japanese.

## Usage
1. Download latest EXE from [Releases](https://github.com/github-harunadev/VRCMetadataStripper/releases)
2. Place the EXE inside VRChat Photos folder (normally `C:\Users\your_name\Pictures\VRChat`)
3. Run the EXE. Follow the instruction.

## Configuration
When program is first launched, it will create `VRCMetadataStripper.config.json` file to store configuration. It's JSON formed, and can be easily be edited with any text editor.
- `language`: string, supports `en` (English), `ko` (Korean), and `ja` (Japanese).
- `skipCheckVRChatPhoto`: bool, supports `true`, `false`
  - If this value is `true`, program will ignore prefix `VRChat_` (indicates photo taken in VRChat) and will register ALL image files.
- `skipCheckVRChatDirectory`: bool, supports `true`, `false`
  - If this value is `true`, program will not check whether itself EXE file is located inside VRChat folder or not.
- `overwriteExistingPhoto`: bool, supports `true`, `false`
  - If this value is `true`, program will overwrite instead of creating a clone.
	- *You have been warned, there's no way to invert changes. _You have been warned twice._*


Incorrect-formatted config file will be ignored by program.

## License and other information
This project is licensed under Apache License 2.0.

This project uses [ImageSharp](https://github.com/SixLabors/ImageSharp) library to process images.