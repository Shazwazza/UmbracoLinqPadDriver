# A LinqPad driver for Umbraco

This allows you to easily query data in an Umbraco installation. All you need to do is add a LinqPad connection and point it to your Umbraco folder! 
Then you can easily query against media and content but best of all it allows you to run scripts against an Umbraco installation using your 
favorite Umbraco Service APIs. 

__What a demo?__ https://www.youtube.com/watch?feature=player_embedded&v=ypPvOQY8sF0

* _v2.0.0 supports Umbraco 7.7+_
* _v1.0.0 supports 7.2+ and < 7.5_

## Installation

* Download the driver from https://github.com/Shazwazza/UmbracoLinqPadDriver/releases/tag/v2.0.0
* In LinqPad, Click "Add Connection"
* Click "View more drivers"
* Select "Browse"
* Add the UmbracoLinqPad.lpx file that you've downloaded
* Choose your Umbraco installation root folder

## Basic Usage

* Expand either Content/Media
* Right click and choose a query (i.e.Take 100)
* The query will display on the right, you can modify it and execute it

## Scripting

The Umbraco LinqPad Driver's data context exposes the Umbraco ApplicationContext as 'ApplicationContext' 
so you can write whatever queries you want against the Umbraco services or DatabaseContext, etc... for example you could just write:

```cs
ApplicationContext.Services.ContentService.GetRootContent()
```

And it will show you the root content

Since LinqPad supports c# statements, etc.. you could write full scripts and even persist things back to your database

## IMPORTANT Notes

Here's some important things to know:

* This will not load your custom plugins, your custom events will not fire
* This does not do anything regarding the web context
* The content xml, lucene indexes, etc... will not be updated if you choose to start persisting stuff with the ApplicationContext
* Do not persist data on a live environment unless you really know what you are doing... actually don't persist stuff at all unless you know what you are doing :)
* This driver does NOT use IQueryable, all data returned from the LinqPad tree is IEnumerable, so any filtering you are doing is based on all of the results returned and then filtered in memory - It will be slow if you have tons of data
     * IQueryable support will come later I've already started some POCs for that