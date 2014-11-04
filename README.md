Loadify
=======

The Problem
-
Spotify offers a special mode, the offline mode. Using this offline mode, you are able to download tracks by storing it locally on your device for listening to music even if you don't have a connection to the internet.

Well, why would you develop a software to download tracks if you could simply do that by using the official client? In fact, you just could use the official client and use that offline mode mentioned before but it comes with a huge disadvantage: The only way to listen to downloaded tracks is to use the official Spotify client. Since Spotify encrypts their audio data, you won't be able to listen to the music you've downloaded once your Spotify premium membership expires.

The Solution
-
There isn't much to say about Loadify. It is (yet another) Spotify downloader that is open source - useable for everyone. We have provided some features that we personally found important when it comes to downloading music. But, how does it work?

Since audio files supplied by the client are encrypted, we needed to jump in a little bit earlier and thus used **libspotify**, the official Spotify library written in C (we actually use a C# wrapper that manages the transition from unmanaged to managed by marshalling). Since audio data is streamed into some type of callback once we told the API to load a certain track into the audio player, we just need to capture the data flying into this callback. The track is currently saved as `wave` file into the specified folder and then immediately gets converted to a `MP3` file.

Features
-

### User friendly, simple, beautiful
While this is not important for many other developers that did take hand on designing Spotify downloaders, it was very important for us to make using the software self-explainable and to keep things simple.

### Login and Authentication
The login is as simple as it gets. You just enter your username and password you'd normally use for logging into Spotify and click on `Login`. Please note that the _Remember me_ option works fine but requires to store your password unencrypted into the configuration file. If you can't deal with that, please don't use this feature.

![](http://i.epvpimg.com/JYQCg.png)

