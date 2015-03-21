#pragma once

#include "Sprite.h"
#include "Graphics.h"

class GraphicsComponent
{
public:
	GraphicsComponent();
	~GraphicsComponent();

	void draw(Sprite& sprite, int x, int y, Graphics& graphics);
};
