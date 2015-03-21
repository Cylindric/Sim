#ifndef TILE_H
#define TILE_H

#include <SDL.h>
#include "Graphics.h"
#include "GraphicsComponent.h"

class Tile
{
public:
	Tile();
	Tile(SDL_Texture* tileset, SDL_Rect* clip, int posX, int posY, bool walkable);
	~Tile();

	bool isWalkable();
	void update(float delta);
	void draw(Graphics& graphics);

private:
	GraphicsComponent m_graphicsComponent;
	SDL_Texture* m_tileset;
	SDL_Rect* m_rect;
	bool m_walkable;
	int m_posX;
	int m_posY;
};

#endif