# ImageData Extractor/Replacer
Extract or replace images from/in **.accountpicture-ms** files

## Usage:
### ImageDataExtractor
	`<executable name> <file name> <output file name (optional)>`
For example: `ImageDataExtractor.exe myFile.accountpicture-ms "extractedImage"` will create two images with names "extractedImage-96.bmp" and "extractedImage-448.bmp"

### ImageDataReplacer
	`<executable name> <file name> <image file name>`
For example: `ImageDataReplacer.exe myFile.accountpicture-ms imageToInsert.png` will create new modified .accountpicture-ms file with name "myFile_modified.accountpicture-ms"

Forked from https://github.com/Efreeto/AccountPicConverter
