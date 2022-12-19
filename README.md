# Pop2TalkNordic

Pop2Talk Nordic is forked from Pop2Talk developed by Hokema Oy. The game is licensed under GPL. Take the source and use it well, but don't forget to publish the source.


## Audio 
The audio instances for Characters and general greeting is located at: Assets\Audio Instances\Resources\(Language)
The audio wav file for cheers is located in Assets\Audio Clips\Cheers + (language code)
(Except for legacy English wav file, which is located in Assets\Word Data\Audio\2020\Instructional)

The Word Card pronunciation audio is located in Assets\Word Data\(Language folder)
The audio file (Audio Clip) is named as (language code)-(us or gb if english)_ang_wordname.wav 
I have absolutely no idea what does "ang" mean.


## Image
The Word Card image is located in Assets\Word Data\Images + (language code)
The Image file is named as art_card_wordname.jpg
The target resolution is 507x465 pixels.
The background color is #D1DCE0, or 209, 220, 224
The image is imported as Sprite (2D and UI), Wrap mode: Clamp, Max Size: 512.

## Word Data
The Word Card scriptable object (define in Assets\Scripts\Data Holders\WordData) (which is loaded for our script) is located in Assets\Word Data\Data Objects\Resources\
The scriptable object WordData will need to be manually select picture, language, spelling and pronunciations.
The scriptable object WordData name must be in the format: (language code)_(us or gb if english)_wordname. This way we can easily load different WordData in different language with just one game.
The scriptable object must be located under Resources folder, otherwise it can't be loaded with the script.


## Localisation
Addressable need to be rebuild for EACH platform after you make change.
(Windows\Asset Management\Addressables\Groups: select Play Mode Script\Use existing build, and select Build\Default Build Script)
