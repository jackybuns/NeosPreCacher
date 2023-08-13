# NeosPreCacher

Is a tool used to download bigger files and put them into the NeosVR cache while Neos is not running to save on resources. Useful for downloading movies for example.

# Usage

It currently is only a command line application. So you need to open a terminal to execute it.
It automatically downloads [aria2](https://aria2.github.io/) on the first run. This is used for faster downloading of files.

The NeosVR cache and data directory, as well as the number of download connections can be configured in the `settings.json` file. (If the data and cache dir are empty it will fill it with the default location)

Usage is:
`NeosPreCacher.exe URL [--force]`

With `--force` the URL will be downloaded again an the entry in the Cache will be replace with the new one (useful if the file is corrupted).