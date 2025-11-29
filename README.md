# Osu Beatmap Artwork Exporter
Download Osu beatmap artwork the easy way!

Automates the process of downloading beatmaps and extracting the artwork. Only keeping the files that YOU wish to keep.

 ![Multiple Artwork Demo](https://github.com/Peekaey/OsuBeatmapArtwork-Downloader/blob/master/RepoImages/MultipleArtwork1.gif).

### How it works.
1. User provides their login session cookie direct from the osu site (This can be obtained via the Network tab in the browser dev tools.)
2. User enters the beatmapset Id into the application.
3. Application will download the beatmapset in the background, load any images found in the beatmapset to memory and delete the beatmapset automatically.
4. Images are displayed to the user with the option to save any desired image in original quality.

### Features
- Download Artwork in original quality from any beatmapset available on osu.ppy.sh
- Choice of only storing cookie during runtime or saving to a json config file to be initialised on startup
- Cross platform support with Windows/MacOS (Should also work on linux but untested currently)
- Light / Dark Theme
- Easy to use!


### FAQ
- **Q:** Why do I need to provide my session cookie from the osu site?
  - **A:** As you may or not be aware, being logged into the Osu site is required to download beatmaps. There is the option of using third
  party mirror sites to download the beatmaps without being authenticated, however they do not have all the beatmaps that are currently available from official sources.
    -  There is also an official Api Endpoint that can be authenticated with OAuth to download beatmapsets which is found at the [osu!api v2 documentation](https://osu.ppy.sh/docs/index.html#get-apiv2beatmapsetsbeatmapset) , however this endpoint is currently only available to the Osu Lazer Client.
    - Once this endpoint is publicly available, the auth method can be changed to use OAuth to authenticate and download the beatmapsets.
- **Q:** Do I need to enter my session cookie each time?
- **A:** The session cookie needs to be provided each time the application is started as the cookie is not stored by default. There is an option in the settings to save this cookie 
  to a json config file for the application to read and load automatically on startup. 

Made with [Avalonia UI](https://avaloniaui.net/) & [Playwright](https://playwright.dev/).
