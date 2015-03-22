set slnPath=%1
echo slnPath = %slnPath%

xcopy /i/y %slnPath%\UmbracoLinqPad\bin\Debug\UmbracoLinqPad.* "%programdata%\LINQPad\Drivers\DataContext\4.0\UmbracoLinqPad (977d3d694b4b0d0b)\"
xcopy /i/y %slnPath%\UmbracoLinqPad.Gateway\bin\Debug\UmbracoLinqPad.* "%programdata%\LINQPad\Drivers\DataContext\4.0\UmbracoLinqPad (977d3d694b4b0d0b)\"