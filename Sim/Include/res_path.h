#pragma once

#include <iostream>
#include <string>
#include <SDL.h>

class Resource
{
public:
	Resource();
	~Resource();

	/*
	* Get the resource path for resources located in res/subDir
	* It's assumed the project directory is structured like:
	* Bin/
	*  the executable
	* Resources/
	*  Project1/
	*  Project2/
	*
	* Paths returned will be Lessons/res/subDir
	*/
	static std::string getResourcePath(const std::string &subDir);

};