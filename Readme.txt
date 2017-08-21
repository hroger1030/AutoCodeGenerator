A quick note from me to you...

   I spent a number of years writing and using this code, it seems a waste to not share it with anyone who might benefit from it. I am sharing this under the MIT license, so you can do pretty much anything with it. It would be nice if you wanted to share any code additions or bug fixes.

   Code generation is a technique that has been around for awhile. The idea is pretty cool, you design a code pattern, test it thoroughly and then reuse it to generate bug free code. There are several ways of defining your code patterns. For this application, I chose to use a SQL DB schema for the ORM and DAL objects. The tool also allows you to auto-generate SQL stored procs, and even some front end asp.net web pages. (although they are a little out of date, at some point I will probably move the UI code to an MVC pattern to be a little more contemporary)

   To use it, point it at a SQL DB, and then select the objects that you wish to generate and click the button. The files will be spit out in a subdirectory and you can add them to your project. If you make any changes to your SQL schema, regenerate the files and your code will reflect the changes.

   The ORM and DAL objects are pretty cool, as they are wired up so you can just include the DAL project that is part of the code generator project in your own project and it becomes very easy to pull back strongly typed generic collections from a SQL DB with just a few lines of code.

   As I mentioned, I have used this tool for a number of years, so basic functionality is pretty well tested. I expect that if you find any bugs it will be in areas involving more obscure SQU data types that I have had less call to use/test out.

   I have moved the DAL assembly into its own project, so make sure that you pull down that repository too, or else the app will not build! This is better than trying to keep two seperate copies of the DAL, updates will be more frequent.(https://github.com/MadTigger/Database.git) 

   If you use this, I'd love to hear your feed back about it good or bad. If you really love it, feel free to contribute more code. There is plenty more that could be included in the functionality.

Enjoy,
Roger Hill
globalconquestgames@gmail.com
2016
