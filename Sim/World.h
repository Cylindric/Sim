#pragma once

#include <vector>
#include "SDL.h"
#include "Graphics.h"
#include "res_path.h"
#include "Tile.h"
#include "Character.h"

class World
{
public:
	World(Graphics* graphics);
	~World();
	void draw();
	bool isWalkable(int x, int y);

	// Accessors
	int getWidth() { return m_tileColumns * m_tileSize; }
	int getHeight(){ return m_tileRows * m_tileSize; }

private:
	void loadSprites();
	void loadTiles();
	void loadCharacters();

	Graphics* m_graphics;
	SDL_Texture* m_tileset;
	std::vector<int>* m_tiles;
	std::vector<Tile>* m_sprites;
	int m_tileSize;
	int m_tileColumns;
	int m_tileRows;
};