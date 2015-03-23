set slnPath=%1
echo slnPath = %slnPath%

rmdir /S /Q "%slnPath%\_Build\"

xcopy /i/y header.xml "%slnPath%\_Build\"
xcopy /i/y %slnPath%\UmbracoLinqPad\bin\Debug\UmbracoLinqPad.* "%slnPath%\_Build\"
xcopy /i/y %slnPath%\IQToolkit\bin\Debug\*.* "%slnPath%\_Build\"
xcopy /i/y %slnPath%\UmbracoLinqPad.Gateway\bin\Debug\UmbracoLinqPad.* "%slnPath%\_Build\"
xcopy /i/y %slnPath%\UmbracoLinqPad.Gateway\bin\Debug\LinqToAnything.* "%slnPath%\_Build\"
xcopy /i/y %slnPath%\UmbracoLinqPad.Gateway\bin\Debug\System.Linq.Dynamic.* "%slnPath%\_Build\"
xcopy /i/y %slnPath%\UmbracoLinqPad.Gateway\bin\Debug\QueryInterceptor.* "%slnPath%\_Build\"

"C:\Program Files\7-Zip\7z.exe" a -tzip "%slnPath%\_Build\UmbracoLinqPad.lpx" -w "%slnPath%\_Build\"

xcopy /i/y %slnPath%\_Build\*.* "%programdata%\LINQPad\Drivers\DataContext\4.0\UmbracoLinqPad (977d3d694b4b0d0b)\"