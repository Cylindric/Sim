#pragma once

#include "Character.h"
#include "Graphics.h"

class GraphicsComponent
{
public:
	GraphicsComponent();
	~GraphicsComponent();

	void update(Character& character, Graphics& graphics);
};
