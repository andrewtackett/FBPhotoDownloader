# FB Photo Downloader

This program will download the highest resolution available of each of the photos you've uploaded to facebook.  Note that photos that others have uploaded in joint albums will not be downloaded as facebook's api restricts access only to the photos that you own.

# Set up

Create a file named "user.txt" in the same directory as the executable and paste in your user access token.  Make sure to grant user_photos permission.  You can generate a user access token by going [here](https://developers.facebook.com/tools/explorer) and clicking "Get Token" in the top right.

# Usage

Select the directory where you want to save pictures.  A subdirectory will be created for each album and photos from that album will be put inside.  The subdirectories and files will be named after the corresponding albums/photos, with some caveats to make Windows happy (See below).

After this just hit the "Download Photos" button to begin.  A thumbnail of each photo will be presented as it's downloaded.

# Caveats

Windows doesn't like certain characters in file or directory names, (<,>,:,",/,\,|,?,*) are just a few, and similarly it doesn't allow file paths to be longer than 260 characters.  To deal with this illegal characters will be removed and full paths longer than 260 characters will be truncated.

Photos with identical names will be given successive underscore ( _ ) characters on the end so their names can be unique.

This program currently only works for windows.  If there is any interest I may port it to other operating systems.