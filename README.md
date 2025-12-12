# Osu Beatmap Artwork Exporter
Download Osu beatmap artwork the easy way!

Automates the process of downloading beatmapsets and extracting the artwork, displaying all the background artwork and giving you the option to copy or save any desired images.

 ![Multiple Artwork Demo](https://github.com/Peekaey/OsuBeatmapArtwork-Downloader/blob/master/RepoImages/MultipleArtwork1.gif).

### How it works.
- Option A - Download from third party mirror sites directly (Nerinyan, OsuDirect, BeatConnect)
1. User selects which mirror site(s) to download from in application settings
2. User enters the beatmapset Id or link to beatmap set into the application.
3. Application will download the beatmapset in the background, load any images found in the beatmapset to memory and delete the beatmapset automatically.
4. Images are displayed to the user with the option to copy or save any desired image in original quality.
Added Benefit is that if the beatmapset is not found in one source, will automatically retry with another source if multiple sources enabled.   

- Option B - Providing osu site cookie value and downloading from official source
1. User provides their login session cookie direct from the osu site (This can be obtained via the network tab in the browser dev tools.)
2. User enters the beatmapset Id or link to beatmap set into the application.
3. Application will download the beatmapset in the background, load any images found in the beatmapset to memory and delete the beatmapset automatically.
4. Images are displayed to the user with the option to copy or save any desired image in original quality.

### Features
- Download artwork in original quality from any beatmapset available on osu.ppy.sh or third party sources
- Supports multiple sources - Nerinyan, OsuDirect, BeatConnect, Official (with caveats)
- Choice of only storing cookie/application preferences during runtime or saving to sqlite application database
- Cross platform support with Windows/MacOS (Should also work on linux but untested currently)
- Light / Dark Theme
- Easy to use!

### How to Install
- Clone the repo
- Execute ```dotnet build``` to compile & restore nuget packages
- Execute ```./playwright.ps1 install``` to install playwright browser for application functionality (will need to install pwsh for MacOS)
- Execute ```dotnet run```

### FAQ
- **Q:** Why do I need to provide my session cookie from the osu site?
  - **A:** As you may or not be aware, being authenticated into the Osu site is required to download beatmapsets. If you do not want to provide your session cookie,
  there is the option to use the third party mirror sites instead. Only caveat is that they may not have every single beatmapset available to download compared to official sources.
    -  There is also an official Api Endpoint that can be authenticated with OAuth to download beatmapsets which is found at the [osu!api v2 documentation](https://osu.ppy.sh/docs/index.html#get-apiv2beatmapsetsbeatmapset) , however this endpoint is currently only available to the Osu Lazer Client.
    - Once this endpoint is publicly available, the auth method can be changed to use OAuth to authenticate and download the beatmapsets instead of using third party mirrors.
- **Q:** Do I need to enter my session cookie each time if i want to use official sources?
- **A:** The session cookie needs to be provided each time the application is started as the cookie is not stored by default. There is an option in the settings to save this cookie 
  to the application sqlite database which will also save the selected application preferences. 

Made with [Avalonia UI](https://avaloniaui.net/) & [Playwright](https://playwright.dev/).
