#include "Tile.h"


Tile::Tile(){

}

Tile::Tile(Graphics* graphics, SDL_Texture* tileset, int offsetX, int offsetY, int width, int height, bool walkable){
	m_graphics = graphics;
	m_tileset = tileset;
	m_rect = new SDL_Rect({ offsetX, offsetY, width, height });
	m_walkable = walkable;
}


Tile::~Tile()
{
}

void Tile::draw(int x, int y)
{
	m_graphics->renderTexture(m_tileset, x, y, m_rect);
}

bool Tile::isWalkable()
{
	return m_walkable;
}