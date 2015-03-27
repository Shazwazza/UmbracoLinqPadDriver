# UmbracoLinqPadDriver
A LinqPad driver for Umbraco

## IMPORTANT!!!!!

Before you proceed to start using this, here's some important things to know:

* This will not load your custom plugins, your custom events will not fire
* This does not do anything regarding the web context
* The content xml, lucene indexes, etc... will not be updated if you choose to start persisting stuff with the ApplicationContext
* Do not persist data on a live environment unless you really know what you are doing... actually don't persist stuff at all unless you know what you are doing :)
* This driver does NOT use IQueryable, all data returned from the LinqPad tree is IEnumerable, so any filtering you are doing is based on all of the results returned and then filtered in memory - It will be ultra slow if you have tons of data
     * IQueryable support will come later I've already started some POCs for that

## Installation

* Download the driver from https://github.com/Shazwazza/UmbracoLinqPadDriver/releases/tag/v1.0.0
* In LinqPad, Click "Add Connection"
* Click "View more drivers"
* Select "Browse"
* Add the UmbracoLinqPad.lpx file that you've downloaded
* Choose your Umbraco installation root folder

## Basic Usage

* Expand either Content/Media
* Right click and choose a query (i.e.Take 100)
* The query will display on the right, you can modify it and execute it

## Advanced Usage

The Umbraco LinqPad Driver's data context exposes the Umbraco ApplicationContext as 'ApplicationContext' so you can write whatever queries you want against the Umbraco services or DatabaseContext, etc... for example you could just write:

     ApplicationContext.Services.ContentService.GetRootContent()

And it will show you the root content

Since LinqPad supports c# statements, etc.. you could write full scripts and even persist things back to your database

## Known issues and limitations

Probably a lot :)

## See it in action

https://www.youtube.com/watch?feature=player_embedded&v=ypPvOQY8sF0
