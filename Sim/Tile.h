#ifndef TILE_H
#define TILE_H

#include <SDL.h>
#include "Graphics.h"

class Tile
{
public:
	Tile();
	Tile(Graphics* graphics, SDL_Texture* tileset, int offsetX, int offsetY, int width, int height, bool walkable);
	~Tile();

	bool isWalkable();
	void draw(int x, int y);

private:
	Graphics* m_graphics;
	SDL_Texture* m_tileset;
	SDL_Rect* m_rect;
	bool m_walkable;
};

#endif