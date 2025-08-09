# VRCMetadataStripper
This simple program find images (with any metadata) through ALL the photos in the folder (where EXE is located), and clones/overwrites the image with stripped metadata, so that the image no longer contains VRChat (as well as other) metadata.

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


Incorrect-formatted config file will be ignored.

## Reason to strip metadata?
Biggest reason behind stripping metadata from VRChat photo is to keep anonymous and help maintaining privacy.

VRChat is known for actively monitoring some types of contents, especially NSFW category. If a moderator found a certain image that contains metadata of a player's name or world's name, moderator could easily find the user and possibly take action on them.

Not only that, if the world is only accessible through URL, any photos taken in that world will going to expose world UUID, possibly allowing for someone without permission to open instances, if that image is shared on public without stripping metadata.

VRCX similarly tries to include any exposed data (who were in the instance, ETC) to the taken photo, which could possibly contain other player's UUID. This could lead to privacy violation, if that photos were shared without stripping out metadata.

## License and other information
This project is licensed under Apache License 2.0.

This project uses [ImageSharp](https://github.com/SixLabors/ImageSharp) library to process images.
