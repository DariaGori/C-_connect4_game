Install tooling
~~~
dotnet tool install --global dotnet-aspnet-codegenerator
~~~

Migration
~~~
dotnet ef migrations add InitialDbCreation --project DAL --startup-project WebApp
~~~

Update database
~~~
dotnet ef database update --project DAL --startup-project WebApp
~~~

Remove migration
~~~
dotnet ef migrations remove --project DAL --startup-project WebApp
~~~

Kill database
~~~
dotnet ef database drop --project DAL --startup-project WebApp
~~~

Install via nuget to WebApp:
Microsoft.VisualStudio.Web.CodeGeneration.Design
Microsoft.EntityFrameworkCore.Design
Microsoft.EntityFrameworkCore.SqlServer

Scaffold pages (cd into WebApp first)
~~~
dotnet aspnet-codegenerator razorpage -m GameState -outDir Pages/GameStates -dc AppDbContext -udl --referenceScriptLibraries -f
~~~