#!/bin/bash
nuget pack nuspec/Vernacular.Forms.nuspec -BasePath ./ -Prop Configuration=Release -Verbosity quiet
