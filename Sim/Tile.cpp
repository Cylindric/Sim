#include "Tile.h"


Tile::Tile()
{
}

Tile::Tile(SDL_Texture* tileset, SDL_Rect* clip, int posX, int posY, bool walkable)
{
	m_tileset = tileset;
	m_rect = clip;
	m_posX = posX;
	m_posY = posY;
	m_walkable = walkable;
}


Tile::~Tile()
{
}

void Tile::update(float delta)
{

}

void Tile::draw(Graphics& graphics)
{
	graphics.renderTexture(m_tileset, m_posX, m_posY, m_rect);
}

bool Tile::isWalkable()
{
	return m_walkable;
}