#pragma once

#include <vector>
#include "SDL.h"
#include "Graphics.h"
#include "GraphicsComponent.h"
#include "Tile.h"
#include "CharacterManager.h"

class World
{
public:
	// Constructors
	World(Graphics& graphics);
	~World();

	// Public Methods
	void update(float delta);
	void draw(Graphics& graphics);
	bool isWalkable(int x, int y);

	// Accessors
	int getWidth() { return m_tileColumns * m_tileSize; }
	int getHeight() { return m_tileRows * m_tileSize; }

private:
	// Private Methods
	void loadTiles(Graphics& graphics);

	SDL_Rect* getSpriteRect(int spriteId);

	// Private objects
	GraphicsComponent m_graphicsComp;
	SDL_Texture* m_tileset;
	std::vector<Tile> m_tiles;
	CharacterManager m_characterManager;
	int m_tileSize;
	int m_tileColumns;
	int m_tileRows;
};