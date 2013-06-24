rd OutputPackaged /s /q
md OutputPackaged

ilmerge /targetplatform:v4 /log:log.txt /out:./OutputPackaged/OsmSharp.dll "./Core/Output/Debug/Any CPU/OsmSharp.Tools.dll" "./Core/Output/Debug/Any CPU/OsmSharp.Osm.dll" "./Core/Output/Debug/Any CPU/OsmSharp.Routing.dll" "./UI/Output/OsmSharp.Osm.Map.dll" "./UI/Output/OsmSharp.Osm.Interpreter.dll" "./UI/Output/OsmSharp.Osm.Renderer.Gdi.dll" "./UI/Output/OsmSharp.Osm.UI.Model.dll" "./UI/Output/OsmSharp.Osm.UI.WinForms.dll"